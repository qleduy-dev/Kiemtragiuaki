using System.ComponentModel.DataAnnotations;

namespace Kiemtragiuaki.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
