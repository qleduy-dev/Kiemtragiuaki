using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Kiemtragiuaki.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewData["CourseCount"] = courses.Count;
            ViewData["EnrollmentCount"] = await _context.Enrollments.CountAsync();
            ViewData["CategoryCount"] = await _context.Categories.CountAsync();

            return View(courses);
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
}
