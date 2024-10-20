using System.IO;
using System.Threading.Tasks;
namespace BlazorServerApp.Services
{
    public class UserFileService
    {
        private readonly string basePath;
        public UserFileService(IConfiguration configuration) 
        {
            basePath = configuration["FileStorage:Basepath"] ?? "D:/CompilerApp/StoredFiles";
            
        }
        public async Task CreateDefaultProjectDirectoriesAsync(string userId, int projectId) 
        {
            if(string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            string projectDirectoryPath = GetProjectDirectoryPath(userId, projectId);

            if (!Directory.Exists(projectDirectoryPath)) 
            {
                Directory.CreateDirectory(projectDirectoryPath);
            }
            string[] subDirs = { "SourceCode", "Inputs", "Outputs", "Logs" };
            foreach (string subDir in subDirs) 
            {
                string subDirPath = Path.Combine(projectDirectoryPath, subDir);
                if (!Directory.Exists(subDirPath)) 
                {
                    Directory.CreateDirectory(subDirPath);
                }
            }
            await Task.CompletedTask;
        }
        public async Task CreateFile(string userId, int projectId, string fileName, string fileExtension, string directory = "") 
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            string projectDirectory = GetProjectDirectoryPath(userId,projectId);
            const string sourceCodeDirectory = "SourceCode";
            
            string fullFilePath = Path.Combine(projectDirectory,sourceCodeDirectory,directory) + "\\" + fileName + fileExtension;

            if (File.Exists(fullFilePath))
                Console.WriteLine("A file with that name already exists in this directory");    
            if (!File.Exists(fullFilePath))
                File.Create(fullFilePath);

            await Task.CompletedTask;
        }
        public string GetProjectDirectoryPath(string userId, int projectId)
        {
            string userDirectoryName = "User_" + userId;
            string projectDirectoryName = "Project_" + projectId;
            return Path.Combine(basePath, userDirectoryName, projectDirectoryName);
        }
    }
}
