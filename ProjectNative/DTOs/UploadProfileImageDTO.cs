namespace ProjectNative.DTOs
{
    public class UploadProfileImageDTO
    {
        public string userId { get; set; }
        public IFormFile? ProfileImage { get; set; }

    }
}
