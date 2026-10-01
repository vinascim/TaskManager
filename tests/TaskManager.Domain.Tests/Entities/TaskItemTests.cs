using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Tests.Entities;

public class TaskItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var title = "Primeira tarefa";
        var description = "Implementar a primeira tarefa do sistema";
        var dueDate = new DateOnly(2026, 12, 31);
        var status = TaskItemStatus.Pending;

        // Act
        var task = TaskItem.Create(title, description, dueDate, status);

        // Assert
        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be(title);
        task.Description.Should().Be(description);
        task.DueDate.Should().Be(dueDate);
        task.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrowDomainException(string? title)
    {
        // Act
        var act = () => TaskItem.Create(title!, null, null, TaskItemStatus.Pending);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("O título é obrigatório.");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldThrowDomainException()
    {
        // Arrange
        var title = new string('a', TaskItem.TitleMaxLength + 1);

        // Act
        var act = () => TaskItem.Create(title, null, null, TaskItemStatus.Pending);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithTitleAtMaxLength_ShouldCreateTask()
    {
        // Arrange
        var title = new string('a', TaskItem.TitleMaxLength);

        // Act
        var task = TaskItem.Create(title, null, null, TaskItemStatus.Pending);

        // Assert
        task.Title.Should().HaveLength(TaskItem.TitleMaxLength);
    }

    [Fact]
    public void Create_WithTitleSurroundedBySpaces_ShouldTrimTitle()
    {
        // Act
        var task = TaskItem.Create("  Primeira Tarefa  ", null, null, TaskItemStatus.Pending);

        // Assert
        task.Title.Should().Be("Primeira Tarefa");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ShouldSetDescriptionToNull(string? description)
    {
        // Act
        var task = TaskItem.Create("Título", description, null, TaskItemStatus.Pending);

        // Assert
        task.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrowDomainException()
    {
        // Arrange
        var description = new string('a', TaskItem.DescriptionMaxLength + 1);

        // Act
        var act = () => TaskItem.Create("Título", description, null, TaskItemStatus.Pending);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithInvalidStatus_ShouldThrowDomainException()
    {
        // Act
        var act = () => TaskItem.Create("Título", null, null, (TaskItemStatus)99);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Status inválido.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var task = TaskItem.Create("Título original", "Descrição original", new DateOnly(2026, 12, 31), TaskItemStatus.Pending);
        var originalId = task.Id;
        var newDueDate = new DateOnly(2027, 1, 15);

        // Act
        task.Update("Título novo", "Descrição nova", newDueDate, TaskItemStatus.InProgress);

        // Assert
        task.Id.Should().Be(originalId);
        task.Title.Should().Be("Título novo");
        task.Description.Should().Be("Descrição nova");
        task.DueDate.Should().Be(newDueDate);
        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void Update_WithNullDueDate_ShouldClearDueDate()
    {
        // Arrange
        var task = TaskItem.Create("Título", null, new DateOnly(2026, 12, 31), TaskItemStatus.Pending);

        // Act
        task.Update("Título", null, null, TaskItemStatus.Pending);

        // Assert
        task.DueDate.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyTitle_ShouldThrowDomainException(string? title)
    {
        // Arrange
        var task = TaskItem.Create("Título", null, null, TaskItemStatus.Pending);

        // Act
        var act = () => task.Update(title!, null, null, TaskItemStatus.Pending);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("O título é obrigatório.");
    }

    [Fact]
    public void Update_WithInvalidData_ShouldNotChangeAnyField()
    {
        // Arrange
        var dueDate = new DateOnly(2026, 12, 31);
        var task = TaskItem.Create("Título original", "Descrição original", dueDate, TaskItemStatus.Pending);

        // Act
        var act = () => task.Update("Título novo", "Descrição nova", null, (TaskItemStatus)99);

        // Assert
        act.Should().Throw<DomainException>();
        task.Title.Should().Be("Título original");
        task.Description.Should().Be("Descrição original");
        task.DueDate.Should().Be(dueDate);
        task.Status.Should().Be(TaskItemStatus.Pending);
    }
}