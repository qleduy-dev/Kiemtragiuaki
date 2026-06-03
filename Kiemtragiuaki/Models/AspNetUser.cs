using Microsoft.AspNetCore.Identity;

namespace Kiemtragiuaki.Models
{
    public class AspNetUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
