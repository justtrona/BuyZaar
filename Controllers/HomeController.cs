using BuyZaar.Data;
using BuyZaar.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BuyZaar.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Seed()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.Add(new Category { Name = "Electronics" });
                _context.Categories.Add(new Category { Name = "Fashion" });
                _context.SaveChanges();
            }

            return Content("Seeded successfully.");
        }

        public IActionResult Categories()
        {
            var data = _context.Categories.ToList();
            return Json(data);
        }
    }
}