using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Souq.Models;
using System.Security.Claims;

namespace Souq.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly SouqcomContext _context;

        public WishlistController(SouqcomContext context)
        {
            _context = context;
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        // POST: Wishlist/Toggle
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existingItem != null)
            {
                _context.Wishlists.Remove(existingItem);
                TempData["Message"] = "Removed from wishlist";
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    UserId = userId,
                    ProductId = productId
                });
                TempData["Message"] = "Added to wishlist";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "Products");
        }

        // POST: Wishlist/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.Wishlists.FindAsync(id);
            if (item != null)
            {
                _context.Wishlists.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
