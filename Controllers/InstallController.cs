using Audiobooks.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Audiobooks.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstallController : Controller

    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public InstallController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }


        public IActionResult Migrate()
        {
            _context.Database.Migrate();
            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> Index()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });

            var user = new IdentityUser
            {
                UserName = "admin@audiobooks.net",
                Email = "admin@audiobooks.net",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };


            await userManager.CreateAsync(user, "P@ssw0rd!23");
            await userManager.AddToRoleAsync(user, "Admin");
            return RedirectToAction(nameof(Index), "Home");
        }
    }
}
