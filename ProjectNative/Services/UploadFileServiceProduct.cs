using ProjectNative.Services.IService;

namespace ProjectNative.Services
{
    public class UploadFileServiceProduct : IUploadFileServiceProduct
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public UploadFileServiceProduct(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        public bool IsUpload(IFormFileCollection formFiles)
        {
            return formFiles != null && formFiles?.Count > 0;
        }

        public async Task<List<string>> UploadImages(IFormFileCollection formFiles)
        {
            var listFileName = new List<string>();
            //จัดการเส้นทาง
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            var uploadPath = Path.Combine(wwwRootPath, "images");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            foreach (var formFile in formFiles)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string fullName = Path.Combine(uploadPath, fileName);
                using (var stream = File.Create(fullName))
                {
                    await formFile.CopyToAsync(stream);
                }
                listFileName.Add(fileName);
            }
            return listFileName;
        }

        public string Validation(IFormFileCollection formFiles)
        {
            foreach (var file in formFiles)
            {
                if (!ValidationExtension(file.FileName))
                {
                    return "Invalid File Extension";
                }
                if (!ValidationSize(file.Length))
                {
                    return "The file is too large";
                }
            }
            return null;
        }

        public bool ValidationExtension(string filename)
        {
            string[] permittedExtensions = { ".jpg", ".png" };
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                return false;
            };
            return true;
        }

        public bool ValidationSize(long fileSize) => _configuration.GetValue<long>("FileSizeLimit") > fileSize;


        public Task DeleteFileImages(List<string> files)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            foreach (var item in files)
            {
                var file = Path.Combine("images", item);
                var oldImagePath = Path.Combine(wwwRootPath, file);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            return Task.CompletedTask;
        }
    }
}
