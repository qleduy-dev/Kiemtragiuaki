using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kiemtragiuaki.Data
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<AspNetUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                // Seed roles
                string[] roles = { "ADMIN", "STUDENT" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (!result.Succeeded)
                        {
                            Console.WriteLine($"Failed to create role {role}");
                        }
                    }
                }

                // Seed admin user - luôn chuẩn hóa lại để database cũ vẫn đăng nhập được
                await EnsureUserAsync(
                    userManager,
                    email: "admin@kiemtragiuaki.com",
                    password: "Admin@123456",
                    fullName: "Quản Trị Viên",
                    role: "ADMIN");

                // Seed test student user - luôn chuẩn hóa lại để database cũ vẫn đăng nhập được
                await EnsureUserAsync(
                    userManager,
                    email: "student@kiemtragiuaki.com",
                    password: "Student@123456",
                    fullName: "Sinh Viên Test",
                    role: "STUDENT");

                // Seed categories (Vietnamese)
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Lập Trình Web" },
                        new Category { Name = "Lập Trình Di Động" },
                        new Category { Name = "Khoa Học Dữ Liệu" },
                        new Category { Name = "Điện Toán Đám Mây" }
                    };
                    context.Categories.AddRange(categories);
                    await context.SaveChangesAsync();
                }

                // Seed courses (Vietnamese)
                if (!context.Courses.Any())
                {
                    var courses = new List<Course>
                    {
                        new Course
                        {
                            Name = "Phát Triển Web ASP.NET Core",
                            Credit = "3",
                            Lecturer = "Nguyễn Văn A",
                            Image = "course1.jpg",
                            CategoryId = 1
                        },
                        new Course
                        {
                            Name = "React.js Cơ Bản",
                            Credit = "3",
                            Lecturer = "Trần Thị B",
                            Image = "course2.jpg",
                            CategoryId = 1
                        },
                        new Course
                        {
                            Name = "Phát Triển Ứng Dụng iOS",
                            Credit = "4",
                            Lecturer = "Lê Văn C",
                            Image = "course3.jpg",
                            CategoryId = 2
                        },
                        new Course
                        {
                            Name = "Python Cho Khoa Học Dữ Liệu",
                            Credit = "4",
                            Lecturer = "Phạm Thị D",
                            Image = "course4.jpg",
                            CategoryId = 3
                        },
                        new Course
                        {
                            Name = "Giải Pháp Cloud Azure",
                            Credit = "3",
                            Lecturer = "Hoàng Văn E",
                            Image = "course5.jpg",
                            CategoryId = 4
                        }
                    };
                    context.Courses.AddRange(courses);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
        private static async Task EnsureUserAsync(
            UserManager<AspNetUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AspNetUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Không thể tạo user {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                var changed = false;

                if (user.UserName != email)
                {
                    user.UserName = email;
                    changed = true;
                }

                if (user.Email != email)
                {
                    user.Email = email;
                    changed = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    changed = true;
                }

                if (user.FullName != fullName)
                {
                    user.FullName = fullName;
                    changed = true;
                }

                if (user.LockoutEnd != null || user.AccessFailedCount != 0)
                {
                    user.LockoutEnd = null;
                    user.AccessFailedCount = 0;
                    changed = true;
                }

                if (changed)
                {
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Không thể cập nhật user {email}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    }
                }

                if (!await userManager.CheckPasswordAsync(user, password))
                {
                    var hasPassword = await userManager.HasPasswordAsync(user);
                    if (hasPassword)
                    {
                        var removeResult = await userManager.RemovePasswordAsync(user);
                        if (!removeResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Không thể reset mật khẩu user {email}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    var addPasswordResult = await userManager.AddPasswordAsync(user, password);
                    if (!addPasswordResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Không thể đặt mật khẩu user {email}: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Không thể gán role {role} cho {email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
        }

    }
}
