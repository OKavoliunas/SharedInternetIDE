using System.IO;
using System.Threading.Tasks;
using BlazorServerApp.Services;
using BlazorServerApp.Models;
namespace BlazorServerApp.Services
{
    public class UserFileService
    {
        private readonly string BASE_PATH;
        private readonly ProjectDbService projectDbService;


        private readonly IFileSystem fileSystem;
        public UserFileService(
            IConfiguration configuration,
            ProjectDbService projectDbService,
            IFileSystem fileSystem)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string? basePath = configuration["FileStorage:Basepath"];
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new InvalidOperationException("Base path is not configured");
            }
            BASE_PATH = basePath;

            if (projectDbService == null)
            {
                throw new ArgumentNullException(nameof(projectDbService));
            }
            this.projectDbService = projectDbService;

            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            this.fileSystem = fileSystem;
        }
        public interface IFileSystem
        {
            void CreateDirectory(string path);
        }
        private static string[] subDirs = { "SourceCode", "Inputs", "Outputs", "Logs" };
        private static void CheckProjectDirectoriesInput(string userId, int projectId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (projectId <= 0)
                throw new ArgumentOutOfRangeException(nameof(projectId));
        }
        private void CreateDirectoryWithSubdirectories(
            string basePath,
            IEnumerable<string> subDirectories)
        {
            fileSystem.CreateDirectory(basePath);

            foreach (var subDir in subDirectories) { fileSystem.CreateDirectory(Path.Combine(basePath, subDir)); }
        }


        private const string CODE_DIRECTORY = "SourceCode";
        private async Task<bool> UserOwnsProject(string userId, int projectId)
        {
            return await projectDbService.IsProjectOwnedByUser(userId, projectId);
        }
        private string GetSourceCodeFilePath(string userId, int projectId, string fileName)
        {
            return Path.Combine(
                GetProjectDirectoryPath(userId, projectId),
                CODE_DIRECTORY,
                fileName
            );
        }
        private async Task<string?> ReadFileSafelyAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (IOException)
            {
                return null;
            }
        }




        public UserFileService(IConfiguration configuration, ProjectDbService projectDbService) 
        {
            BASE_PATH = configuration["FileStorage:Basepath"] ?? "D:/CompilerApp/StoredFiles";
            this.projectDbService = projectDbService ?? throw new ArgumentNullException(nameof(projectDbService));
        }



        public Task CreateDefaultProjectDirectoriesAsync(string userId, int projectId)
        {
            CheckProjectDirectoriesInput(userId, projectId);

            string projectDirectoryPath = GetProjectDirectoryPath(userId, projectId);

            CreateDirectoryWithSubdirectories(projectDirectoryPath, subDirs);

            return Task.CompletedTask;
        }



        public async Task CreateFile(string userId, int projectId, string fileName, string fileExtension, string directory = "") 
        {
            if (string.IsNullOrEmpty(userId)) 
                throw new ArgumentNullException(nameof(userId));

            string projectDirectory = GetProjectDirectoryPath(userId,projectId); // Single Responsibility?, Direktorijos traukimas != CreateFile
            const string sourceCodeDirectory = "SourceCode";
            
            string fullFilePath = Path.Combine(projectDirectory,sourceCodeDirectory, directory,  fileName + fileExtension); // Vėl Single Responsibility?, Direktorijos kūrimas != CreateFile

            if (File.Exists(fullFilePath))
                Console.WriteLine("A file with that name already exists in this directory");
            else 
            {
                using (FileStream fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write)) 
                {
                    using (StreamWriter wr = new StreamWriter(fs)) 
                    {
                        var language = await GetLanguageByExtension(fileExtension);
                        await wr.WriteAsync(await GetCodePreset(language, fileName, await projectDbService.GetProjectById(projectId)));
                    }
                }
            }
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
                // Single Responsibility Principle, metodas tikrina autentifikaciją
                if (await projectDbService.IsProjectOwnedByUser(userId, projectId))
                {
                    List<String> fileNames = await GetProjectFileNames(userId, projectId);
                    if (fileNames.Contains(fileName)) // Single Responsibility Principle, metodas tikrina ar failas egzistuoja direktorijoje
                    {
                        const string CODE_DIRECTORY = "SourceCode";
                        string fullFilePath = Path.Combine(GetProjectDirectoryPath(userId, projectId),CODE_DIRECTORY,fileName); // Single Responsibility Principle, metodas kombinuoja file path
                        await File.WriteAllTextAsync(fullFilePath, fileContent);// Single Responsibility Principle, metodas saugo failą
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



        public async Task<string?> GetFileContentAsync(string userId, int projectId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (!await UserOwnsProject(userId, projectId))
            {
                Console.WriteLine($"User {userId} does not own project {projectId}");
                return null;
            }

            string filePath = GetSourceCodeFilePath(userId, projectId, fileName);

            var content = await ReadFileSafelyAsync(filePath);
            if (content == null)
            {
                Console.WriteLine($"Could not read file: {fileName}");
            }

            return content;
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
        public Task<String> GetExtensionByLanguage(String language)
        {
            return Task.FromResult(language.ToLower() switch
            {
                "java" => ".java",
                "c++" => ".cpp",
                "c" => ".c",
                "xml" => ".xml",
                _ => ""
            });
        }
        public Task<String> GetLanguageByExtension(String extension) 
        {
            return Task.FromResult(extension.ToLower() switch 
            {
                ".java" => "java",
                ".cpp" => "c++",
                ".c" => "c",
                ".xml" => "xml",
                _ => ""
            });
        }
        public async Task CreateDefaultProjectFiles(string userId, int projectId)
        {
            const string MAVEN_CONFIG_FILE_NAME = "pom", MAVEN_CONFIG_FILE_EXTENSION = ".xml";
            Project project = await projectDbService.GetProjectById(projectId);
            switch (project.Language.ToLower())
            {
                case "java":  
                    await CreateFile(userId, projectId, project.Name, await GetExtensionByLanguage("java"));
                    await CreateFile(userId, projectId, MAVEN_CONFIG_FILE_NAME, MAVEN_CONFIG_FILE_EXTENSION);
                    break;
                case "c++":
                    await CreateFile(userId, projectId, project.Name, await GetExtensionByLanguage("c++"));
                    break;
                case "c":
                    await CreateFile(userId, projectId, project.Name, await GetExtensionByLanguage("c"));
                    break;
                default:
                    throw new ArgumentException($"Language of the project with id: {projectId} is not supported");
            };
        }
        public async Task<String> GetCodePreset(String language, String fileName, Project project)
        {
            
            return await Task.FromResult(language.ToLower() switch 
            {
                "java" => $"public class {fileName} {{\n    public static void main(String[] args) {{\n        System.out.println(\"Hello, World!\");\n    }}\n}}",
                "c++" => $"#include <iostream>\n\nusing namespace std;\n\nint main() {{\n    cout << \"Hello, World!\" << endl;\n    return 0;\n}}",
                "c" => $"#include <stdio.h>\n\nint main() {{\n    printf(\"Hello, World!\\n\");\n    return 0;\n}}",
                "xml" => fileName.ToLower().Equals("pom")
                ? @$"<project xmlns=""http://maven.apache.org/POM/4.0.0""
                     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                     xsi:schemaLocation=""http://maven.apache.org/POM/4.0.0 
                     https://maven.apache.org/xsd/maven-4.0.0.xsd"">
                <modelVersion>4.0.0</modelVersion>

                <groupId>com.example</groupId>
                <artifactId>myapp</artifactId>
                <version>1.0-SNAPSHOT</version>

                <properties>
                    <maven.compiler.source>17</maven.compiler.source>
                    <maven.compiler.target>17</maven.compiler.target>
                </properties>

                <build>
                    <plugins>
                        <plugin>
                            <groupId>org.apache.maven.plugins</groupId>
                            <artifactId>maven-compiler-plugin</artifactId>
                            <version>3.8.1</version>
                            <configuration>
                                <source>17</source>
                                <target>17</target>
                            </configuration>
                        </plugin>
                        <plugin>
                            <groupId>org.apache.maven.plugins</groupId>
                            <artifactId>maven-assembly-plugin</artifactId>
                            <version>3.3.0</version>
                            <configuration>
                                <descriptorRefs>
                                    <descriptorRef>jar-with-dependencies</descriptorRef>
                                </descriptorRefs>
                                <archive>
                                    <manifest>
                                        <mainClass>{project.Name}</mainClass>
                                    </manifest>
                                </archive>
                            </configuration>
                            <executions>
                                <execution>
                                    <id>make-assembly</id>
                                    <phase>package</phase>
                                    <goals>
                                        <goal>single</goal>
                                    </goals>
                                </execution>
                            </executions>
                        </plugin>
                    </plugins>
                </build>

                <dependencies>
                    <!-- Add any project dependencies here -->
                </dependencies>
            </project>"
            : "",
                _  => ""

            });
        }
    }
}
