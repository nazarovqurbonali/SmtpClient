using Microsoft.AspNetCore.Identity;

namespace WebApi.Data.Seed;

public class Seeder
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public Seeder(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> SeedRole()
    {
        var newRoles = new List<IdentityRole>()
        {
            new(Roles.SuperAdmin),
            new(Roles.Marketing),
            new(Roles.Mentor),
            new(Roles.Student),
        };

        var existing = _roleManager.Roles.ToList();
        foreach (var role in newRoles)
        {
            if (existing.Exists(e => e.Name == role.Name) == false)
            {
                await _roleManager.CreateAsync(role);
            }
        }

        return true;
    }

    public async Task<bool> SeedUser()
    {
        var existing = await _userManager.FindByNameAsync("admin");
        if (existing != null) return false;

        var identity = new IdentityUser()
        {
            UserName = "admin",
            PhoneNumber = "13456777",
            Email = "admin@gmail.com"
        };

        var result = await _userManager.CreateAsync(identity, "hello123");
        await _userManager.AddToRoleAsync(identity, Roles.SuperAdmin);
        return result.Succeeded;
    }
}