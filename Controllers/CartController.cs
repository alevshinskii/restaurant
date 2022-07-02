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
    public class CartController : Controller
    {
        private RestaurantContext db;

        private readonly ILogger<HomeController> _logger;

        public CartController(ILogger<HomeController> logger, RestaurantContext context)
        {
            _logger = logger;
            db = context;
        }

        public IActionResult Add(int id)
        {
            string cartId;
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains("CartId"))
            {
                cartId = HttpContext.Request.Cookies["CartId"];
            }
            else
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Response.Cookies.Append("CartId", cartId);
            }
            var query = db.Carts.Where(c => c.CartId == cartId && c.ProductId == id);
            if (query.Any())
            {
                CartItem cart = query.First();
                cart.Quantity++;
                db.Entry(cart).State = EntityState.Modified;
            }
            else 
            {
                var item = new CartItem()
                {
                    ProductId = id,
                    CartId = cartId,
                    Quantity = 1
                };
                db.Carts.Add(item);
            }
            db.SaveChanges();
            return RedirectToAction("Menu", "Home");
        }

        public IActionResult Index()
        {
            string cartId = null;
            if (HttpContext.Request.Cookies.Keys.Count > 0 &&
                HttpContext.Request.Cookies.Keys.Contains("CartId"))
            {
                cartId = HttpContext.Request.Cookies["CartId"];
            }
            List<CartItem> cartList = new List<CartItem>();
            List<int> costList = new List<int>();
            if (cartId != null)
            {
                cartList = db.Carts.Where(c => c.CartId == cartId).ToList();
                int sum = 0;
                foreach (var item in cartList)
                {
                    var product= db.Products.Find(item.ProductId);
                    item.SelectProduct = product;
                    int cost = product.Price * item.Quantity;
                    sum += cost;
                    costList.Add(cost);
                }
                ViewBag.Sum = sum;
                ViewBag.Cost = costList;
            }
            ViewBag.Msg = cartList.Count == 0 ?
                "Ваша корзина пуста" : "Корзина";
            return View(cartList);
        }
        public class ChangeItemQuantityDto
        {
            public int id { get; set; }
            public int newQuantity { get; set; }
        }
        public class CartChangingResult
        {
            public int delta { get; set; }
            public int cartCount { get; set; }
            public int bookId { get; set; }
        }
        [HttpPost]
        public IActionResult ChangeItemQuantity([FromBody] ChangeItemQuantityDto dto)
        {
            var cartItem = db.Carts.Find(dto.id);
            var product = db.Products.Find(cartItem.ProductId);

            var delta = (dto.newQuantity - cartItem.Quantity) * product.Price;
            cartItem.Quantity = dto.newQuantity;
            db.Entry(cartItem).State = EntityState.Modified;
            db.SaveChanges();
            int count = db.Carts
                .Where(c => c.CartId == cartItem.CartId)
                .Sum(c => c.Quantity);

            return Json(new CartChangingResult() { delta = delta, cartCount = count, bookId = product.Id });
        }


        public IActionResult Delete(int id)
        {
            var cartItem = db.Carts.Find(id);
            db.Carts.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}