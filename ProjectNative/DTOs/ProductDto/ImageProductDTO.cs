
using System.Reflection;

namespace ProjectNative.DTOs.ProductDto
{
    public class ImageProductDTO
    {
        public string? Id { get; set; }
        public IFormFileCollection? FormFiles { get; set; }
        public string ProductID { get; set; }

        public static ValueTask<ImageProductDTO?> BindAsync(HttpContext context,
                                                  ParameterInfo parameter)
        {
            var file = context.Request.Form.Files;
            return ValueTask.FromResult<ImageProductDTO?>(new ImageProductDTO
            {
                Id = context.Request.Form["Id"],
                ProductID = context.Request.Form["ProductID"],
                FormFiles = file,
            });
        }


    }
}
