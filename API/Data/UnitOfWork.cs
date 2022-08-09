using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public readonly IMapper _mapper;
        public readonly IPhotoService _service;

        public UnitOfWork(DataContext context, IMapper mapper, IPhotoService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        public ILikesRepository LikesRepository => new LikeRepository(_context);
        
        public IPhotoRepository PhotoRepository => new PhotoRepository(_context,  _mapper, _service);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}