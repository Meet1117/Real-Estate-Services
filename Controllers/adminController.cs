using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_Services.db;
using Real_Estate_Services.Models;

namespace Real_Estate_Services.Controllers
{
    public class adminController : Controller
    {
        private readonly DB _context;
        private readonly IWebHostEnvironment _env;

        public adminController(DB context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Sites
        [Authorize]
        public async Task<IActionResult> Index()
        {
            
            ViewBag.totalprojects = _context.Sites.Count();
            ViewBag.Residential = _context.Sites.Count(x => x.Category == "Residential");
            ViewBag.Commercial = _context.Sites.Count(x => x.Category == "Commercial");
            return View();
        }

        public async Task<IActionResult> Projects()
        {
            IEnumerable<Sites> list = await _context.Sites.ToListAsync();
            return View(list);
        }
    }
}
