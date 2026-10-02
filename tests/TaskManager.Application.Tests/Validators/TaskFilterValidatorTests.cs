using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Validators;

public class TaskFilterValidatorTests
{
    private readonly TaskFilterValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyFilter_ShouldNotHaveErrors()
    {
        // Act
        var result = _validator.TestValidate(new TaskFilter());

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllFieldsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var filter = new TaskFilter(TaskItemStatus.InProgress, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "estudar");

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSameDueDateFromAndTo_ShouldNotHaveErrors()
    {
        // Arrange
        var date = new DateOnly(2026, 6, 15);
        var filter = new TaskFilter(DueDateFrom: date, DueDateTo: date);

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDueDateToBeforeDueDateFrom_ShouldHaveError()
    {
        // Arrange
        var filter = new TaskFilter(DueDateFrom: new DateOnly(2026, 12, 31), DueDateTo: new DateOnly(2026, 1, 1));

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DueDateTo)
            .WithErrorMessage("A data final deve ser maior ou igual à data inicial.");
    }

    [Fact]
    public void Validate_WithOnlyDueDateTo_ShouldNotHaveErrors()
    {
        // Arrange
        var filter = new TaskFilter(DueDateTo: new DateOnly(2026, 1, 1));

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveError()
    {
        // Arrange
        var filter = new TaskFilter(Status: (TaskItemStatus)99);

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithSearchExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var filter = new TaskFilter(Search: new string('a', TaskFilterValidator.SearchMaxLength + 1));

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithPageLessThanOne_ShouldHaveError(int page)
    {
        // Arrange
        var filter = new TaskFilter(Page: page);

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(TaskFilter.MaxPageSize + 1)]
    public void Validate_WithPageSizeOutOfRange_ShouldHaveError(int pageSize)
    {
        // Arrange
        var filter = new TaskFilter(PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(TaskFilter.MaxPageSize)]
    public void Validate_WithPageSizeAtLimits_ShouldNotHaveErrors(int pageSize)
    {
        // Arrange
        var filter = new TaskFilter(PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(filter);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}