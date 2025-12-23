using Moq;
using Microsoft.Extensions.Configuration;
using BlazorServerApp.Services;
using System.IO;
using System.Threading.Tasks;
using System;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Castle.Core.Logging;
namespace BlazorServerApp.Tests.Services
{
    public class UserFileService_CreateDefaultProjectDirectoriesAsyncTests
    {
        private readonly UserFileService service;
        private readonly string basePath;

        public UserFileService_CreateDefaultProjectDirectoriesAsyncTests()
        {
            basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["FileStorage:Basepath"]).Returns(basePath);

            var projectDbMock = new Mock<ProjectDbService>(null!);

            service = new UserFileService(configMock.Object, projectDbMock.Object);
        }

        //Ar userID == null
        [Fact]
        public async Task CreateDefaultProjectDirectories_UserIdNull_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateDefaultProjectDirectoriesAsync(null!, 1));
        }

        //Ar sukuriamos visos projekto direkrorijos
        [Fact]
        public async Task CreateDefaultProjectDirectories_ProjectDoesNotExist_CreatesDirectories()
        {
            await service.CreateDefaultProjectDirectoriesAsync("user1", 1);

            string baseDir = Path.Combine(basePath, "User_user1", "Project_1");

            Assert.True(Directory.Exists(baseDir));
            Assert.True(Directory.Exists(Path.Combine(baseDir, "SourceCode")));
            Assert.True(Directory.Exists(Path.Combine(baseDir, "Inputs")));
            Assert.True(Directory.Exists(Path.Combine(baseDir, "Outputs")));
            Assert.True(Directory.Exists(Path.Combine(baseDir, "Logs")));
        }
    }
}
