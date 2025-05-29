using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_BanMayTinh.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Website_BanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FeaturedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeaturedController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(searchString) ||
                    p.Id.ToString().Contains(searchString) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(searchString)) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(searchString))
                );
            }

            int pageSize = 10; // Số sản phẩm mỗi trang
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            return View(products.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeatured(int id, string searchString, int? page)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsFeatured = !product.IsFeatured;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { searchString, page });
        }
    }
}
