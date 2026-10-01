using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Tests.Entities;

public class TaskItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateTask()
    {
        var title = "Estudar Clean Architecture";
        var description = "Ler sobre regra de dependência";
        var dueDate = new DateOnly(2026, 12, 31);
        var status = TaskItemStatus.Pending;

        var task = new TaskItem(title, description, dueDate, status);

        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be(title);
        task.Description.Should().Be(description);
        task.DueDate.Should().Be(dueDate);
        task.Status.Should().Be(status);
    }
}