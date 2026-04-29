using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Souq.Models;

namespace Souq.Controllers
{
    public class HomeController : Controller
    {
        private readonly SouqcomContext _db;

        public HomeController(SouqcomContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Cart()
        {
            var cartItems = _db.Carts.Include(c => c.Product).ToList();
            return View(cartItems);
        }

        public IActionResult Categories()
        {
            var cats = _db.Categories.ToList();
            return View(cats);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int qty = 1)
        {
            var cartItem = new Cart
            {
                ProductId = productId,
                Qty = qty
            };
            _db.Carts.Add(cartItem);
            _db.SaveChanges();
            return RedirectToAction("Cart");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}