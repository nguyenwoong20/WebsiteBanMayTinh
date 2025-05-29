using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        public CartSummaryViewComponent() { }
        public IViewComponentResult Invoke()
        {
            var sessionData = HttpContext.Session.GetString("cart");
            var cart = string.IsNullOrEmpty(sessionData)
            ? new List<CartItem>()
            :

            JsonConvert.DeserializeObject<List<CartItem>>(sessionData) ??
            new();

            ViewBag.TotalQuantity = cart.Sum(x => x.Quantity);
            return View();
        }
    }
}
