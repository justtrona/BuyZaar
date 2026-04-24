using Microsoft.AspNetCore.Mvc;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Assign Seller layout for authenticated users with Seller or Shopper roles
            if (User.IsInRole("Seller") || User.IsInRole("Shopper"))
            {
                ViewData["Layout"] = "_Layout.Seller";  // Seller/Shopper layout
            }
            else
            {
                ViewData["Layout"] = "_Layout.Admin";  // Admin layout
            }
        }
        else
        {
            // Use landing page layout for unauthenticated users
            ViewData["Layout"] = "_Layout";  // Landing page layout
        }

        return View();
    }
}