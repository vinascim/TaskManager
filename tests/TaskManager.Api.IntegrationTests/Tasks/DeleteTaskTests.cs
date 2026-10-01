using System.Net;
using FluentAssertions;
using TaskManager.Api.IntegrationTests.Infrastructure;

namespace TaskManager.Api.IntegrationTests.Tasks;

public class DeleteTaskTests : ApiTestBase
{
    public DeleteTaskTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Delete_WhenTaskExists_ShouldReturnNoContentAndRemoveTask()
    {
        // Arrange
        var created = await CreateTaskAsync("Implementar tela de login");

        // Act
        var response = await Client.DeleteAsync($"{TasksUrl}/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"{TasksUrl}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenTaskDoesNotExist_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"{TasksUrl}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
