using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public sealed record TaskFilter(
    TaskItemStatus? Status = null,
    DateOnly? DueDateFrom = null,
    DateOnly? DueDateTo = null,
    string? Search = null);