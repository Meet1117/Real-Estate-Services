using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_Services.db;
using Real_Estate_Services.Models;

namespace Real_Estate_Services.Controllers;

public class HomeController : Controller
{
    private readonly DB _context;
    private readonly IWebHostEnvironment _env;

    public HomeController( DB context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.Sites.AsNoTracking().ToListAsync();
        return View(list);
    }

    public IActionResult Property_Details(int id)
    {
        //get the property details by id
        var sites = _context.Sites.AsNoTracking().FirstOrDefault(x => x.Id == id);
        return View(sites);
    }

    public IActionResult Aboutus()
    {
        return View();
    }

    public async Task<IActionResult> Projects()
    {
        var list = await _context.Sites.AsNoTracking().ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Project_details(int? id)
    {
        if (id == null) return NotFound();

        var site = await _context.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
        if (site == null) return NotFound();

        return View(site);
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
