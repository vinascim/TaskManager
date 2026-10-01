using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Filtros opcionais e combináveis da listagem de tarefas.
/// </summary>
/// <param name="Status" example="Pending">Retorna apenas tarefas com este status.</param>
/// <param name="DueDateFrom" example="2026-01-01">Vencimento a partir desta data (inclusive).</param>
/// <param name="DueDateTo" example="2026-12-31">Vencimento até esta data (inclusive).</param>
/// <param name="Search" example="login">Texto buscado no título e na descrição, sem diferenciar maiúsculas.</param>
public sealed record TaskFilter(
    TaskItemStatus? Status = null,
    DateOnly? DueDateFrom = null,
    DateOnly? DueDateTo = null,
    string? Search = null);