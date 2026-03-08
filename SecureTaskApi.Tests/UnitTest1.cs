using Xunit;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.Dtos;
using SecureTaskApi.Services;

namespace SecureTaskApi.Tests;

public class TaskServiceTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreatedTask()
    {
        var ctx = GetInMemoryContext();
        var service = new TaskService(ctx);
        var dto = new TaskCreateDto { Title = "Test Task" };

        var result = await service.CreateTaskAsync(1, dto);

        Assert.Equal("Test Task", result.Title);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnOnlyUserTasks()
    {
        var ctx = GetInMemoryContext();
        var service = new TaskService(ctx);

        await service.CreateTaskAsync(1, new TaskCreateDto { Title = "User 1 Task" });
        await service.CreateTaskAsync(2, new TaskCreateDto { Title = "User 2 Task" });

        var result = await service.GetTasksAsync(1);

        Assert.Single(result);
        Assert.Equal("User 1 Task", result.First().Title);
    }

    [Fact]
    public async Task DeleteTask_WithWrongUser_ShouldReturnFalse()
    {
        var ctx = GetInMemoryContext();
        var service = new TaskService(ctx);
        await service.CreateTaskAsync(1, new TaskCreateDto { Title = "Task" });

        var result = await service.DeleteTaskAsync(userId: 99, id: 1);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateTitle()
    {
        var ctx = GetInMemoryContext();
        var service = new TaskService(ctx);
        await service.CreateTaskAsync(1, new TaskCreateDto { Title = "Old Title" });

        var updated = await service.UpdateTaskAsync(1, 1, new TaskCreateDto { Title = "New Title" });

        Assert.True(updated);
    }
}
