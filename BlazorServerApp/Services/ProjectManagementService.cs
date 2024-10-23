using BlazorServerApp.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorServerApp.Pages;
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
        public async Task<int> CreateProjectAsync(CreateProjectModal.ProjectCreatedEventArgs args)
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
        public async Task<int> DeleteProjectAsync(int projectId)
        {
            const int SUCCESS = 0;
            const int FAILURE = 1;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is not authenticated");
                return FAILURE;
            }
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null or empty");
                return FAILURE;
            }
            var result = await projectDbService.DeleteProjectFromDatabaseAsync(userId, projectId);
            if (result == SUCCESS)
            {
                return await userFileService.DeleteProjectDirectoriesAsync(userId, projectId);
            }
            else 
            {
                return FAILURE;
            }

        }
    }
}
