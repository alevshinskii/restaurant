using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Authorize(Roles="admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {

            return View(_userManager.Users.ToList());
        }

        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if(user!=null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditRole(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if(user!=null)
            {
                var userRoles =await  _userManager.GetRolesAsync(user);
                var roles = _roleManager.Roles.ToList();
                var model = new ChangeRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    AllRoles = roles,
                    UserRoles = userRoles
                };
                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult>EditRole(string id, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);
                await _userManager.AddToRolesAsync(user, addedRoles);
                await _userManager.RemoveFromRolesAsync(user, removedRoles);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            if (await _roleManager.FindByNameAsync(name) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(name));
            }
            return RedirectToAction("GetRoles");
        }

        public IActionResult GetRoles()
        {
            var allRoles = _roleManager.Roles.ToList();
            return View(allRoles);
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role!=null)
            {
                await _roleManager.DeleteAsync(role);
                return RedirectToAction("GetRoles");
            }
            return NotFound();
        }
    }
}
