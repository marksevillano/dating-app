using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("Cannot send message to yourself! ehe");
            
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message {
                Sender = sender,
                Recipient = recipient,
                RecipientUsername = recipient.UserName,
                SenderUsername = sender.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Cannot send message.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
            [FromQuery] MessagePaginationParams messagePaginationParams) 
        {
            messagePaginationParams.Username = User.GetUsername();

            var messages = await _unitOfWork.MessageRepository.GetMesssagesForUser(messagePaginationParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage (int messageId)
        {
            var message = await _unitOfWork.MessageRepository.GetMessage(messageId);
            var currentUsername = User.GetUsername();
            if (currentUsername != message.SenderUsername && currentUsername != message.RecipientUsername) Unauthorized("You can delete only your own sent message");
            
            if (currentUsername == message.SenderUsername) {
                message.SenderDeleted = true;
            }

            if (currentUsername == message.RecipientUsername) {
                message.RecipientDeleted = true;
            }

            if(message.SenderDeleted && message.RecipientDeleted) {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("cannot delete message");
        }
    }
}