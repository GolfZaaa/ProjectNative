using ProjectNative.Data;
using ProjectNative.DTOs.ProductDto;
using ProjectNative.Models;

namespace ProjectNative.Extenstions
{
    public static class ProductExtenstions
    {
        //public static IQueryable<Product> ProjectProductToProductDTO(this IQueryable<Product> query, DataContext dataContext)
        //{
        //    return query
        //        .Select(product => new Product
        //        {
        //            Id = product.Id,
        //            Name = product.Name,
        //            Type = product.Type,
        //            Price = product.Price,
        //            Description = product.Description,
        //            QuantityInStock = product.QuantityInStock,
        //            Calorie = product.Calorie,
        //            ProductImages = dataContext.ProductImages.Where(a => a.ProductId == product.Id).Select(image => ImageProductResponse.FromImageProduct(image)).ToList(),
        //        }).AsNoTracking();
        //}
    }
}
