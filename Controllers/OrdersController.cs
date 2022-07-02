using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class OrdersController : Controller
    {
        private readonly RestaurantContext _context;
        UserManager<IdentityUser> _userManager;
        RoleManager<IdentityRole> _roleManager;

        public OrdersController(RestaurantContext context, UserManager<IdentityUser> manager, 
            RoleManager<IdentityRole> rolesManager)
        {
            _context = context;
            _userManager = manager;
            _roleManager = rolesManager;
        }
        
        [Authorize(Roles = "manager,admin")]
        [HttpGet]
        public IActionResult GetOrders()
        {
            var model = new OrderNumber()
            {
                End = DateTime.Now,
                Start = DateTime.Now.AddDays(-30)
            };
            return View(model);
        }

        [Authorize(Roles = "manager,admin")]
        [HttpPost]
        public IActionResult GetOrders([Bind("Start,End")] OrderNumber model)
        {
            var total = _context.Orders.Where(o => o.Date >= model.Start && o.Date <= model.End).Sum(o => o.TotalPrice);
            var count = _context.Orders.Count(o => o.Date >= model.Start && o.Date <= model.End);
            model.Number = count;
            model.TotalSum = total;
            if (count > 0)
                model.Average = (int)total / count;
            return View(model);
        }
        [Authorize(Roles = "manager,admin,kassa,kitchen")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Items = await _context.Items.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            if (User.Identity.IsAuthenticated)
            {
                var name = HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(name);
                var userRoles = await _userManager.GetRolesAsync(user);

                if (userRoles.Contains("admin") || userRoles.Contains("manager"))
                {
                    return View(await _context.Orders.ToListAsync());
                }
                else if(userRoles.Contains("kitchen"))
                {
                    return View(await _context.Orders.Where(o=>o.Status=="Обрабатывается").ToListAsync());
                }
                else if(userRoles.Contains("kassa"))
                {
                    return View(await _context.Orders.Where(o=>o.Status=="Приготовлен"||o.Status=="Готов к выдаче").ToListAsync());
                }
            }
            return View();
        }

        [Authorize(Roles = "manager,admin,kitchen")]
        public IActionResult OrderCooked(int orderId)
        {
            var orders = _context.Orders.Where(o => o.Id == orderId).ToList();
            
            if (orders.Any())
            {
                Order order=orders[0];
                order.Status = "Приготовлен";
                _context.Entry(order).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();


        }
        [Authorize(Roles = "manager,admin,kassa")]
        public IActionResult OrderReady(int orderId)
        {
            var orders = _context.Orders.Where(o => o.Id == orderId).ToList();
            
            if (orders.Any())
            {
                Order order=orders[0];
                order.Status = "Готов к выдаче";
                _context.Entry(order).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }
        [Authorize(Roles = "manager,admin,kassa")]
        public IActionResult OrderEnd(int orderId)
        {
            var orders = _context.Orders.Where(o => o.Id == orderId).ToList();
            
            if (orders.Any())
            {
                Order order=orders[0];
                order.Status = "Завершён";
                _context.Entry(order).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public async Task<IActionResult> Create()
        {
            string cartId = GetCookie();
            if (cartId == null)
                return RedirectToAction("Index", "Home");
            var items = _context.Carts.Where(c => c.CartId == cartId).ToList();
            var order = new Order();
            var goods = new List<Item>();
            int cost = 0;
            foreach (var item in items)
            {
                var product= _context.Products.Find(item.ProductId);
                if (product == null) continue;
                if (product.Available < item.Quantity)
                {
                    item.Quantity = product.Available;
                }
                var good = new Item
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    TheProduct = product
                };
                product.Available -= item.Quantity;
                _context.Entry(product).State = EntityState.Modified;
                _context.SaveChanges();
                goods.Add(good);
                cost += product.Price * item.Quantity;
            }
            order.Items = goods;
            order.TotalPrice = cost;
            order.Status = "Обрабатывается";
            order.Date = DateTime.Now;
            order.Name = "Введите имя";
            order.LastName = "Введите фамилию";
            order.DeliveryMethod = "На кассе";
            order.Address = "Введите адрес";
            order.ClientId = "";

            if (User.Identity.IsAuthenticated)
            {
                var name = HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(name);
                order.ClientId = user.Id;

                var client = _context.Clients.Find(user.Id);
                if (client != null)
                {
                    if (client.LastName != null)
                        order.LastName = client.LastName;
                    if (client.Name != null)
                        order.Name = client.Name;
                    if (client.Address != null)
                        order.Address = client.Address;
                    if (client.CurrentDiscount > 0)
                    {
                        var discount = (client.CurrentDiscount * order.TotalPrice / 100);
                        ViewBag.Discount = discount;
                        ViewBag.Cost = order.TotalPrice;
                        order.TotalPrice -= discount;
                    }
                }
            }

            _context.Orders.Add(order);
            _context.Items.AddRange(goods);
            _context.SaveChanges();
            return View(order);
        }

        private string GetCookie()
        {
            string cartId = null;
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains("CartId"))
            {
                cartId = HttpContext.Request.Cookies["CartId"];
            }

            return cartId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,TotalPrice,Date,LastName,Name,Address,DeliveryMethod,Status")] Order order)
        {
            ModelState.Remove("LastName");
            if (ModelState.IsValid)
            {
                order.Address = "Адрес";
                order.LastName = "Фамилия";
                order.Status = "Обрабатывается";
                order.ClientId = "";
                
                _context.Entry(order).State = EntityState.Modified;
                _context.SaveChanges();

                if (User.Identity.IsAuthenticated)
                {
                    var name = HttpContext.User.Identity.Name;
                    var user = await _userManager.FindByNameAsync(name);

                    var client = _context.Clients.Find(user.Id);
                    bool is_new = false;
                    if (client == null)
                    {
                        client = new Client
                        {
                            Id = user.Id,
                            OrdersNumber = 0,
                            CurrentDiscount = 0,
                            TotalOrdersCost = 0,
                            ReviewsNumber = 0
                        };
                        is_new = true;
                    }
                    client.Address = order.Address;
                    client.Name = order.Name;
                    client.LastName = order.LastName;
                    client.OrdersNumber++;
                    client.TotalOrdersCost += order.TotalPrice;
                    order.ClientId = client.Id;
                    DiscountCalculator(client);
                    if (is_new)
                    {
                        _context.Clients.Add(client);
                    }
                    else _context.Entry(client).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                


                string cartId = GetCookie();
                if (cartId != null)
                {
                    var carts = _context.Carts.Where(c => c.CartId == cartId);
                    _context.RemoveRange(carts);
                    await _context.SaveChangesAsync();
                    HttpContext.Response.Cookies.Delete("CartId");
                }
                return RedirectToAction("Success");
            }
            return View(order);
        }

        private void DiscountCalculator(Client client)
        {
            if (client.TotalOrdersCost < 5000)
                client.CurrentDiscount = 5;
            else if (client.TotalOrdersCost > 5000 && client.TotalOrdersCost < 10000)
                client.CurrentDiscount = 10;
            else if (client.TotalOrdersCost > 10000 && client.TotalOrdersCost < 25000)
                client.CurrentDiscount = 15;
            else if (client.TotalOrdersCost > 25000)
                client.CurrentDiscount = 18;
        }

        public IActionResult Success()
        {
            ViewBag.Msg = "Ваш заказ обрабатывается";
            return View();
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var goods = _context.Items.Where(g => g.OrderId == order.Id).ToList();
            foreach (var item in goods)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null) continue;
                product.Available += item.Quantity;
                _context.Entry(product).State = EntityState.Modified;
            }
            order.Items = goods;
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}
