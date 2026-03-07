using SecureTaskApi.Dtos;

namespace SecureTaskApi.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskReadDto>> GetTasksAsync(int userId);
    Task<TaskReadDto?> GetTaskAsync(int userId, int id);
    Task<TaskReadDto> CreateTaskAsync(int userId, TaskCreateDto dto);
    Task<bool> UpdateTaskAsync(int userId, int id, TaskCreateDto dto);
    Task<bool> DeleteTaskAsync(int userId, int id);
}
