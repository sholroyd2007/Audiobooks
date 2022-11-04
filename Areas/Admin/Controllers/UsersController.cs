using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiobooks.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class UsersController : Controller
    {
        public UsersController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public UserManager<IdentityUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }

        public async Task<IActionResult> CreateRoles()
        {
            var createRoleResult = await RoleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            var addToRoleResult = await UserManager.AddToRoleAsync(await UserManager.FindByNameAsync("admin@audiobooks.app"), "Admin");
            return RedirectToAction("Index", "Home", new { area="" });


        }
    }
}
