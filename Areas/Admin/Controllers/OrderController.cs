using System.Drawing.Printing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string search, int? pageNumber)
        {
            int pageSize = 5; // Số lượng đơn hàng trên mỗi trang
            int currentPage = pageNumber ?? 1;// Trang hiện tại

            var orders = _context.Orders
                .Include(o => o.User)
                .AsQueryable(); // Cho phép lọc dữ liệu

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o => o.User != null &&
                    (o.User.FullName.Contains(search) || o.User.Email.Contains(search)));
            }

            int totalOrders = await orders.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            if (currentPage < 1) currentPage = 1; // ✅ Không cho nhỏ hơn 1
            if (currentPage > totalPages && totalPages > 0) currentPage = totalPages;

            var pagedOrders = await orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Search = search;
            ViewBag.TotalOrders = totalOrders;

            return View(pagedOrders);
        }

        public async Task<IActionResult> Details(int? id, int pageNumber = 1)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CurrentPage = pageNumber;

            return View(order);
        }

        public async Task<IActionResult> Delete(int? id, int pageNumber = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.CurrentPage = pageNumber;

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int pageNumber = 1)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new {pageNumber = pageNumber});      
        }

        [HttpPost]
        public IActionResult ConfirmPayment(int id, int pageNumber = 1)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            order.IsPay = true;
            _context.SaveChanges();

            TempData["Message"] = $"Đơn hàng #{id} đã được xác nhận thanh toán.";
            return RedirectToAction("Index", new {pageNumber = pageNumber});
        }
    }
}
