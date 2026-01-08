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
        public async Task<int> CreateProjectInDatabaseAsync(string userId, string projectName, string language, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentNullException(nameof(projectName));

            await using var tx = await applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                var project = new Project
                {
                    UserID = userId,
                    Name = projectName,
                    Language = language,
                    Description = description,
                    CreationDate = DateTime.UtcNow
                };

                applicationDbContext.Projects.Add(project);
                await applicationDbContext.SaveChangesAsync();

                applicationDbContext.ProjectAccesses.Add(new ProjectAccess
                {
                    ProjectID = project.ProjectID,
                    UserID = userId
                });

                await applicationDbContext.SaveChangesAsync();
                await tx.CommitAsync();

                return project.ProjectID;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                throw new Exception($"An exception occured while trying to create project/access: {ex}");
            }
        }
        public async Task<bool> UserHasAccessToProjectAsync(string userId, int projectId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            return await applicationDbContext.ProjectAccesses
                .AnyAsync(x => x.UserID == userId && x.ProjectID == projectId);
        }

        public async Task<bool> IsProjectOwnedByUser(string userId, int projectId) 
        {
            try
            {
                Project project = await GetProjectById(projectId);
                if (project.UserID == userId)
                    return true;
            }
            catch (Exception ex) 
            {
                throw new Exception($"An exception occured: {ex}");
            }
                return false;
        }
        public async Task<List<Project>> GetProjectsByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                return await applicationDbContext.Projects
                    .AsNoTracking()
                    .Where(p => p.ProjectAccesses.Any(pa => pa.UserID == userId))
                    .OrderByDescending(p => p.CreationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"An exception occured while trying to retrieve projects for user {userId}: {ex}");
            }
        }
        public string GetProjectLanguage(Project project) 
        {
            return project.Language;
        }
        public async Task<Project> GetProjectById(int projectId)
		{
            Project project = new Project();
            try
            {
                project = await applicationDbContext.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);
            }
            catch (Exception ex) 
            {
                throw new Exception($"An exception occured while trying to retrieve Project: {projectId} by its Id: {ex}");
            }
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
            else 
            {
                throw new KeyNotFoundException($"User with Id: {userId} doesn't own a project with Id {projectId}");
            }
        }
        public async Task DeleteProjectFromDatabaseAsync(string userId, int projectId) 
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            var project = await applicationDbContext.Projects.Where(p => p.UserID == userId && p.ProjectID == projectId).FirstOrDefaultAsync();
            if (project != null)
            {
                try
                {
                    applicationDbContext.Projects.Remove(project);
                }
                catch (Exception ex) 
                {
                    throw new Exception($"An exception occured while trying to delete project with Id: {projectId} from database: {ex}");
                }
                try 
                {
                    await applicationDbContext.SaveChangesAsync();
                }
                catch(Exception ex) 
                {
                    throw new Exception($"An exception occured while trying to save changes within the database: {ex}");
                }
                
            }
            else { 
                throw new NullReferenceException(nameof(project));
            }
        }
        public async Task<bool> IsUserAdminAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;
            var level = await applicationDbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PermissionLevel)
                .FirstOrDefaultAsync();

            return level == PermissionLevel.Admin;
        }
    }
}
