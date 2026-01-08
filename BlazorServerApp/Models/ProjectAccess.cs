using System;

namespace BlazorServerApp.Models
{
    public class ProjectAccess
    {
        public int ProjectID { get; set; }
        public string UserID { get; set; } = string.Empty;

        public Project Project { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

    }
}
