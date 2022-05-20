using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Tarefar.DB.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string Name { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Default = "Default";
        public const string Plus = "Plus";
    }
}
