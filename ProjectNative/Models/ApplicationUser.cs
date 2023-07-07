using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectNative.Models.CartAccount;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectNative.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Address> Addresses { get; set; }
    }
}
