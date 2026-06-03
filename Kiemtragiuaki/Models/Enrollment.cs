using System.ComponentModel.DataAnnotations;

namespace Kiemtragiuaki.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        public DateTime EnrollDate { get; set; } = DateTime.Now;

        public AspNetUser? User { get; set; }
        public Course? Course { get; set; }
    }
}
