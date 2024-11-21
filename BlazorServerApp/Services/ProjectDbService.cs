using System;
using System.Threading.Tasks;
using BlazorServerApp.Data;
using BlazorServerApp.Models;
using Microsoft.EntityFrameworkCore;
namespace BlazorServerApp.Services
{
    public class ProjectDbService
    {

        private readonly ApplicationDbContext applicationDbContext;
        public ProjectDbService(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }

        public ProjectDbService()
        {
            applicationDbContext = new ApplicationDbContext();
        }

        public async Task<int> CreateProjectInDatabaseAsync(string userId, string projectName, string language, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentNullException(nameof(projectName));

            var project = new Project
            {
                UserID = userId,
                Name = projectName,
                Language = language,
                Description = description,
                CreationDate = DateTime.UtcNow
            };

            applicationDbContext.Add(project);
            await applicationDbContext.SaveChangesAsync();

            return project.ProjectID;
        }
        public async Task<bool> IsProjectOwnedByUser(string userId, int projectId) 
        {
            Project project = await GetProjectById(projectId);
            if (project.UserID == userId)
                return true;
            else
                return false;
        }
        public async Task<List<Project>> GetProjectsByUserIdAsync(string userId) 
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            return await applicationDbContext.Projects
                .Where(p => p.UserID == userId)
                .OrderByDescending(p => p.CreationDate)
                .ToListAsync();
        }
        public async Task<Project> GetProjectById(int projectId) 
        { 
            var project = await applicationDbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} was not found");
            return project;
        }
        public async Task UpdateProjectNameInDatabaseAsync(string userId, int projectId, string newProjectName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            var project = await applicationDbContext.Projects.Where(p => p.UserID == userId && p.ProjectID == projectId).FirstOrDefaultAsync();
            if (project != null)
            {
                project.Name = newProjectName;
                await applicationDbContext.SaveChangesAsync();
            }
        }
        public async Task<int> DeleteProjectFromDatabaseAsync(string userId, int projectId) 
        {
            const int SUCCESS = 0;
            const int FAILURE = 1;
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            var project = await applicationDbContext.Projects.Where(p => p.UserID == userId && p.ProjectID == projectId).FirstOrDefaultAsync();
            if (project != null)
            {
                applicationDbContext.Projects.Remove(project);
                await applicationDbContext.SaveChangesAsync();
                return SUCCESS;
            }
            else { 
                return FAILURE;
            }
        }
    }
}
