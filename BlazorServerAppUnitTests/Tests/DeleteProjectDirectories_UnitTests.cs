using BlazorServerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using BlazorServerApp.Data;
using BlazorServerApp.Services;
namespace BlazorServerApp.Tests
{
    public class UserFileServiceTests
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
        [Fact]
        public async Task DeleteProjectDirectoriesAsync_DeletesDirectory_WhenDirectoryExists()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "ufstest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string file1 = Path.Combine(tempDir, "a.txt");
            string file2 = Path.Combine(tempDir, "b.txt");
            File.WriteAllText(file1, "hello");
            File.WriteAllText(file2, "world");

            IConfiguration configuration = GetTestConfiguration();

            ApplicationDbContext db = CreateInMemoryDb();
            ProjectDbService projectDbService = new ProjectDbService(db);

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, tempDir);


            await userFileService.DeleteProjectDirectories("anyUser", 1);

            Assert.False(Directory.Exists(tempDir));
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DeleteProjectDirectoriesAsync_Throws_WhenUserIdMissing(string userId)
        {
            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = new ProjectDbService(CreateInMemoryDb());

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, Path.GetTempPath());

            await Assert.ThrowsAsync<ArgumentNullException>(() => userFileService.DeleteProjectDirectories(userId, 1));
        }
        [Fact]
        public async Task DeleteProjectDirectoriesAsync_DoesNotThrow_WhenDirectoryMissing()
        {
            string missingDir = Path.Combine(Path.GetTempPath(), "missing_" + Guid.NewGuid().ToString("N"));

            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = new ProjectDbService(CreateInMemoryDb());

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, missingDir);

            await userFileService.DeleteProjectDirectories("u1", 1);
            //throw
            Assert.False(Directory.Exists(missingDir));
        }
        [Fact]
        public async Task DeleteProjectDirectoriesAsync_DoesNotThrow_WhenPathIsInvalid()
        {
            string invalidPath = "C:\\invalid?path";

            IConfiguration configuration = GetTestConfiguration();
            ProjectDbService projectDbService = new ProjectDbService(CreateInMemoryDb());

            TestUserFileService userFileService = new TestUserFileService(configuration, projectDbService, invalidPath);
            //doesnt throw
            await userFileService.DeleteProjectDirectories("u1", 1);
        }

    }
}
