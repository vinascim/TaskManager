using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Tarefa retornada pela API.
/// </summary>
/// <param name="Id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">Identificador único gerado pelo sistema.</param>
/// <param name="Title" example="Implementar tela de login">Título da tarefa.</param>
/// <param name="Description" example="Criar formulário com e-mail e senha e validação dos campos">Descrição da tarefa, se houver.</param>
/// <param name="DueDate" example="2026-12-31">Data de vencimento, se houver.</param>
/// <param name="Status" example="Pending">Status atual da tarefa.</param>
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