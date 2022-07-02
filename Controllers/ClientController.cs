using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ClientController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RestaurantContext db;
        public ClientController(UserManager<IdentityUser> userManager, RestaurantContext db)
        {
            _userManager = userManager;
            this.db = db;
        }

        public async Task<IActionResult> GetOrders()
        {
            var name = HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(name);
            var orders = db.Orders.Where(d => d.ClientId == user.Id).OrderByDescending(d => d.Date).ToList();
            return View(orders);
        }
        
    }
}
