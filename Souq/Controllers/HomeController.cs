using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Souq.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            var featuredProducts = _db.Products
                .Where(p => p.IsFeatured)
                .Take(3)
                .ToList();

            if (!featuredProducts.Any())
            {
                // Fallback if none are marked, just take the first 3
                featuredProducts = _db.Products.Take(3).ToList();
            }

            return View(featuredProducts);
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
        [Authorize]
        public IActionResult AddToCart(int productId, int qty = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = new Cart
            {
                ProductId = productId,
                Qty = qty,
                UserId = userId
            };
            _db.Carts.Add(cartItem);
            _db.SaveChanges();
            return RedirectToAction("Cart");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _db.Carts.Include(c => c.Product).Where(c => c.UserId == userId).ToList();
            if (!cartItems.Any()) return RedirectToAction("Cart");
            return View(cartItems);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(string FirstName, string LastName, string Address, string City, string Phone, string PaymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var cartItems = await _db.Carts.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync();
            if (!cartItems.Any()) return RedirectToAction("Index");

            // Create Order
            var order = new Order
            {
                UserId = userId,
                FullName = $"{FirstName} {LastName}",
                Address = Address,
                City = City,
                Phone = Phone,
                PaymentMethod = PaymentMethod,
                TotalAmount = cartItems.Sum(c => c.Qty * (c.Product?.Price ?? 0)),
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Create Order Items
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Qty,
                    Price = item.Product?.Price ?? 0
                };
                _db.OrderItems.Add(orderItem);
            }

            // Clear Cart
            _db.Carts.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            ViewBag.OrderId = order.Id;
            return View("OrderSuccess");
        }

        [HttpPost]
        [Authorize]
        public IActionResult RemoveFromCart(int cartId)
        {
            var cartItem = _db.Carts.Find(cartId);
            if (cartItem != null)
            {
                _db.Carts.Remove(cartItem);
                _db.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}