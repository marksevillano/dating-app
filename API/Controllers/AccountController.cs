
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken.");
            }
            using var hmac = new HMACSHA512();
            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(photo => photo.IsMain)?.Url,
                KnownAs = user.KnownAs,
                PreferenceGender = user.PreferenceGender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(user => user.UserName == loginDto.Username);
            if (user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            if (ByteArrayToString(computedHash)
                != ByteArrayToString(user.PasswordHash)) return Unauthorized("Invalid Password");

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(photo => photo.IsMain)?.Url,
                KnownAs = user.KnownAs,
                PreferenceGender = user.PreferenceGender
            };
        }

        private async Task<bool> UserExists(String username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-","");
        }
    }
}