using System.IO;
using System.Threading.Tasks;
using BlazorServerApp.Services;

namespace BlazorServerApp.Services
{
    public class UserFileService
    {
        private readonly string BASE_PATH;
        private readonly ProjectDbService projectDbService;
        public UserFileService(IConfiguration configuration, ProjectDbService projectDbService) 
        {
            BASE_PATH = configuration["FileStorage:Basepath"] ?? "D:/CompilerApp/StoredFiles";
            this.projectDbService = projectDbService ?? throw new ArgumentNullException(nameof(projectDbService));
        }
        public async Task CreateDefaultProjectDirectoriesAsync(string userId, int projectId) 
        {
            if(string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            string projectDirectoryPath = GetProjectDirectoryPath(userId, projectId);

            try
            {
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
            }
            catch (Exception ex) 
            {
                throw ex;
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
          
            return Path.Combine(BASE_PATH, userDirectoryName, projectDirectoryName); ;
        }
        public async Task SaveFileContentsAsync(string fileName, string fileContent, string userId, int projectId) 
        {
            try
            {
                if (await projectDbService.IsProjectOwnedByUser(userId, projectId))
                {
                    List<String> fileNames = await GetProjectFileNames(userId, projectId);
                    if (fileNames.Contains(fileName))
                    {
                        const string CODE_DIRECTORY = "SourceCode";
                        string fullFilePath = Path.Combine(GetProjectDirectoryPath(userId, projectId),CODE_DIRECTORY,fileName) ;
                        File.WriteAllText(fullFilePath, fileContent);
                    }
                    else 
                    {
                        throw new KeyNotFoundException($"File name: {fileName}, projectId: {projectId}");
                    }
                }
                else 
                {
                    throw new KeyNotFoundException($"userId: {userId}, projectId: {projectId}");
                }
            }
            catch (Exception ex) 
            {
                throw new Exception($"An exception occured while trying to save the file contents for file: {fileName} within project with projectId: {projectId} Exception: {ex}");
            }
        }
        public async Task<List<String>> GetProjectFileNames(string userId, int projectId) 
        {
            List<String> fileNames = new List<String>();
            const string CODE_DIRECTORY = "SourceCode";
            if (await projectDbService.IsProjectOwnedByUser(userId, projectId))
            {
                string projectDirectory = Path.Combine(GetProjectDirectoryPath(userId, projectId),CODE_DIRECTORY);

                try
                {
                    if (Directory.Exists(projectDirectory))
                    {
                        var files = await Task.Run(() => Directory.GetFiles(projectDirectory));
                        foreach (var file in files)
                        {
                            string fileName = await Task.Run(() => Path.GetFileName(file));
                            fileNames.Add(fileName);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Project directory not found: {projectDirectory}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error retrieving project file names: {e.Message}");
                }

            }
            else 
            {
                Console.WriteLine($"User with UserId: {userId}  doesn't own project with ProjectId: {projectId}");
            }
            return fileNames;
        }
        public async Task<string> GetFileContentAsync(string userId,int projectId, string fileName) 
        {
            const string CODE_DIRECTORY = "SourceCode";
            if (await projectDbService.IsProjectOwnedByUser(userId, projectId))
            {
                string projectDirectoryPath = GetProjectDirectoryPath(userId, projectId);
                string filePath = Path.Combine(projectDirectoryPath,CODE_DIRECTORY, fileName);
                try
                {
                    if (File.Exists(filePath))
                    {
                        return await File.ReadAllTextAsync(filePath);
                    }
                    else
                    {
                        Console.WriteLine($"File not found {filePath}");
                        return string.Empty;
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                }
                return string.Empty;
            }
            else 
            {
                Console.WriteLine($"User with UserId: {userId}  doesn't own project with ProjectId: {projectId}");
            }
            return string.Empty;
        }
        public async Task DeleteProjectDirectoriesAsync(string userId, int projectId)
        {

            const bool DELETESUBDIRECTORIES = true;
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
            string projectDirectoryPath = GetProjectDirectoryPath(userId, projectId);
            try 
            {
                if (Directory.Exists(projectDirectoryPath))
                {
                    foreach (var file in Directory.GetFiles(projectDirectoryPath))
                    {
                        File.Delete(file);
                    }
                    await Task.Run(() => Directory.Delete(projectDirectoryPath, DELETESUBDIRECTORIES));
                }
                else
                {
                    Console.WriteLine($"Project directory not found: {projectDirectoryPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting project directory: {ex.Message}");
            }
        }
        
    }
}
