using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Website_BanMayTinh.Extentions;
using Website_BanMayTinh.Models;
using Website_BanMayTinh.Repositories;

namespace Website_BanMayTinh.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        const string CARTKEY = "cart";



        List<CartItem> GetCart()
        {
            var session = HttpContext.Session.GetString(CARTKEY);
            return string.IsNullOrEmpty(session)
            ? new List<CartItem>()
            : JsonConvert.DeserializeObject<List<CartItem>>(session) ?? new();
        }
        void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CARTKEY, JsonConvert.SerializeObject(cart));
        }
        // GET: CartController
        public async Task<ActionResult> Index()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(cart);
        }

        [HttpPost]
        public ActionResult Index(int productId, int quantity)
        {
            var cart = GetCart();

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var cartItem = cart.FirstOrDefault(i => i.Id == productId);
            if (cartItem != null)
            {
                // Nếu có rồi, cập nhật số lượng
                cartItem.Quantity = quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới sản phẩm vào giỏ
                var product = _context.Products.Find(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        Id = productId,
                        Quantity = quantity,
                        Price = product.Price
                    });
                }
            }

            // Lưu lại giỏ hàng
            SaveCart(cart);

            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            return View(cart);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == id);
            if (item != null) item.Quantity++;
            else cart.Add(new CartItem
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            });
            SaveCart(cart);
            return RedirectToAction("Index");
        }


        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == productId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = GetCart(); // Lấy giỏ hàng đúng kiểu dữ liệu
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Select(i => new OrderDetail
            {
                ProductId = i.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CARTKEY); // Xóa giỏ hàng sau khi thanh toán
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("OrderCompleted", order.Id);
        }


        [HttpPost]
        public IActionResult AddBuildToCart([FromBody] List<CartItemDTO> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Danh sách rỗng");

            var cart = GetCart();

            foreach (var item in items)
            {
                if (item.Quantity <= 0) continue;

                var product = _context.Products.Find(item.ProductId);
                if (product == null) continue;

                var existingItem = cart.FirstOrDefault(c => c.Id == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }

            SaveCart(cart);

            return Ok(new { success = true });
        }
    }
}
