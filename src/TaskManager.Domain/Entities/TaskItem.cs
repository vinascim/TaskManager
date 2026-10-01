using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskItemStatus Status { get; private set; }

    private TaskItem() { }

    public TaskItem(string title, string? description, DateOnly? dueDate, TaskItemStatus status)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
    }
}