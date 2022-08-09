using API.Entities;

namespace API.DTOs
{
    public class PhotoAdminDto
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public bool IsMain { get; set; }

        public string PublicId { get; set; }
        
        public bool IsApproved { get; set; }
        
        public int AppUserId {get; set;}

        public string Username {get; set;}
    }
}