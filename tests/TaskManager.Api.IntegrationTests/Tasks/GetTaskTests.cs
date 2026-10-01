using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.IntegrationTests.Infrastructure;
using TaskManager.Application.DTOs;

namespace TaskManager.Api.IntegrationTests.Tasks;

public class GetTaskTests : ApiTestBase
{
    public GetTaskTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetById_WhenTaskExists_ShouldReturnTask()
    {
        // Arrange
        var created = await CreateTaskAsync("Implementar tela de login");

        // Act
        var response = await Client.GetAsync($"{TasksUrl}/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        task.Should().Be(created);
    }

    [Fact]
    public async Task GetById_WhenTaskDoesNotExist_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"{TasksUrl}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }
}
