using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementApi.Controllers;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace TaskManagementApi.Tests
{
    [TestFixture]
    public class TaskControllerTests
    {
        private TaskContext _context;
        private TaskController _controller;

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TaskContext(options);
            _controller = new TaskController(_context);
        }

        // This test verifies that the CreateTaskItem method creates a new task and returns the correct CreatedAtActionResult.
        [Test]
        public async Task CreateTaskItem_ShouldReturnCreatedTask()
        {
            var taskItem = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Deadline = DateTime.Now.AddDays(1),
                Status = "ToDo",
                isFavorite = false
            };

            var result = await _controller.CreateTaskItem(taskItem);
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            var createdTask = createdAtActionResult?.Value as TaskItem;

            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult?.ActionName, Is.EqualTo("GetTaskItem"));
            Assert.That(createdTask, Is.Not.Null);
            Assert.That(createdTask?.Name, Is.EqualTo(taskItem.Name));
        }

        // This test verifies that the GetTaskItem method returns the correct task based on the provided ID.
        [Test]
        public async Task GetTaskItem_ShouldReturnTask()
        {
            var taskItem = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Deadline = DateTime.Now.AddDays(1),
                Status = "ToDo",
                isFavorite = false
            };

            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();

            var result = await _controller.GetTaskItem(taskItem.Id) as ActionResult<TaskItem>;
            var fetchedTask = result?.Value;

            Assert.That(fetchedTask, Is.Not.Null);
            Assert.That(fetchedTask?.Name, Is.EqualTo(taskItem.Name));
        }

        // This test verifies that the UpdateTaskItem method updates an existing task and returns the correct updated task.
        [Test]
        public async Task UpdateTaskItem_ShouldReturnUpdatedTask()
        {
            var taskItem = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Deadline = DateTime.Now.AddDays(1),
                Status = "ToDo",
                isFavorite = false
            };

            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();

            taskItem.Name = "Updated Task";
            var result = await _controller.UpdateTaskItem(taskItem.Id, taskItem) as OkObjectResult;
            var updatedTask = result?.Value as TaskItem;

            Assert.That(updatedTask, Is.Not.Null);
            Assert.That(updatedTask?.Name, Is.EqualTo("Updated Task"));
        }
        // This test verifies that the UploadFile method correctly handles file uploads and returns the expected file URL.
        [Test]
        public async Task UploadFile_ShouldReturnFileUrl()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Name = "Test Task",
                Description = "Test Description",
                Deadline = DateTime.Now.AddDays(1),
                Status = "ToDo",
                isFavorite = false
            };

            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();

            var fileName = "testfile.jpg";
            var fileContent = "test file content";
            var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.ContentType).Returns("image/jpeg");
            fileMock.Setup(_ => _.Length).Returns(fileStream.Length);

            // Act
            var result = await _controller.UploadFile(taskItem.Id, fileMock.Object) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var response = result?.Value as dynamic;
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Id, Is.EqualTo(taskItem.Id));
            Assert.That(response.ImageUrl, Is.EqualTo($"/upload/{fileName}"));
        }




    }
}
