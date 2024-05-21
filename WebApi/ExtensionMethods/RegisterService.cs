using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Data.Seed;
using WebApi.Services.AuthService;
using WebApi.Services.EmailService;

namespace WebApi.ExtensionMethods;

public static class RegisterService
{
    public static void AddRegisterService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(configure =>
            configure.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<Seeder>();
        services.AddScoped<IEmailService,EmailService>();
        services.AddScoped<IAccountService, AccountService>();
        
        
        services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false; // must have at least one digit
                config.Password.RequireNonAlphanumeric = false; // must have at least one non-alphanumeric character
                config.Password.RequireUppercase = false; // must have at least one uppercase character
                config.Password.RequireLowercase = false;  // must have at least one lowercase character
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();
    }
}