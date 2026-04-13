using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Shopper")]
    public class ShopperController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}