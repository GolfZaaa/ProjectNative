using ProjectNative.DTOs.ProductDto;
using ProjectNative.Models;

namespace ProjectNative.Services.IService
{
    public interface IProductService
    {
        Task<List<Product>> GetProductListAsync();
        Task CreateAsync(ProductRequest request);
        Task<List<string>> GetTypeAsync();
        Task UpdateAsync(ProductRequest request);
        Task DeleteAsync(Product product);
        Task<List<Product>> SearchAsync(string name);
        Task<Product> GetByIdAsync(int id);
    }
}
