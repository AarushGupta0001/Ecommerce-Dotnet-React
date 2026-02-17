using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.API.Models;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/orders/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var email = User.Identity!.Name;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return BadRequest(new { message = "Cart is empty" });

            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // Clear cart
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order placed successfully",
                orderId = order.Id,
                total = order.TotalAmount
            });
        }
        // GET: api/orders/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                OrderId = o.Id,
                Customer = o.User.Email,
                Total = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.Product.Name,
                    i.Quantity,
                    i.Price
                })
            });

            return Ok(result);
        }
        // PUT: api/orders/status
        [Authorize(Roles = "Admin")]
        [HttpPut("status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated" });
        }
        // GET: api/orders/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var email = User.Identity!.Name;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ToListAsync();

            if (!orders.Any())
                return Ok(new { message = "No orders found" });

            var result = orders.Select(o => new
            {
                OrderId = o.Id,
                Total = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.Product.Name,
                    i.Quantity,
                    i.Price
                })
            });

            return Ok(result);
        }

    }
}
