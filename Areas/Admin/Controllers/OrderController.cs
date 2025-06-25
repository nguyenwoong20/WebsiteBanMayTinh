using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string search, string paymentMethod, string dateRange, string isPaid, int? pageNumber)
        {
            int pageSize = 5;
            int currentPage = pageNumber ?? 1;

            var orders = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Lọc theo tên hoặc email người dùng
            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    // Tìm kiếm trong thông tin User (khách hàng online)
                    (o.User != null &&
                     (o.User.FullName.Contains(search) ||
                      o.User.Email.Contains(search) ||
                      (o.User.PhoneNumber != null && o.User.PhoneNumber.Contains(search)))) ||
                    // Tìm kiếm trong thông tin Customer (khách hàng offline)
                    (o.CustomerName != null && o.CustomerName.Contains(search)) ||
                    (o.CustomerEmail != null && o.CustomerEmail.Contains(search)) ||
                    (o.CustomerPhone != null && o.CustomerPhone.Contains(search)) ||
                    // Tìm kiếm trong Notes
                    (o.Notes != null && o.Notes.Contains(search))
                );

            }

            // Lọc theo phương thức thanh toán
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                orders = orders.Where(o => o.PaymentMethod == paymentMethod);
            }

            // Lọc theo khoảng thời gian
            if (!string.IsNullOrEmpty(dateRange))
            {
                var today = DateTime.Today;

                if (dateRange == "today")
                {
                    orders = orders.Where(o => o.OrderDate.Date == today);
                }
                else if (dateRange == "week")
                {
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    orders = orders.Where(o => o.OrderDate >= startOfWeek);
                }
                else if (dateRange == "month")
                {
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    orders = orders.Where(o => o.OrderDate >= startOfMonth);
                }
            }

            // Lọc theo trạng thái thanh toán
            if (!string.IsNullOrEmpty(isPaid))
            {
                bool isPaidBool = isPaid == "true";
                orders = orders.Where(o => o.IsPay == isPaidBool);
            }

            int totalOrders = await orders.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            if (currentPage < 1) currentPage = 1;
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

            // Truyền lại dữ liệu bộ lọc cho View
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.DateRange = dateRange;
            ViewBag.IsPaid = isPaid;

            return View(pagedOrders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateOrderViewModel();

            // Có thể truyền thêm danh sách sản phẩm để chọn nếu cần
            ViewBag.Products = _context.Products.ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = _context.Products.ToList();
                return View(model);
            }

            var order = new Order
            {
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                ShippingAddress = model.ShippingAddress,
                Notes = model.Notes,
                PaymentMethod = model.PaymentMethod,
                IsPay = model.IsPay,
                OrderDate = DateTime.Now,
                UserId = null, // Khách offline
                OrderDetails = new List<OrderDetail>()
            };

            decimal total = 0;

            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });

                total += item.Quantity * product.Price;
            }

            order.TotalAmount = total;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đã tạo đơn hàng thành công!";
            return RedirectToAction("Index");
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
            return RedirectToAction(nameof(Index), new { pageNumber = pageNumber });
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
            return RedirectToAction("Index", new { pageNumber = pageNumber });
        }
    }
}
