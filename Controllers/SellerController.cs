using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyZaar.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}