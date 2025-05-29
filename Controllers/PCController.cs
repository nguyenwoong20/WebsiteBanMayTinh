using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_BanMayTinh.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Website_BanMayTinh.Controllers
{
    public class PCController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PCController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        public async Task<IActionResult> Details(int id, string fromBuild, string partId)
        {
            ViewBag.PartId = partId; // Vẫn cần truyền PartId cho nút "Chọn"
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        public async Task<IActionResult> Build()
        {
            var viewModel = new PCBuildViewModel
            {
                CPUs = _context.Products.Where(p => p.Category.Name == "CPU").ToList(),
                Mainboards = _context.Products.Where(p => p.Category.Name == "Mainboard").ToList(),
                RAMs = _context.Products.Where(p => p.Category.Name == "RAM").ToList(),
                SSDs = _context.Products.Where(p => p.Category.Name == "SSD").ToList(),
                HDDs = _context.Products.Where(p => p.Category.Name == "HDD").ToList(),
                VGAs = _context.Products.Where(p => p.Category.Name == "VGA").ToList(),
                PSUs = _context.Products.Where(p => p.Category.Name == "PSU").ToList(),
                Cases = _context.Products.Where(p => p.Category.Name == "Case").ToList(),
                AirCoolers = _context.Products.Where(p => p.Category.Name == "Tản khí").ToList(),
                LiquidCoolers = _context.Products.Where(p => p.Category.Name == "Tản nước").ToList()
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Build(PCBuildViewModel viewModel)
        {
            var selectedProducts = new List<Product>
            {
                _context.Products.Find(viewModel.SelectedCPUId),
                _context.Products.Find(viewModel.SelectedMainboardId),
                _context.Products.Find(viewModel.SelectedRAMId),
                _context.Products.Find(viewModel.SelectedSSDId),
                _context.Products.Find(viewModel.SelectedHDDId),
                _context.Products.Find(viewModel.SelectedVGAId),
                _context.Products.Find(viewModel.SelectedPSUId),
                _context.Products.Find(viewModel.SelectedCaseId),
                _context.Products.Find(viewModel.SelectedAirCoolerId),
                _context.Products.Find(viewModel.SelectedLiquidCoolerId)
            }.Where(p => p != null).ToList();

            if (!selectedProducts.Any()) return RedirectToAction("Build");

            // Lấy giỏ hàng hiện tại từ Session
            var cartSession = HttpContext.Session.GetString("cart");
            var cart = string.IsNullOrEmpty(cartSession)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartSession) ?? new();

            // Thêm các linh kiện vào giỏ hàng
            foreach (var product in selectedProducts)
            {
                var item = cart.FirstOrDefault(p => p.Id == product.Id);
                if (item != null)
                    item.Quantity++;
                else
                    cart.Add(new CartItem
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = 1
                    });
            }

            // Lưu lại giỏ hàng vào Session
            HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));

            // Chuyển hướng đến giỏ hàng
            return RedirectToAction("Index", "Cart");
        }


        public IActionResult ExportToExcel(List<int> productIds)
        {
            var products = _context.Products
                .Include(p => p.Category) // Đảm bảo tải Category
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            // Tính tổng tiền
            decimal totalPrice = products.Sum(p => p.Price);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Cho phép dùng phi thương mại

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PC Configuration");

                // Header
                worksheet.Cells[1, 1].Value = "Loại linh kiện";
                worksheet.Cells[1, 2].Value = "Tên sản phẩm";
                worksheet.Cells[1, 3].Value = "Giá (VND)";
                worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Dữ liệu
                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cells[row, 1].Value = product.Category?.Name ?? "Không xác định";
                    worksheet.Cells[row, 2].Value = product.Name ?? "Không có tên";
                    worksheet.Cells[row, 3].Value = product.Price;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                    row++;
                }

                // Thêm dòng tổng tiền
                worksheet.Cells[row, 1, row, 2].Merge = true; // Gộp ô từ cột 1 đến cột 2
                worksheet.Cells[row, 1].Value = "Tổng chi phí";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 3].Value = totalPrice;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 3].Style.Font.Bold = true;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"PC_Config_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}



