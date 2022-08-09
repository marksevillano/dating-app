using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IPhotoService _service;

        public PhotoRepository(DataContext context, IMapper mapper, IPhotoService service)
        {
            _context = context;
            _mapper = mapper;
            _service = service;
        }

        public int ApproveOrDisapprovePhoto(Photo photo, Boolean predicate)
        {
            if (!predicate) {
                if (photo.PublicId != null) _service.DeletePhotoAsync(photo.PublicId);
                _context.Photos.Remove(photo);
            } else {
                 photo.IsApproved = predicate;
                _context.Entry(photo).State = EntityState.Modified;
            }
           return photo.Id;
        }

        public async Task<Photo> GetPhoto(int photoId)
        {
            return await _context.Photos.Include(p => p.AppUser).SingleOrDefaultAsync(i => i.Id == photoId);
        }

        public async Task<PagedList<PhotoAdminDto>> GetUnapprovedPhotos(PaginationParams pageParams)
        {
            var query = _context.Photos
                .ProjectTo<PhotoAdminDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .AsQueryable();

            query = query.Where(p => p.IsApproved == false);
            query = query.OrderByDescending(p => p.Id);
            return await PagedList<PhotoAdminDto>.CreateAsync(
                query,
                pageParams.PageNumber, pageParams.PageSize);
        }
    }
}