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
                : JsonConvert.DeserializeObject<List<CartItem>>(session) ?? new List<CartItem>();
        }

        void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CARTKEY, JsonConvert.SerializeObject(cart));
        }

        // GET: CartController
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            if (quantity > product.Stock)
            {
                TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.Id == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Id = productId,
                    Name = product.Name,
                    Quantity = quantity,
                    Price = product.Price
                });
            }

            SaveCart(cart);
            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return NotFound();
            }

            if (product.Stock <= 0)
            {
                TempData["Error"] = $"Sản phẩm {product.Name} đã hết hàng.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == id);
            if (item != null)
            {
                if (item.Quantity + 1 > product.Stock)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                    return RedirectToAction("Index");
                }
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });
            }

            SaveCart(cart);
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Index");
            }

            if (quantity > product.Stock)
            {
                TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.Id == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
                TempData["Success"] = "Đã cập nhật số lượng.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();

            // Kiểm tra tồn kho trước khi cho Checkout
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.Id);
                if (product == null)
                {
                    TempData["Error"] = $"Sản phẩm với ID {item.Id} không còn tồn tại.";
                    return RedirectToAction("Index");
                }
                if (item.Quantity > product.Stock)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho. Vui lòng cập nhật lại số lượng.";
                    return RedirectToAction("Index");
                }
            }

            // Nếu hợp lệ, sang trang Checkout
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order, IFormFile? ReceiptImage, string PaymentMethod)
        {
            if (!ModelState.IsValid)
            {
                // Gửi lại view cùng với model để hiển thị lỗi
                return View(order);
            }

            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            // Kiểm tra tồn kho
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.Id);
                if (product == null)
                {
                    TempData["Error"] = $"Sản phẩm với ID {item.Id} không còn tồn tại.";
                    return RedirectToAction("Index");
                }

                order.PaymentMethod = PaymentMethod;

                if (PaymentMethod == "BankTransfer" && ReceiptImage != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ReceiptImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReceiptImage.CopyToAsync(stream);
                    }

                    order.ReceiptImagePath = "/uploads/" + fileName;
                }

                if (product.Stock < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                    return RedirectToAction("Index");
                }
            }

            // Lưu đơn hàng
            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = new List<OrderDetail>(); // Khởi tạo danh sách
            foreach (var item in cart)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.Id,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            // Giảm tồn kho
            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.Id);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _context.Update(product);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng
            SaveCart(new List<CartItem>());
            TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: {order.Id}";

            return View("OrderCompleted", order.Id);
        }


        [HttpPost]
        public async Task<IActionResult> AddBuildToCart([FromBody] List<CartItemDTO> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Danh sách rỗng");

            var cart = GetCart();

            foreach (var item in items)
            {
                if (item.Quantity <= 0) continue;

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    TempData["Error"] = $"Sản phẩm với ID {item.ProductId} không tồn tại.";
                    continue;
                }

                if (item.Quantity > product.Stock)
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                    continue;
                }

                var existingItem = cart.FirstOrDefault(c => c.Id == item.ProductId);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + item.Quantity > product.Stock)
                    {
                        TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.Stock} cái trong kho.";
                        continue;
                    }
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