using System.Diagnostics;
using System.Threading.Tasks;

public class CompilerService
{
    private const string RUN_ARG = "run", REMOVE_FLAG = "--rm", CPU_LIMIT_ARG = "--cpus=0.5", MEMORY_LIMIT_ARG = "--memory=256m", PID_LIMIT = "--pids-limit=64", SECURITY_ARG = "--security-opt=no-new-privileges",
        NETWORK_ARG = "--network=none", VOLUME_FLAG = "-v", WORKDIR_FLAG = "--workdir", WORKDIR_ARG = "/compiler", NAME_FLAG = "--name", SHELL_DIR_ARG = "/bin/sh", COMMAND_FLAG = "-c";
    
    public async Task<CompilationResult> CompileCodeAsync(string code, string codeFileName, string language)
    {
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

        string codeFilePath = Path.Combine(tempDir, codeFileName);
        await File.WriteAllTextAsync(codeFilePath, code);
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            
            processStartInfo.ArgumentList.Add(RUN_ARG);
            processStartInfo.ArgumentList.Add(REMOVE_FLAG);
            processStartInfo.ArgumentList.Add(CPU_LIMIT_ARG);
            processStartInfo.ArgumentList.Add(MEMORY_LIMIT_ARG);
            processStartInfo.ArgumentList.Add(PID_LIMIT);
            processStartInfo.ArgumentList.Add(SECURITY_ARG);
            processStartInfo.ArgumentList.Add(NETWORK_ARG);
            processStartInfo.ArgumentList.Add(VOLUME_FLAG);
            processStartInfo.ArgumentList.Add($"{tempDir}:/compiler");
            processStartInfo.ArgumentList.Add(WORKDIR_FLAG);
            processStartInfo.ArgumentList.Add(WORKDIR_ARG);
            processStartInfo.ArgumentList.Add(NAME_FLAG);
            processStartInfo.ArgumentList.Add(containerName);
            processStartInfo.ArgumentList.Add(imageName);
            processStartInfo.ArgumentList.Add(SHELL_DIR_ARG);
            processStartInfo.ArgumentList.Add(COMMAND_FLAG);

            switch (language.ToLower()) 
            {
                case "c":
                    processStartInfo.ArgumentList.Add($"gcc {codeFileName} -o {outputFileName} && timeout 5 ./{outputFileName}");
                    break;
                case "c++":
                    processStartInfo.ArgumentList.Add($"g++ {codeFileName} -o {outputFileName} && timeout 5 ./{outputFileName}");
                    break;
                case "java":
                    processStartInfo.ArgumentList.Add($"javac {codeFileName} && timeout 5 Main");
                    break;
                default:
                    throw new NotSupportedException($"Language '{language}' is not supported.");
                   
            }

            var process = new Process
            {
                StartInfo = processStartInfo,
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string errors = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            return new CompilationResult
            {
                Output = output,
                Errors = errors,
                Success = process.ExitCode == 0,
            };
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

public class CompilationResult
{
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Errors { get; set; }
}
