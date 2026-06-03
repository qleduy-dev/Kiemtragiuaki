using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kiemtragiuaki.Controllers
{
    [Authorize(Roles = "STUDENT")]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AspNetUser> _userManager;

        public EnrollmentsController(ApplicationDbContext context, UserManager<AspNetUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
                .AsNoTracking()
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            if (courseId <= 0)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
                return NotFound();

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Message"] = "Bạn đã đăng ký học phần này rồi.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký học phần thành công.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int enrollmentId)
        {
            if (enrollmentId <= 0)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId);

            if (enrollment == null)
                return NotFound();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hủy đăng ký học phần thành công.";
            return RedirectToAction(nameof(MyCourses));
        }
    }
}
