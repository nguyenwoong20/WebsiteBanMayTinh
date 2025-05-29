using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Website_BanMayTinh.Models;
using Microsoft.EntityFrameworkCore;

namespace Website_BanMayTinh.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize] // Chỉ cho phép người dùng đã đăng nhập
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của user đang đăng nhập
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng nếu chưa đăng nhập
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product) // Lấy thông tin sản phẩm
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(orders);
        }


        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId) // Chỉ cho phép xem đơn hàng của chính mình
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(); // Nếu không tìm thấy đơn hàng hoặc không thuộc user
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(order);
        }
    }
}
