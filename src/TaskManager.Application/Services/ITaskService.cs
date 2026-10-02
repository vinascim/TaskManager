using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskResponse>> ListAsync(TaskFilter filter, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}