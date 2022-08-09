using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<Photo> GetPhoto(int photoId);
        Task<PagedList<PhotoAdminDto>> GetUnapprovedPhotos(PaginationParams pageParams);

        int ApproveOrDisapprovePhoto(Photo photo, Boolean predicate);

    }
}