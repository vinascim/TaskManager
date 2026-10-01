using FluentValidation;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Validators;

internal static class TaskValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(TaskItem.TitleMaxLength)
            .WithMessage($"O título deve ter no máximo {TaskItem.TitleMaxLength} caracteres.");

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .MaximumLength(TaskItem.DescriptionMaxLength)
            .WithMessage($"A descrição deve ter no máximo {TaskItem.DescriptionMaxLength} caracteres.");
}