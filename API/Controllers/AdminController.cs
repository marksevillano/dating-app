using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new 
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
            
            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            
            if (user == null) return NotFound("Couldn't find user.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            
            if (!result.Succeeded) return BadRequest("Failed to add to roles");
            
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            
            if (!result.Succeeded) return BadRequest("Failed to remove to roles");
            
            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhotosForModeration([FromQuery] PaginationParams pageParams)
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos(pageParams);
            Response.AddPaginationHeader(photos.CurrentPage, photos.PageSize, photos.TotalCount, photos.TotalPages);

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPut("photos-approve")]
        public async Task<ActionResult> ApprovePhotos(PhotoApproveDto photoApproveDto)
        {
            Boolean predicate = photoApproveDto.Predicate;
            
            if (photoApproveDto.PhotoIds.Length > 0 && predicate != null) {
                foreach (var photoId in photoApproveDto.PhotoIds)
                {
                    var photo = await _unitOfWork.PhotoRepository.GetPhoto(photoId);
                    var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(photo.AppUser.UserName);
                    var countApprovedPhotos = 0;
                    foreach (var p in user.Photos)
                    {
                        if (p.IsApproved == true)
                        {
                            countApprovedPhotos += 1;
                        }
                    }
                    if (countApprovedPhotos == 0) {
                        photo.IsMain = true;
                    }
                    var photoIdModified = _unitOfWork.PhotoRepository.ApproveOrDisapprovePhoto(photo, predicate);
                }
                await _unitOfWork.Complete();
                return Ok("{\"message\": \"Photos are " + (predicate ? "Approved" : "Rejected") + "\"}");
            }
            return BadRequest("No Photo Ids selected");
        }
    }
}