using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Filtros opcionais e combináveis da listagem de tarefas.
/// </summary>
/// <param name="Status" example="Pending">Retorna apenas tarefas com este status.</param>
/// <param name="DueDateFrom" example="2026-01-01">Vencimento a partir desta data (inclusive).</param>
/// <param name="DueDateTo" example="2026-12-31">Vencimento até esta data (inclusive).</param>
/// <param name="Search" example="login">Texto buscado no título e na descrição, sem diferenciar maiúsculas.</param>
/// <param name="Page" example="1">Número da página, começando em 1. Padrão: 1.</param>
/// <param name="PageSize" example="20">Itens por página, de 1 a 100. Padrão: 20.</param>
public sealed record TaskFilter(
    TaskItemStatus? Status = null,
    DateOnly? DueDateFrom = null,
    DateOnly? DueDateTo = null,
    string? Search = null,
    int Page = 1,
    int PageSize = TaskFilter.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}