using BlazorServerApp.Services;
using BlazorServerApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
namespace BlazorServerApp.Services
{
    public class ProjectManagementService
    {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly ProjectDbService projectDbService;
        private readonly UserFileService userFileService;

        public ProjectManagementService(AuthenticationStateProvider authenticationStateProvider, ProjectDbService projectService, UserFileService userFileService)
        {
            this.authenticationStateProvider = authenticationStateProvider;
            this.projectDbService = projectService;
            this.userFileService = userFileService;
        }
        public async Task<int> CreateProjectAsync(Models.ProjectCreatedEventArgs args)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is not authenticated");
                return -1;
            }
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null or empty");
                return -1;
            }
            var projectId = await projectDbService.CreateProjectInDatabaseAsync(userId, args.FileName,args.Language);

            await userFileService.CreateDefaultProjectDirectoriesAsync(userId, projectId);

            await userFileService.CreateFile(userId, projectId, args.FileName, args.Extension);

            return projectId;
        }
        public async Task DeleteProjectAsync(int projectId)
        {
            try 
            {
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                if (user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    throw new KeyNotFoundException("The user is not authenticated");
                }
                string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId));
                }
                await projectDbService.DeleteProjectFromDatabaseAsync(userId, projectId);
                await userFileService.DeleteProjectDirectoriesAsync(userId, projectId);
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }
        public async Task<List<Project>> GetProjectsAsync() 
        {
            List<Project> projects = new List<Project>();
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentNullException(nameof(userId));
                else
                    projects = await projectDbService.GetProjectsByUserIdAsync(userId) ?? new List<Project>();

            }
            return projects;
        }
    }
}
