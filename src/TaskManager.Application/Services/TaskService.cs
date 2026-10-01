using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<TaskFilter> _filterValidator;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository repository,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<TaskFilter> filterValidator,
        ILogger<TaskService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _logger = logger;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status ?? TaskItemStatus.Pending);

        await _repository.AddAsync(task, cancellationToken);

        _logger.LogInformation("Task {TaskId} created", task.Id);

        return TaskResponse.FromEntity(task);
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);
        return TaskResponse.FromEntity(task);
    }

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        await _filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var tasks = await _repository.ListAsync(filter, cancellationToken);

        return tasks.Select(TaskResponse.FromEntity).ToList();
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await GetExistingTaskAsync(id, cancellationToken);

        task.Update(request.Title, request.Description, request.DueDate, request.Status);

        await _repository.UpdateAsync(task, cancellationToken);

        _logger.LogInformation("Task {TaskId} updated", task.Id);

        return TaskResponse.FromEntity(task);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);

        await _repository.DeleteAsync(task, cancellationToken);

        _logger.LogInformation("Task {TaskId} deleted", id);
    }

    private async Task<TaskItem> GetExistingTaskAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            _logger.LogWarning("Task {TaskId} not found", id);
            throw new NotFoundException($"Tarefa '{id}' não encontrada.");
        }

        return task;
    }
}