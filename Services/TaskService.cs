using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.Dtos;
using SecureTaskApi.Models;

namespace SecureTaskApi.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    public TaskService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TaskReadDto>> GetTasksAsync(int userId) =>
        await _context.Tasks
            .Where(t => t.UserId == userId)
            .Select(t => new TaskReadDto { Id = t.Id, Title = t.Title, Description = t.Description, IsCompleted = t.IsCompleted, CreatedAt = t.CreatedAt })
            .ToListAsync();

    public async Task<TaskReadDto?> GetTaskAsync(int userId, int id) =>
        await _context.Tasks
            .Where(t => t.UserId == userId && t.Id == id)
            .Select(t => new TaskReadDto { Id = t.Id, Title = t.Title, Description = t.Description, IsCompleted = t.IsCompleted, CreatedAt = t.CreatedAt })
            .FirstOrDefaultAsync();

    public async Task<TaskReadDto> CreateTaskAsync(int userId, TaskCreateDto dto)
    {
        var task = new TaskItem { Title = dto.Title, Description = dto.Description, UserId = userId };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return new TaskReadDto { Id = task.Id, Title = task.Title, Description = task.Description, IsCompleted = task.IsCompleted, CreatedAt = task.CreatedAt };
    }

    public async Task<bool> UpdateTaskAsync(int userId, int id, TaskCreateDto dto)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task == null) return false;
        task.Title = dto.Title;
        task.Description = dto.Description;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int userId, int id)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task == null) return false;
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
