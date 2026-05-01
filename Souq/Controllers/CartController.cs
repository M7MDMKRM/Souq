using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Souq.Models;

namespace Souq.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly SouqcomContext _context;

        public CartController(SouqcomContext context)
        {
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Check if item already in cart
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Qty++;
                _context.Update(cartItem);
            }
            else
            {
                cartItem = new Cart
                {
                    ProductId = productId,
                    UserId = userId,
                    Qty = 1
                };
                _context.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.Carts.FindAsync(id);
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQty
        [HttpPost]
        public async Task<IActionResult> UpdateQty(int id, int qty)
        {
            var cartItem = await _context.Carts.FindAsync(id);
            if (cartItem != null && qty > 0)
            {
                cartItem.Qty = qty;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
