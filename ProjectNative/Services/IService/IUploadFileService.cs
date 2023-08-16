namespace ProjectNative.Services.IService
{
    public interface IUploadFileService
    {
        //ตรวจสอบมีการอัพโหลดไฟล์เข้ามาหรือไม่
        bool IsUpload(IFormFile formFile);

        //ตรวจสอบนามสกุลไฟล์หรือรูปแบบที่่ต้องการ
        string Validation(IFormFile formFile);

        //อัพโหลดและส่งรายชื่อไฟล์ออกมา
        Task <string> UploadImages(IFormFile formFile);

        Task DeleteFileImages(string file);

    }
}
