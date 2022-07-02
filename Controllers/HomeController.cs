using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private RestaurantContext db;

        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger, RestaurantContext context)
        {
            this.logger = logger;
            db = context;
            //AddDataToDb();
        }

        private void AddDataToDb()
        {
            var categories = db.Categories.ToList();
            categories.Add(new Category(){Id=1,Name = "Бургеры"});
            categories.Add(new Category() { Id = 2, Name = "Картофель Фри" });
            var products=db.Products.ToList();
            products.Add(new Product()
            {
                Name = "Чизбургер", Available = 1000, CategoryId = 1, Description =
                    "Рубленый бифштекс из натуральной говядины на карамелизованной булочке, " +
                    "с ломтиком сыра «Чеддер», кетчупом, горчицей, луком и маринованными огурчиками.",
                Price = 53,
                ImageUrl = "~/images/products/cheeseburger.png"
            });
            products.Add(new Product()
            {
                Name = "Гамбургер",
                Available = 1000,
                CategoryId = 1,
                Description =
                    "Рубленый бифштекс из 100% говядины, приправленный солью и перцем на гриле, карамелизованная " +
                    "булочка с кетчупом, горчицей, луком и маринованным огурчиком.",
                Price = 51,
                ImageUrl = "~/images/products/hamburger.png"
            });
            db.Products.AddRange(products);
            db.Categories.AddRange(categories);
            db.SaveChanges();
        }

        public IActionResult Index()
        {
            ViewBag.PopularProducts = getPopularProducts();
            return View();
        }

        private IEnumerable<Product> getPopularProducts()
        {
              var productsIds= db.Items.GroupBy(        
                  i => i.ProductId,
                  (id, quantities) => new
                  {
                      Key = id,
                      Sum=quantities.Sum(q=>q.Quantity)
                  }).OrderByDescending(i=>i.Sum).Take(4).Select(i=>i.Key).ToList();

              var popularProducts = db.Products.Where(p => productsIds.Contains(p.Id)).Select(p=>p);
              if (popularProducts.Any())
              {
                  return popularProducts.ToList();
              }
              return db.Products.Take(4).ToList();
        }

        public IActionResult Restaurants()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public IActionResult Menu(string search)
        {
            List<Product> products;
            if(!String.IsNullOrEmpty(search))
            {
                products = db.Products.Include(product => product.Category ).Where(p=>p.Name.ToLower().Contains(search.ToLower())).ToList();
            }
            else
            {
                products = db.Products.Include(product => product.Category ).ToList();

            }

            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}