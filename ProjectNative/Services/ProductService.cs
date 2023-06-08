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
        private readonly DataContext _dataContex;
        private readonly IMapper _mapper;

        public ProductService(DataContext dataContex, IMapper mapper)
        {
            _dataContex = dataContex;
            _mapper = mapper;
        }

        public async Task CreateAsync(ProductRequest request)
        {
            var result = _mapper.Map<Product>(request);

            await _dataContex.Products.AddAsync(result);
            await _dataContex.SaveChangesAsync();

        }

        public async Task DeleteAsync(Product product)
        {
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
            var result = await _dataContex.Products.Include(p => p.ProductImages)
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

        public async Task UpdateAsync(ProductRequest request)
        {
            var result = _mapper.Map<Product>(request);

            _dataContex.Products.Update(result);
            await _dataContex.SaveChangesAsync();

        }
    }
}
