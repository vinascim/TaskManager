using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus Status)
{
    public static TaskResponse FromEntity(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.DueDate, task.Status);
}