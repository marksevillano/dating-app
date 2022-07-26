using API.Entities;
using API.Helpers;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(con => con.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            // Cleaner query and better performance when projecting to Dto before making it a List
            // This eliminates include or eager loading.
            var messages = await _context.Messages
                .Where(m => (m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                    && m.Sender.UserName == recipientUsername)
                    || (m.Sender.UserName == currentUsername
                    && m.Recipient.UserName == recipientUsername && m.SenderDeleted == false)
                )
                .OrderBy(message => message.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var unreadMessages = messages.Where(message => (message.DateRead == null || message.DateRead == DateTime.MinValue)
                && message.RecipientUsername == currentUsername).ToList();
            
            if (unreadMessages.Any()) {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }
            return messages;
        }

        public async Task<PagedList<MessageDto>> GetMesssagesForUser(MessagePaginationParams messagePaginationParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();
            query = messagePaginationParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messagePaginationParams.Username
                    && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.RecipientUsername == messagePaginationParams.Username
                    && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messagePaginationParams.Username
                    && (u.DateRead == null || u.DateRead == DateTime.MinValue)
                    && u.RecipientDeleted == false)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messagePaginationParams.PageNumber, messagePaginationParams.PageSize);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

    }
}