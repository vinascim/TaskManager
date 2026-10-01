using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(
            _repository,
            new CreateTaskRequestValidator(),
            new UpdateTaskRequestValidator(),
            new TaskFilterValidator(),
            NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldAddTaskAndReturnResponse()
    {
        // Arrange
        var request = new CreateTaskRequest("Primeira tarefa", "Descrição", new DateOnly(2026, 12, 31), TaskItemStatus.InProgress);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        response.Id.Should().NotBeEmpty();
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.DueDate.Should().Be(request.DueDate);
        response.Status.Should().Be(TaskItemStatus.InProgress);

        await _repository.Received(1).AddAsync(
            Arg.Is<TaskItem>(t => t.Id == response.Id && t.Title == request.Title),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithoutStatus_ShouldDefaultToPending()
    {
        // Arrange
        var request = new CreateTaskRequest("Primeira tarefa", null, null, null);

        // Act
        var response = await _service.CreateAsync(request);

        // Assert
        response.Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ShouldThrowValidationExceptionAndNotAddTask()
    {
        // Arrange
        var request = new CreateTaskRequest("", null, null, null);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTask()
    {
        // Arrange
        var task = TaskItem.Create("Primeira tarefa", null, null, TaskItemStatus.Pending);
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        // Act
        var response = await _service.GetByIdAsync(task.Id);

        // Assert
        response.Should().Be(TaskResponse.FromEntity(task));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        // Act
        var act = () => _service.GetByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ListAsync_WithValidFilter_ShouldReturnMappedTasks()
    {
        // Arrange
        var filter = new TaskFilter(Status: TaskItemStatus.Pending);
        var tasks = new List<TaskItem>
        {
            TaskItem.Create("Tarefa 1", null, null, TaskItemStatus.Pending),
            TaskItem.Create("Tarefa 2", null, null, TaskItemStatus.Pending)
        };
        _repository.ListAsync(filter, Arg.Any<CancellationToken>()).Returns(tasks);

        // Act
        var response = await _service.ListAsync(filter);

        // Assert
        response.Should().BeEquivalentTo(tasks.Select(TaskResponse.FromEntity));
    }

    [Fact]
    public async Task ListAsync_WithInvalidFilter_ShouldThrowValidationExceptionAndNotQueryRepository()
    {
        // Arrange
        var filter = new TaskFilter(DueDateFrom: new DateOnly(2026, 12, 31), DueDateTo: new DateOnly(2026, 1, 1));

        // Act
        var act = () => _service.ListAsync(filter);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().ListAsync(Arg.Any<TaskFilter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_ShouldUpdateAndReturnTask()
    {
        // Arrange
        var task = TaskItem.Create("Título original", null, null, TaskItemStatus.Pending);
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var request = new UpdateTaskRequest("Título novo", "Descrição nova", new DateOnly(2027, 1, 15), TaskItemStatus.Completed);

        // Act
        var response = await _service.UpdateAsync(task.Id, request);

        // Assert
        response.Should().Be(new TaskResponse(task.Id, "Título novo", "Descrição nova", new DateOnly(2027, 1, 15), TaskItemStatus.Completed));
        await _repository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ShouldThrowNotFoundExceptionAndNotUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);
        var request = new UpdateTaskRequest("Título", null, null, TaskItemStatus.Pending);

        // Act
        var act = () => _service.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidRequest_ShouldThrowValidationExceptionAndNotQueryRepository()
    {
        // Arrange
        var request = new UpdateTaskRequest("", null, null, TaskItemStatus.Pending);

        // Act
        var act = () => _service.UpdateAsync(Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_ShouldDeleteTask()
    {
        // Arrange
        var task = TaskItem.Create("Tarefa", null, null, TaskItemStatus.Pending);
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        // Act
        await _service.DeleteAsync(task.Id);

        // Assert
        await _repository.Received(1).DeleteAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ShouldThrowNotFoundExceptionAndNotDelete()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        // Act
        var act = () => _service.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }
}