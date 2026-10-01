using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Exceptions;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.ExceptionHandling;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            DomainException => CreateProblem(StatusCodes.Status400BadRequest, "Regra de negócio violada.", exception.Message),
            NotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Recurso não encontrado.", exception.Message),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "Erro interno do servidor.", "Ocorreu um erro inesperado.")
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais erros de validação ocorreram."
        };
    }
}