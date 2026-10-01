using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Validators;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateTaskRequest("Primeira tarefa", "Descrição", new DateOnly(2026, 12, 31), TaskItemStatus.Pending);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyTitle_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateTaskRequest("Primeira tarefa", null, null, null);

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
        var request = new CreateTaskRequest(title!, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("O título é obrigatório.");
    }

    [Fact]
    public void Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskRequest(new string('a', TaskItem.TitleMaxLength + 1), null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskRequest("Título", new string('a', TaskItem.DescriptionMaxLength + 1), null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskRequest("Título", null, null, (TaskItemStatus)99);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status inválido.");
    }

    [Fact]
    public void Validate_WithPastDueDate_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new CreateTaskRequest("Título", null, new DateOnly(2000, 1, 1), null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}