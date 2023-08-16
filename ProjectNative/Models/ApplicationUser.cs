using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectNative.Models.CartAccount;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjectNative.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonIgnore]
        public List<Address> Addresses { get; set; }

    }
}
