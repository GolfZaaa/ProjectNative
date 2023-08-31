using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Models.ReviewProduct;
using System.Reflection.Emit;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ProjectNative.Data
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        public DataContext(DbContextOptions options,IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.UseSqlServer("Server=DESKTOP-DTGB06O\\SQLEXPRESS; Database=ProjectNativeSummer; Trusted_connection=true; TrustServerCertificate=true");
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DatabaseConnect"));

        }

        //สร้างข้อมูลเริ่มต้นให้กับ Role
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole { Name = "Member", NormalizedName = "MEMBER" },
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" }
            );

            builder.Entity<Product>()
               .HasData(
               new Product { Id = 1, Name = "Product01", Price = 10, QuantityInStock = 1, Description = "Test", Type = "food" },
               new Product { Id = 2, Name = "Product02", Price = 10, QuantityInStock = 1, Description = "Test", Type = "food" }
            );
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}
