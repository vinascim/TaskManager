using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.IntegrationTests.Infrastructure;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.IntegrationTests.Tasks;

public class ListTasksTests : ApiTestBase
{
    public ListTasksTests(TaskManagerApiFactory factory) : base(factory) { }

    [Fact]
    public async Task List_FilteredByStatus_ShouldReturnOnlyMatchingTasks()
    {
        // Arrange
        var tag = UniqueTag();
        var pending = await CreateTaskAsync($"Pendente {tag}", status: TaskItemStatus.Pending);
        await CreateTaskAsync($"Concluída {tag}", status: TaskItemStatus.Completed);

        // Act
        var tasks = await ListAsync($"?search={tag}&status=Pending");

        // Assert
        tasks.Should().ContainSingle().Which.Id.Should().Be(pending.Id);
    }

    [Fact]
    public async Task List_FilteredByDueDateRange_ShouldReturnTasksInsideRange()
    {
        // Arrange
        var tag = UniqueTag();
        await CreateTaskAsync($"Antes {tag}", dueDate: new DateOnly(2026, 1, 10));
        var inside = await CreateTaskAsync($"Dentro {tag}", dueDate: new DateOnly(2026, 6, 15));
        await CreateTaskAsync($"Depois {tag}", dueDate: new DateOnly(2026, 12, 20));
        await CreateTaskAsync($"Sem data {tag}");

        // Act
        var tasks = await ListAsync($"?search={tag}&dueDateFrom=2026-06-01&dueDateTo=2026-06-30");

        // Assert
        tasks.Should().ContainSingle().Which.Id.Should().Be(inside.Id);
    }

    [Fact]
    public async Task List_WithSearch_ShouldMatchTitleAndDescriptionIgnoringCase()
    {
        // Arrange
        var tag = UniqueTag();
        var byTitle = await CreateTaskAsync($"Implementar LOGIN {tag}");
        var byDescription = await CreateTaskAsync($"Revisar código {tag}", description: "Validar fluxo de login");
        await CreateTaskAsync($"Configurar deploy {tag}");

        // Act
        var tasks = await ListAsync($"?search=login");

        // Assert
        tasks.Select(t => t.Id).Should().Contain(new[] { byTitle.Id, byDescription.Id });
        tasks.Should().OnlyContain(t =>
            t.Title.Contains("login", StringComparison.OrdinalIgnoreCase) ||
            (t.Description != null && t.Description.Contains("login", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task List_ShouldOrderByDueDateWithUndatedTasksLast()
    {
        // Arrange
        var tag = UniqueTag();
        var undated = await CreateTaskAsync($"A sem data {tag}");
        var later = await CreateTaskAsync($"B mais tarde {tag}", dueDate: new DateOnly(2026, 12, 1));
        var sooner = await CreateTaskAsync($"C mais cedo {tag}", dueDate: new DateOnly(2026, 3, 1));

        // Act
        var tasks = await ListAsync($"?search={tag}");

        // Assert
        tasks.Select(t => t.Id).Should().Equal(sooner.Id, later.Id, undated.Id);
    }

    [Fact]
    public async Task List_WithInvertedDateRange_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync($"{TasksUrl}?dueDateFrom=2026-12-31&dueDateTo=2026-01-01");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKey("dueDateTo");
    }

    [Fact]
    public async Task List_WithInvalidDateFormat_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync($"{TasksUrl}?dueDateFrom=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKey("dueDateFrom");
    }

    [Fact]
    public async Task List_WithoutPaginationParameters_ShouldUseDefaults()
    {
        // Arrange
        var tag = UniqueTag();
        await CreateTaskAsync($"Tarefa {tag}");

        // Act
        var page = await ListPageAsync($"?search={tag}");

        // Assert
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(TaskFilter.DefaultPageSize);
        page.TotalCount.Should().Be(1);
        page.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task List_WithPagination_ShouldReturnRequestedPageAndMetadata()
    {
        // Arrange
        var tag = UniqueTag();
        var created = new List<TaskResponse>();
        for (var day = 1; day <= 5; day++)
            created.Add(await CreateTaskAsync($"Tarefa {day} {tag}", dueDate: new DateOnly(2026, 1, day)));

        // Act
        var page = await ListPageAsync($"?search={tag}&page=2&pageSize=2");

        // Assert
        page.Items.Select(t => t.Id).Should().Equal(created[2].Id, created[3].Id);
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(2);
        page.TotalCount.Should().Be(5);
        page.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task List_WithPageBeyondLast_ShouldReturnEmptyItemsWithTotalCount()
    {
        // Arrange
        var tag = UniqueTag();
        await CreateTaskAsync($"Tarefa {tag}");

        // Act
        var page = await ListPageAsync($"?search={tag}&page=10");

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task List_WithPageSizeAboveMaximum_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync($"{TasksUrl}?pageSize={TaskFilter.MaxPageSize + 1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem!.Errors.Should().ContainKey("pageSize");
    }

    private async Task<IReadOnlyList<TaskResponse>> ListAsync(string query) =>
        (await ListPageAsync(query)).Items;

    private async Task<PagedResult<TaskResponse>> ListPageAsync(string query)
    {
        var response = await Client.GetAsync($"{TasksUrl}{query}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<TaskResponse>>(JsonOptions);
        return page!;
    }
}
