using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
namespace BlazorServerApp.Models
{
    public enum PermissionLevel : byte 
    {
        User = 0,
        Admin = 1
    }
    public class ApplicationUser : IdentityUser
    {
        public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.User;
        public ICollection<ProjectAccess> ProjectAccesses { get; set; } = new List<ProjectAccess>();
    }
}
