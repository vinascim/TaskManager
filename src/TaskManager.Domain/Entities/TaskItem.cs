using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Entities;

public sealed class TaskItem
{
    public const int TitleMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskItemStatus Status { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, DateOnly? dueDate, TaskItemStatus status)
    {
        var task = new TaskItem { Id = Guid.NewGuid() };
        task.Apply(title, description, dueDate, status);
        return task;
    }

    public void Update(string title, string? description, DateOnly? dueDate, TaskItemStatus status)
    {
        Apply(title, description, dueDate, status);
    }

    private void Apply(string title, string? description, DateOnly? dueDate, TaskItemStatus status)
    {
        var validTitle = ValidateTitle(title);
        var validDescription = ValidateDescription(description);
        ValidateStatus(status);

        Title = validTitle;
        Description = validDescription;
        DueDate = dueDate;
        Status = status;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título é obrigatório.");

        var trimmed = title.Trim();

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException($"O título deve ter no máximo {TitleMaxLength} caracteres.");

        return trimmed;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        return trimmed;
    }

    private static void ValidateStatus(TaskItemStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException("Status inválido.");
    }
}
