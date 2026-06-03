using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiemtragiuaki.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses - Tất cả người dùng đều được xem học phần
        public async Task<IActionResult> Index(string? searchString, int? categoryId)
        {
            var courses = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                courses = courses.Where(c => c.Name.Contains(keyword) || c.Lecturer.Contains(keyword));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                courses = courses.Where(c => c.CategoryId == categoryId.Value);
            }

            ViewData["Categories"] = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            ViewData["SearchString"] = searchString;
            ViewData["CategoryId"] = categoryId;

            return View(await courses.OrderBy(c => c.Name).ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([Bind("Name,Credit,Lecturer,Image,CategoryId")] Course course)
        {
            await ValidateCourseBusinessRulesAsync(course);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(course);
            }

            course.Name = course.Name.Trim();
            course.Credit = course.Credit.Trim();
            course.Lecturer = course.Lecturer.Trim();
            course.Image = NormalizeImageName(course.Image);

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm học phần thành công.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            await LoadCategoriesAsync();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Credit,Lecturer,Image,CategoryId")] Course course)
        {
            if (id != course.Id || id <= 0)
                return NotFound();

            await ValidateCourseBusinessRulesAsync(course);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(course);
            }

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
                return NotFound();

            existingCourse.Name = course.Name.Trim();
            existingCourse.Credit = course.Credit.Trim();
            existingCourse.Lecturer = course.Lecturer.Trim();
            existingCourse.Image = NormalizeImageName(course.Image);
            existingCourse.CategoryId = course.CategoryId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật học phần thành công.";
            return RedirectToAction(nameof(Details), new { id = course.Id });
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (course.Enrollments.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa học phần vì đã có sinh viên đăng ký. Hãy hủy các đăng ký liên quan trước.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa học phần thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync()
        {
            ViewData["Categories"] = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }

        private async Task ValidateCourseBusinessRulesAsync(Course course)
        {
            if (course.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == course.CategoryId.Value);
                if (!categoryExists)
                    ModelState.AddModelError(nameof(Course.CategoryId), "Danh mục không tồn tại.");
            }
        }

        private static string? NormalizeImageName(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            return Path.GetFileName(imageName.Trim());
        }
    }
}
