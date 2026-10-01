using System.Net;
using FluentAssertions;
using TaskManager.Api.IntegrationTests.Infrastructure;

namespace TaskManager.Api.IntegrationTests;

public class HealthCheckTests : ApiTestBase
{
    public HealthCheckTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }
}
