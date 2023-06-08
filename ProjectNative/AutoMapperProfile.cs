using AutoMapper;
using ProjectNative.DTOs.ProductDto;
using ProjectNative.Models;

namespace ProjectNative
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, ProductRequest>();
            CreateMap<ProductRequest, Product>();


            CreateMap<Product, ProductResponse>();
            CreateMap<ProductResponse, Product>();

        }
    }
}
