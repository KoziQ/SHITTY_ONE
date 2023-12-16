using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Entities;
using ShittyOne.Models;
using System.Data;
using System.Security.Claims;

namespace ShittyOne.Data
{
    public static class SeedDB
    {
        public static async Task Initialize(IServiceProvider provider)
        {
            var dbContext = provider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var roles = typeof(Roles).GetFields().Where(f => f.FieldType == typeof(string)).ToList();
            var existingRoles = await roleManager.Roles.ToListAsync();

            foreach (var deleteRole in existingRoles.Where(er => !roles.Any(r => r.Name == er.Name)))
            {
                await roleManager.DeleteAsync(deleteRole);
            }

            foreach (var addRole in roles.Where(r => !existingRoles.Any(er => er.Name == r.Name)))
            {
                if (!await roleManager.RoleExistsAsync(addRole.Name))
                {
                    var role = new IdentityRole<Guid> { Name = addRole.Name };
                    await roleManager.CreateAsync(role);
                    await roleManager.AddClaimAsync(role, new Claim(ClaimsIdentity.DefaultRoleClaimType, addRole.Name));
                }
            }

            var admin = await userManager.FindByNameAsync("Admin");

            if (admin == null)
            {
                admin = new User { Email = "admin@mail.ru", UserName = "Admin", EmailConfirmed = true};
                await userManager.CreateAsync(admin, "Qwerty_123");
            }

            if (!await userManager.IsInRoleAsync(admin, nameof(Roles.Admin)))
            {
                await userManager.AddToRoleAsync(admin, nameof(Roles.Admin));
                await userManager.UpdateAsync(admin);
            }

        }
    }
}
