using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus? Status);