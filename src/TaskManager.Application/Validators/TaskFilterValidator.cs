using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public sealed class TaskFilterValidator : AbstractValidator<TaskFilter>
{
    public const int SearchMaxLength = 100;

    public TaskFilterValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status inválido.");

        RuleFor(x => x.DueDateTo)
            .Must((filter, dueDateTo) => dueDateTo >= filter.DueDateFrom)
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue)
            .WithMessage("A data final deve ser maior ou igual à data inicial.");

        RuleFor(x => x.Search)
            .MaximumLength(SearchMaxLength)
            .WithMessage($"A busca deve ter no máximo {SearchMaxLength} caracteres.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, TaskFilter.MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {TaskFilter.MaxPageSize}.");
    }
}