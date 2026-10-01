using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Dados para criar uma tarefa.
/// </summary>
/// <param name="Title" example="Implementar tela de login">Título da tarefa. Obrigatório, até 100 caracteres.</param>
/// <param name="Description" example="Criar formulário com e-mail e senha e validação dos campos">Descrição opcional, até 500 caracteres.</param>
/// <param name="DueDate" example="2026-12-31">Data de vencimento opcional (yyyy-MM-dd). Datas no passado são aceitas.</param>
/// <param name="Status" example="Pending">Status inicial. Se omitido, assume Pending.</param>
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus? Status);