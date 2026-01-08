using Microsoft.AspNetCore.Identity;

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
    }
}
