using System.ComponentModel.DataAnnotations;

namespace Kiemtragiuaki.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên học phần")]
        [StringLength(150, ErrorMessage = "Tên học phần không được vượt quá 150 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Tên file hình ảnh không được vượt quá 255 ký tự")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tín chỉ")]
        [RegularExpression(@"^([1-9]|10)$", ErrorMessage = "Số tín chỉ phải là số từ 1 đến 10")]
        public string Credit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên giảng viên")]
        [StringLength(100, ErrorMessage = "Tên giảng viên không được vượt quá 100 ký tự")]
        public string Lecturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
