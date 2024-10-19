using System;
using System.Threading.Tasks;
using BlazorServerApp.Data;
using BlazorServerApp.Models;
using Microsoft.EntityFrameworkCore;
namespace BlazorServerApp.Services
{
    public class ProjectService
    {
        private readonly ApplicationDbContext applicationDbContext;
        public ProjectService(ApplicationDbContext applicationDbContext) 
        {
            this.applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateProjectInDatabaseAsync(string userId, string projectName, string? description = null) 
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentNullException(nameof(projectName));

            var project = new Project
            {
                UserID = userId,
                Name = projectName,
                Description = description,
                CreationDate = DateTime.UtcNow
            };
            
            applicationDbContext.Add(project);
            await applicationDbContext.SaveChangesAsync();

            return project.ProjectID;
        }
    }
}
