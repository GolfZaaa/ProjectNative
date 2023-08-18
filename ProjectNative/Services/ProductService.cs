using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.ProductDto;
using ProjectNative.Models;
using ProjectNative.Services.IService;

namespace ProjectNative.Services
{
    public class ProductService : IProductService
    {
        private readonly IUploadFileServiceProduct _uploadFileService;
        private readonly DataContext _dataContex;
        private readonly IMapper _mapper;

        public ProductService(IUploadFileServiceProduct uploadFileService, DataContext dataContex, IMapper mapper)
        {
            _uploadFileService = uploadFileService;
            _dataContex = dataContex;
            _mapper = mapper;
        }

        public async Task<string> CreateAsync(ProductRequest request)
        {
            //อัพโหลดไฟล์
            (string errorMessage, List<string> imageNames) = await UploadImageAsync(request.FormFiles);
            if (!string.IsNullOrEmpty(errorMessage)) return errorMessage;
            var result = _mapper.Map<Product>(request);
            await _dataContex.Products.AddAsync(result);
            await _dataContex.SaveChangesAsync();
            //จัดการไฟล์ในฐานข้อมูล
            if (imageNames.Count > 0)
            {
                var images = new List<ProductImage>();
                foreach (var image in imageNames)
                {
                    images.Add(new ProductImage { ProductId = result.Id, Image = image });
                }
                await _dataContex.ProductImages.AddRangeAsync(images);
            }
            await _dataContex.SaveChangesAsync();
            return null;
        }

        public async Task<(string errorMessage, List<string> imageNames)> UploadImageAsync(IFormFileCollection formFiles)
        {
            var errorMessage = string.Empty;
            var imageNames = new List<string>();
            if (_uploadFileService.IsUpload(formFiles))
            {
                errorMessage = _uploadFileService.Validation(formFiles);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    //บันทึกลงไฟล์ในโฟลเดอร์ 
                    imageNames = await _uploadFileService.UploadImages(formFiles);
                }
            }
            return (errorMessage, imageNames);
        }



        public async Task DeleteAsync(Product product)
        {
            //ค้นหาและลบไฟล์ภาพ
            var oldImages = await _dataContex.ProductImages
                .Where(p => p.ProductId == product.Id).ToListAsync();
            if (oldImages != null)
            {
                //ลบไฟล์ใน database
                _dataContex.ProductImages.RemoveRange(oldImages);
                //ลบไฟล์ในโฟลเดอร์
                var files = oldImages.Select(p => p.Image).ToList();
                await _uploadFileService.DeleteFileImages(files);
            }
            _dataContex.Products.Remove(product);
            await _dataContex.SaveChangesAsync();
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            var result = await _dataContex.Products.Include(p => p.ProductImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);


            return result;
        }





        public async Task<List<Product>> GetProductListAsync()
        {
            var result = await _dataContex.Products.Include(x=>x.Reviews).ThenInclude(a=>a.ReviewImages).Include(p => p.ProductImages)
                .OrderByDescending(p => p.Id).ToListAsync();
            return result;
        }




        public async Task<List<string>> GetTypeAsync()
        {
            var result = await _dataContex.Products.GroupBy(p => p.Type)
    .Select(result => result.Key).ToListAsync();
            return result;
        }

        public async Task<List<Product>> SearchAsync(string name)
        {
            var result = await _dataContex.Products.Include(p => p.ProductImages)
                 .Where(p => p.Name.Contains(name))
                 .ToListAsync();
            return result;
        }

        public async Task<string> UpdateAsync(ProductRequest request)
        {
            //ตรวจสอบและอัพโหลดไฟล์
            (string errorMessage, List<string> imageNames) = await UploadImageAsync(request.FormFiles);
            if (!string.IsNullOrEmpty(errorMessage)) return errorMessage;
            var result = _mapper.Map<Product>(request);
            _dataContex.Products.Update(result);
            await _dataContex.SaveChangesAsync();
            //ตรวจสอบและจัดการกับไฟล์ที่ส่งเข้ามาใหม่
            if (imageNames.Count > 0)
            {
                var images = new List<ProductImage>();
                foreach (var image in imageNames)
                {
                    images.Add(new ProductImage { ProductId = result.Id, Image = image });
                }
                //ลบไฟล์เดิม
                var oldImages = await _dataContex.ProductImages
                    .Where(p => p.ProductId == result.Id).ToListAsync();
                if (oldImages != null)
                {
                    //ลบไฟล์ใน database
                    _dataContex.ProductImages.RemoveRange(oldImages);
                    //ลบไฟล์ในโฟลเดอร์
                    var files = oldImages.Select(p => p.Image).ToList();
                    await _uploadFileService.DeleteFileImages(files);
                }
                //ใส่ไฟล์เข้าไปใหม่
                await _dataContex.ProductImages.AddRangeAsync(images);
                await _dataContex.SaveChangesAsync();
            }
            return null;

        }
    }
}
