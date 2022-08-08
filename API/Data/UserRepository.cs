using System.Collections;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(user => user.Photos)
                .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(user => user.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
            
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .AsQueryable();
            query = query.Where(user => (user.UserName != userParams.CurrentUsername));
            // filter by current user gender preference
            if (userParams.CurrentUserGenderPreference != "both") {
                query = query.Where(user => user.Gender == userParams.CurrentUserGenderPreference);

                // filter by user's individual preference
                // example: if male user prefers female, 
                // but the female prefers female, then the user must be hidden
                // in male user's side
                // but if the female prefers both, then the user still must appear
                // in the male user's side
                if (userParams.CurrentUserGender == userParams.CurrentUserGenderPreference) {
                    query = query.Where(user => user.Gender == user.PreferenceGender || (user.Gender == userParams.CurrentUserGender && user.PreferenceGender == "both"));
                } 
                // if straight
                else {
                    query = query.Where(user => user.Gender != user.PreferenceGender || (user.Gender != userParams.CurrentUserGender && user.PreferenceGender == "both"));
                }
            }

           /* query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(user => user.Created),
                _ => query.OrderByDescending(user => user.LastActive)
            };*/

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            query = query.Where(user => user.DateOfBirth <= maxDob && user.DateOfBirth >= minDob);
            
            
            return await PagedList<MemberDto>.CreateAsync(
                query,
                userParams.PageNumber, userParams.PageSize);

        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<AppUser> GetUserGender(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(u => new AppUser { Gender = u.Gender, PreferenceGender = u.PreferenceGender })
                .FirstOrDefaultAsync();
        }
    }
}