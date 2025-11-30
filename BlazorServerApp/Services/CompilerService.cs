using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using BlazorServerApp.Models;
using BlazorServerApp.Exceptions;
namespace BlazorServerApp.Services 
{

    public class CompilerService
    {
        private UserFileService userFileService;
        private static readonly string RUN_ARG;
        private static readonly string REMOVE_FLAG;
        private static readonly string CPU_LIMIT_ARG;
        private static readonly string MEMORY_LIMIT_ARG;
        private static readonly string PID_LIMIT;
        private static readonly string SECURITY_ARG;
        private static readonly string NETWORK_ARG;
        private static readonly string VOLUME_FLAG;
        private static readonly string WORKDIR_FLAG;
        private static readonly string WORKDIR_ARG;
        private static readonly string NAME_FLAG;
        private static readonly string SHELL_DIR_ARG;
        private static readonly string COMMAND_FLAG;
        static CompilerService()
        {
            RUN_ARG = "run";
            REMOVE_FLAG = "--rm";
            CPU_LIMIT_ARG = "--cpus=0.5";
            MEMORY_LIMIT_ARG = "--memory=256m";
            PID_LIMIT = "--pids-limit=64";
            SECURITY_ARG = "--security-opt=no-new-privileges";
            //NETWORK_ARG = "--network=none"; maven doesn't like this one as it needs internet to function, TODO: figure out how to get this one back on
            VOLUME_FLAG = "-v";
            WORKDIR_FLAG = "--workdir";
            WORKDIR_ARG = "/compiler";
            NAME_FLAG = "--name";
            SHELL_DIR_ARG = "/bin/sh";
            COMMAND_FLAG = "-c";
        }
        public event Action<string> OnOutputReceived;
        public event Action<string> OnErrorReceived;
        public CompilerService(UserFileService userFileService) 
        {
            this.userFileService = userFileService;
        }
        public async Task<Result<CompilationResult>> CompileCodeAsync(string language, string userId, int projectId)
        {
            try
            {
                var fileNames = await userFileService.GetProjectFileNames(userId, projectId);
                if (!fileNames.Any()) 
                {
                    return Result<CompilationResult>.Failure($"No source code was found within the project with the ID: {projectId}");
                }
                string imageName;
                switch (language.ToLower())
                {
                    case "c":
                        imageName = "c-compiler:latest";
                        break;
                    case "c++":
                        imageName = "cpp-compiler:latest";
                        break;
                    case "java":
                        imageName = "java-compiler:latest";
                        break;
                    default:
                        throw new NotSupportedException($"Language '{language}' is not supported.");
                }
                string sessionId = Guid.NewGuid().ToString();
                string containerName = $"compiler_{sessionId}";

                string outputFileName = "main.out";


                string tempDir = Path.Combine(Path.GetTempPath(), sessionId);
                Directory.CreateDirectory(tempDir);
                
                foreach (var fileName in fileNames) 
                {
                    var content = await userFileService.GetFileContentAsync(userId,projectId, fileName);
                    string targetPath = Path.Combine(tempDir, fileName);
                    await File.WriteAllTextAsync(targetPath, content);
                }
                if (language.ToLower() == "java") // If the programming language is java - it should hold a Maven supported file structure
                {
                    string srcMainJavaDir = Path.Combine(tempDir, "src", "main", "java");
                    Directory.CreateDirectory(srcMainJavaDir);

                    var javaFiles = Directory.GetFiles(tempDir, "*.java");
                    foreach (var javaFile in javaFiles)
                    {
                        string fileName = Path.GetFileName(javaFile);
                        string targetPath = Path.Combine(srcMainJavaDir, fileName);
                        File.Move(javaFile, targetPath);
                    }


                }

                try
                {

                    ProcessStartInfo processStartInfo;
                    string compilerArgument = language.ToLower() switch
                    {
                        "c" => $"gcc *.c -o {outputFileName} && timeout 5 ./{outputFileName}",
                        "c++" => $"g++ *.cpp -o {outputFileName} && timeout 5 ./{outputFileName}",
                        "java" => $"mvn package && timeout 5 java -jar target/myapp-1.0-SNAPSHOT-jar-with-dependencies.jar",
                        _ => throw new NotSupportedException($"Language '{language}' is not supported.")
                    };
                    AddArgsToProcess(out processStartInfo, RUN_ARG, REMOVE_FLAG, CPU_LIMIT_ARG, MEMORY_LIMIT_ARG, PID_LIMIT, SECURITY_ARG, /*NETWORK_ARG, maven needs internet to function*/ VOLUME_FLAG, $"{tempDir}:/compiler",
                        WORKDIR_FLAG, WORKDIR_ARG, NAME_FLAG, containerName, imageName, SHELL_DIR_ARG, COMMAND_FLAG, compilerArgument);
                    var process = new Process
                    {
                        StartInfo = processStartInfo,
                    };
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            outputBuilder.AppendLine(e.Data);
                            OnOutputReceived?.Invoke(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            errorBuilder.AppendLine(e.Data);
                            OnErrorReceived?.Invoke(e.Data);
                        }
                    };
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();

                    CompilationResult compilationResult = new CompilationResult
                    {
                        Output = outputBuilder.ToString(),
                        Errors = errorBuilder.ToString(),
                        Success = process.ExitCode == 0,
                    };
                    return Result<CompilationResult>.Success(compilationResult);
                }
                finally
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch (CompilationException ex) 
            {
                return Result<CompilationResult>.Failure($"An error happened during compilation: {ex.Message}");
            }
            
        }
        private void AddArgsToProcess(out ProcessStartInfo processStartInfo, params string[] args)
        {
            processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (string arg in args)
            {
                processStartInfo.ArgumentList.Add(arg);
            }
        }
    }

    public class CompilationResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Errors { get; set; }
    }

}