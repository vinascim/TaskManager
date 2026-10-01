using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.IntegrationTests.Infrastructure;

public abstract class ApiTestBase : IClassFixture<TaskManagerApiFactory>
{
    protected const string TasksUrl = "/api/tasks";

    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected ApiTestBase(TaskManagerApiFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; }

    protected async Task<TaskResponse> CreateTaskAsync(
        string title,
        string? description = null,
        DateOnly? dueDate = null,
        TaskItemStatus? status = null)
    {
        var response = await Client.PostAsJsonAsync(TasksUrl, new CreateTaskRequest(title, description, dueDate, status), JsonOptions);
        response.EnsureSuccessStatusCode();

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        task.Should().NotBeNull();
        return task!;
    }

    protected static string UniqueTag() => Guid.NewGuid().ToString("N");
}
