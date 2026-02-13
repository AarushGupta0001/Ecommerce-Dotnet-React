using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ECommerce.API.Data;
using ECommerce.API.Models;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var email = User.Identity!.Name;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
                existingItem.Quantity += quantity;
            else
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });

            await _context.SaveChangesAsync();

            return Ok("Item added to cart");
        }
        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var email = User.Identity!.Name;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return Ok(new { message = "Cart is empty" });

            var result = cart.Items.Select(i => new
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity,
                Total = i.Product.Price * i.Quantity
            });

            return Ok(result);
        }

    }
}
