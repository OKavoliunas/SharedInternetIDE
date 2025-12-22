using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BlazorServerApp.Services;
using BlazorServerApp.Data;
using BlazorServerApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
namespace BlazorServerApp.Tests.Services
{
    public class UserFileService_GetFileContentTests
    {
        private readonly UserFileService service;
        private readonly ApplicationDbContext dbContext;
        private readonly string basePath;

        public UserFileService_GetFileContentTests()
        {
            basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("FileStorage:Basepath", basePath)
                })
                .Build();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new ApplicationDbContext(options);
            var projectDbService = new ProjectDbService(dbContext);

            service = new UserFileService(configuration, projectDbService, NullLogger<UserFileService>.Instance);
        }

        //Jei naudotojas turi projektą ir failas egzistuoja, jį gražinti
        [Fact]
        public async Task GetFileContent_UserOwnsProject_FileExists_ReturnsContent()
        {
            dbContext.Projects.Add(new Project
            {
                ProjectID = 1,
                UserID = "user1",
                Name = "Test",
                Language = "c",
                Description = ""
            });
            await dbContext.SaveChangesAsync();

            string sourceDir = Path.Combine(basePath, "User_user1", "Project_1", "SourceCode");
            Directory.CreateDirectory(sourceDir);

            string filePath = Path.Combine(sourceDir, "test.txt");
            await File.WriteAllTextAsync(filePath, "Hello world");

            string result = await service.GetFileContentAsync("user1", 1, "test.txt");

            Assert.Equal("Hello world", result);
        }

        //Jei naudotojas turi projektą ir failas egzistuoja, gražinti tuščią string
        [Fact]
        public async Task GetFileContent_UserOwnsProject_FileDoesNotExist_ReturnsEmpty()
        {
            dbContext.Projects.Add(new Project
            {
                ProjectID = 1,
                UserID = "user1",
                Name = "Test",
                Language = "c",
                Description = ""
            });
            await dbContext.SaveChangesAsync();

            string result = await service.GetFileContentAsync("user1", 1, "missing.txt");

            Assert.Equal(string.Empty, result);
        }

        //Jei naudotojas neturi projektų, gražinti tuščią string
        [Fact]
        public async Task GetFileContent_UserDoesNotOwnProject_ReturnsEmpty()
        {
            dbContext.Projects.Add(new Project
            {
                ProjectID = 1,
                UserID = "otherUser",
                Name = "Test",
                Language = "c",
                Description = ""
            });
            await dbContext.SaveChangesAsync();

            string result = await service.GetFileContentAsync("user1", 1, "test.txt");

            Assert.Equal(string.Empty, result);
        }
    }
}

