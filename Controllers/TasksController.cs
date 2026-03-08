using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecureTaskApi.Dtos;
using SecureTaskApi.Services;

namespace SecureTaskApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService) => _taskService = taskService;

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskReadDto>>> Get() =>
        Ok(await _taskService.GetTasksAsync(GetUserId()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskReadDto>> Get(int id)
    {
        var task = await _taskService.GetTaskAsync(GetUserId(), id);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskReadDto>> Post(TaskCreateDto dto)
    {
        var created = await _taskService.CreateTaskAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, TaskCreateDto dto) =>
        await _taskService.UpdateTaskAsync(GetUserId(), id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await _taskService.DeleteTaskAsync(GetUserId(), id) ? NoContent() : NotFound();
}
