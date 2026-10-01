using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.IntegrationTests.Infrastructure;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.IntegrationTests.Tasks;

public class CreateTaskTests : ApiTestBase
{
    public CreateTaskTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_WithValidRequest_ShouldReturnCreatedWithLocation()
    {
        // Arrange
        var request = new CreateTaskRequest("Implementar tela de login", "Formulário com e-mail e senha", new DateOnly(2026, 12, 31), TaskItemStatus.InProgress);

        // Act
        var response = await Client.PostAsJsonAsync(TasksUrl, request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        task!.Id.Should().NotBeEmpty();
        task.Title.Should().Be(request.Title);
        task.Description.Should().Be(request.Description);
        task.DueDate.Should().Be(request.DueDate);
        task.Status.Should().Be(TaskItemStatus.InProgress);

        response.Headers.Location!.AbsolutePath.Should().Be($"{TasksUrl}/{task.Id}");
    }

    [Fact]
    public async Task Post_WithOnlyTitle_ShouldCreatePendingTask()
    {
        // Act
        var task = await CreateTaskAsync("Configurar pipeline de deploy");

        // Assert
        task.Status.Should().Be(TaskItemStatus.Pending);
        task.Description.Should().BeNull();
        task.DueDate.Should().BeNull();
    }

    [Fact]
    public async Task Post_WithInvalidFields_ShouldReturnBadRequestWithErrorsPerField()
    {
        // Arrange
        var request = new CreateTaskRequest("", new string('a', 501), null, null);

        // Act
        var response = await Client.PostAsJsonAsync(TasksUrl, request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKeys("title", "description");
    }

    [Fact]
    public async Task Post_WithUnknownStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var body = new { title = "Implementar tela de login", status = "Unknown" };

        // Act
        var response = await Client.PostAsJsonAsync(TasksUrl, body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKey("status");
    }
}
