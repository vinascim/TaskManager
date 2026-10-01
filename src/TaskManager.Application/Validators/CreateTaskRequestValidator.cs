using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status inválido.");
    }
}