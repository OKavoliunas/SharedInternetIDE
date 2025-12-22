using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using BlazorServerApp.Services;
using BlazorServerApp.Models;
using BlazorServerApp.Data;

namespace BlazorServerApp.Tests
{
    public class UserFileService_CreateFileTests
    {
        public class TestUserFileService : UserFileService
        {
            private readonly string FORCED_PATH;

            public TestUserFileService(IConfiguration configuration, ProjectDbService projectDbService, string forcedPath)
                : base(configuration, projectDbService)
            {
                FORCED_PATH = forcedPath;
            }

            public override string GetProjectDirectoryPath(string userId, int projectId)
            {
                return FORCED_PATH;
            }
        }

        public IConfiguration GetTestConfiguration()
        {
            Dictionary<string, string?> configValues = new Dictionary<string, string?>();
            configValues["FileStorage:Basepath"] = Path.GetTempPath();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            return configuration;
        }

        private ApplicationDbContext CreateInMemoryDb()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString("N"))
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task<ProjectDbService> CreateDbWithProject(int projectId, string userId, string projectName, string language)
        {
            ApplicationDbContext db = CreateInMemoryDb();

            Project project = new Project();
            project.ProjectID = projectId;
            project.UserID = userId;
            project.Name = projectName;
            project.Language = language;
            project.Description = "test";
            project.CreationDate = DateTime.UtcNow;

            db.Add(project);
            await db.SaveChangesAsync();

            return new ProjectDbService(db);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateFile_Throws_WhenUserIdMissing(string userId)
        {
            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = await CreateDbWithProject(1, "u1", "MyProj", "c");

            string tempDir = Path.Combine(Path.GetTempPath(), "ufstest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempDir, "SourceCode"));

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, tempDir);

            await Assert.ThrowsAsync<ArgumentNullException>(() => userFileService.CreateFile(userId, 1, "Main", ".c"));
        }

        [Fact]
        public async Task CreateFile_CreatesFile_WithPreset_WhenFileDoesNotExist()
        {
            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = await CreateDbWithProject(1, "u1", "MyProj", "c");

            string tempDir = Path.Combine(Path.GetTempPath(), "ufstest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempDir, "SourceCode"));

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, tempDir);

            await userFileService.CreateFile("u1", 1, "Main", ".c");

            string createdPath = Path.Combine(tempDir, "SourceCode", "Main.c");
            Assert.True(File.Exists(createdPath));

            string content = await File.ReadAllTextAsync(createdPath);
            Assert.Contains("Hello, World!", content);
        }

        [Fact]
        public async Task CreateFile_DoesNotOverwrite_WhenFileAlreadyExists()
        {
            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = await CreateDbWithProject(1, "u1", "MyProj", "c");

            string tempDir = Path.Combine(Path.GetTempPath(), "ufstest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempDir, "SourceCode"));

            string existingPath = Path.Combine(tempDir, "SourceCode", "Main.c");
            await File.WriteAllTextAsync(existingPath, "DO_NOT_OVERWRITE");

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, tempDir);

            await userFileService.CreateFile("u1", 1, "Main", ".c");

            string content = await File.ReadAllTextAsync(existingPath);
            Assert.Equal("DO_NOT_OVERWRITE", content);
        }

        [Fact]
        public async Task CreateFile_CreatesFile_InSubdirectory_WhenDirectoryArgumentProvided()
        {
            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = await CreateDbWithProject(2, "u1", "MyProj2", "c");

            string tempDir = Path.Combine(Path.GetTempPath(), "ufstest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempDir, "SourceCode", "Sub"));

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, tempDir);

            await userFileService.CreateFile("u1", 2, "Main", ".c", "Sub");

            string createdPath = Path.Combine(tempDir, "SourceCode", "Sub", "Main.c");
            Assert.True(File.Exists(createdPath));
        }
    }
}
