using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new UpdateTaskRequest("Título", "Descrição", new DateOnly(2026, 12, 31), TaskItemStatus.Completed);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyTitle_ShouldHaveError(string? title)
    {
        // Arrange
        var request = new UpdateTaskRequest(title!, null, null, TaskItemStatus.Pending);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest(new string('a', TaskItem.TitleMaxLength + 1), null, null, TaskItemStatus.Pending);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest("Título", new string('a', TaskItem.DescriptionMaxLength + 1), null, TaskItemStatus.Pending);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithoutStatus_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest("Título", null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("O status é obrigatório.");
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest("Título", null, null, (TaskItemStatus)99);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}