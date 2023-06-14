using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectNative.Data;
using ProjectNative.Models;
using ProjectNative.Services;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using System.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });


    options.OperationFilter<SecurityRequirementsOperationFilter>();
});


builder.Services.AddDbContext<DataContext>();


#region Identityสร้างเซอร์วิส User,Role (ระวังการเรียงลำดับ) .AddDefaultTokenProviders() เพิ่มมาเพราะต้องใช้ในการเปลี่ยน email User
builder.Services.AddIdentityCore<ApplicationUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                       .AddJwtBearer(opt =>
                       {
                           opt.TokenValidationParameters = new TokenValidationParameters
                           {
                               ValidateIssuer = false,
                               ValidateAudience = false,
                               ValidateLifetime = true,
                               ValidateIssuerSigningKey = true,
                               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                                   .GetBytes(builder.Configuration["JWTSettings:TokenKey"]))
                           };
                       });


builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartUsersService, CartUsersService>();
builder.Services.AddMemoryCache();


#region SendGrid Start


// เพิ่มการกำหนดค่า SendGridClient
builder.Services.AddTransient<SendGridClient>(c =>
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    return new SendGridClient(configuration.GetSection("SendGrid:SendGridKey").Value);
});


#endregion SendGrid End



builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddAuthorization();
#endregion



var app = builder.Build();


#region  //สร้างฐานข้อมูลอัตโนมัติ
using var scope = app.Services.CreateScope(); //using หลังทำงานเสร็จจะถูกทำลายจากMemory
var context = scope.ServiceProvider.GetRequiredService<DataContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();


try
{
    await context.Database.MigrateAsync();   //สร้าง DB ให้อัตโนมัติถ้ายังไม่มี
}
catch (Exception ex)
{
    logger.LogError(ex, "Problem migrating data");
}
#endregion
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


await app.RunAsync();
