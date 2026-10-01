using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status inválido.");
    }
}