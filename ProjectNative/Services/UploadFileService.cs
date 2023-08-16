using Microsoft.AspNetCore.Hosting;
using ProjectNative.Services.IService;

namespace ProjectNative.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public UploadFileService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
          
        public Task DeleteFileImages(string files)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            var file = Path.Combine("orderImage", files);
            var oldImagePath = Path.Combine(wwwRootPath, file);
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            return Task.CompletedTask;
        }
         
        public bool IsUpload(IFormFile formFiles)
        {
            return formFiles != null;
        }
         
        public async Task<string> UploadImages(IFormFile formFile)
        {
            string fileName = null;

            if (formFile != null && formFile.Length > 0)
            {
                // Handle file upload
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "orderImage");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
            }

            return fileName;
        }

        public string Validation(IFormFile formFile)
        {
            if (!ValidationExtension(formFile.FileName))
            {
                return "Invalid File Extension";
            }

            if (!ValidationSize(formFile.Length))
            {
                return "The file is too large";
            }

            return null;
        }

        public bool ValidationExtension(string filename)
        {
            string[] permittedExtensions = { ".jpg", ".png", ".jpeg" }; // สามารถเพิ่มชื่อไฟล์ได้เลย
            string extension = Path.GetExtension(filename).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                return false;
            };
            return true;
        }

        public bool ValidationSize(long fileSize)
            => _configuration.GetValue<long>("FileSizeLimit") > fileSize;
    }
}
