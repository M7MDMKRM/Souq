using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Souq.Models;

using System.Security.Claims;

namespace Souq.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly SouqcomContext _context;

        public DashboardController(SouqcomContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Admin"))
            {
                // Admin Stats
                ViewBag.TotalProducts = await _context.Products.CountAsync();
                ViewBag.TotalCategories = await _context.Categories.CountAsync();
                ViewBag.TotalReviews = await _context.Reviews.CountAsync();
                ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
                ViewBag.SalesCount = await _context.Orders.CountAsync();
                ViewBag.ReturnsCount = (int)(ViewBag.SalesCount * 0.05);
                ViewBag.PendingCount = await _context.Orders.CountAsync(o => o.Status == "Pending");
            }
            else
            {
                // Customer Stats
                ViewBag.MyOrdersCount = await _context.Orders.CountAsync(o => o.UserId == userId);
                ViewBag.MyCartCount = await _context.Carts.CountAsync(c => c.UserId == userId);
                ViewBag.MyWishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId);
                ViewBag.TotalSpent = await _context.Orders.Where(o => o.UserId == userId).SumAsync(o => o.TotalAmount);
                
                ViewBag.RecentOrders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();
            }

            return View();
        }
    }
}
