using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

/// <summary>
/// Dados para substituir uma tarefa existente (PUT). Todos os campos são enviados.
/// </summary>
/// <param name="Title" example="Implementar tela de login">Título da tarefa. Obrigatório, até 100 caracteres.</param>
/// <param name="Description" example="Adicionar opção de lembrar usuário">Descrição opcional, até 500 caracteres. Null remove a descrição.</param>
/// <param name="DueDate" example="2027-01-15">Data de vencimento opcional (yyyy-MM-dd). Null remove o vencimento.</param>
/// <param name="Status" example="InProgress">Status da tarefa. Obrigatório.</param>
public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus? Status);