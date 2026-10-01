using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.IntegrationTests.Infrastructure;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.IntegrationTests.Tasks;

public class UpdateTaskTests : ApiTestBase
{
    public UpdateTaskTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Put_WithValidRequest_ShouldUpdateAndPersistTask()
    {
        // Arrange
        var created = await CreateTaskAsync("Implementar tela de login", "Versão inicial", new DateOnly(2026, 12, 31));
        var request = new UpdateTaskRequest("Implementar tela de login", "Adicionar opção de lembrar usuário", new DateOnly(2027, 1, 15), TaskItemStatus.Completed);

        // Act
        var response = await Client.PutAsJsonAsync($"{TasksUrl}/{created.Id}", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var expected = new TaskResponse(created.Id, request.Title, request.Description, request.DueDate, TaskItemStatus.Completed);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        updated.Should().Be(expected);

        var persisted = await Client.GetFromJsonAsync<TaskResponse>($"{TasksUrl}/{created.Id}", JsonOptions);
        persisted.Should().Be(expected);
    }

    [Fact]
    public async Task Put_WithPastDueDate_ShouldBeAccepted()
    {
        // Arrange
        var created = await CreateTaskAsync("Implementar tela de login");
        var request = new UpdateTaskRequest(created.Title, null, new DateOnly(2020, 1, 1), TaskItemStatus.Completed);

        // Act
        var response = await Client.PutAsJsonAsync($"{TasksUrl}/{created.Id}", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_WithoutStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var created = await CreateTaskAsync("Implementar tela de login");
        var body = new { title = "Implementar tela de login" };

        // Act
        var response = await Client.PutAsJsonAsync($"{TasksUrl}/{created.Id}", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKey("status");
    }

    [Fact]
    public async Task Put_WhenTaskDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateTaskRequest("Implementar tela de login", null, null, TaskItemStatus.Pending);

        // Act
        var response = await Client.PutAsJsonAsync($"{TasksUrl}/{Guid.NewGuid()}", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
