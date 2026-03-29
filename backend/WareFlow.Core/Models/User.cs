using Microsoft.AspNetCore.Identity;

namespace WareFlow.Core.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
