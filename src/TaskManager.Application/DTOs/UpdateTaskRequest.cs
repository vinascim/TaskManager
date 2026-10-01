using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus? Status);