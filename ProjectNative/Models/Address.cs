using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectNative.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string SubDistrict { get; set; }
        public string PostalCode { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


    }
}
