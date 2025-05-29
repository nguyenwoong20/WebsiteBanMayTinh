using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> About()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    public async Task<IActionResult> Contact()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    //Show danh sách các sản phẩm nỗi bậc ra ngoài trang chủ mà không show tất cả
    public async Task<IActionResult> Featured()
    {
        // Danh sách ID các sản phẩm nổi bật bạn muốn hiển thị
        var featuredProducts = await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .Where(p => p.IsFeatured)
        .ToListAsync();

        var highlightCategories = _context.Categories // giả sử có cờ đánh dấu danh mục nổi bật
        .ToList();

        ViewBag.HighlightCategories = highlightCategories;


        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View("Featured", featuredProducts);
    }

    //Show danh sách các sản phẩm, có thể chọn theo danh mục hoặc tìm kiếm
    public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sort)
    {
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        switch (sort)
        {
            case "name-asc":
                products = products.OrderBy(p => p.Name);
                break;
            case "name-desc":
                products = products.OrderByDescending(p => p.Name);
                break;
            case "price-asc":
                products = products.OrderBy(p => p.Price);
                break;
            case "price-desc":
                products = products.OrderByDescending(p => p.Price);
                break;
            default:
                products = products.OrderBy(p => p.Id); // Hoặc mặc định nào đó
                break;
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = products.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Brand != null && p.Brand.Name.Contains(searchTerm)) ||
                (p.Category != null && p.Category.Name.Contains(searchTerm))
            );
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.SearchTerm = searchTerm;

        return View(await products.ToListAsync());
    }

    //Chi tiết sản phẩm
    public async Task<IActionResult> Details(int id)
    {
        var product = _context.Products.Include(p => p.Brand).Include(p => p.Category).FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> SearchSuggestions(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<object>());

        var query = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsQueryable();

        term = term.ToLower();

        query = query.Where(p =>
            EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
            (p.Brand != null && EF.Functions.Like(p.Brand.Name.ToLower(), $"%{term}%")) ||
            (p.Category != null && EF.Functions.Like(p.Category.Name.ToLower(), $"%{term}%"))
        );

        var suggestions = await query.Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.ImageUrl
        })
        .Take(5)
        .ToListAsync();

        return Json(suggestions);
    }   
}
