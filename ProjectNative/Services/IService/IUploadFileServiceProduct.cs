﻿namespace ProjectNative.Services.IService
{
    public interface IUploadFileServiceProduct
    {
        //ตรวจสอบมีการอัพโหลดไฟล์เข้ามาหรือไม่
        bool IsUpload(IFormFileCollection formFiles);


        //ตรวจสอบนามสกุลไฟล์หรือรูปแบบที่่ต้องการ
        string Validation(IFormFileCollection formFiles);


        //อัพโหลดและส่งรายชื่อไฟล์ออกมา
        Task<List<string>> UploadImages(IFormFileCollection formFiles);

        Task DeleteFileImages(List<string> files);
    }
}
