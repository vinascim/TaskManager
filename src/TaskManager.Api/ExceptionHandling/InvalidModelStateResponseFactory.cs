using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TaskManager.Api.ExceptionHandling;

internal static class InvalidModelStateResponseFactory
{
    private const string BodyField = "body";

    public static IActionResult Create(ActionContext context)
    {
        var modelState = new ModelStateDictionary();

        foreach (var (key, entry) in context.ModelState)
        {
            if (entry.Errors.Count == 0)
                continue;

            var field = ToFieldName(key);
            modelState.AddModelError(field, ToMessage(field, key, entry.AttemptedValue));
        }

        var problemDetails = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(
                context.HttpContext,
                modelState,
                StatusCodes.Status400BadRequest,
                "Um ou mais erros de validação ocorreram.");

        return new BadRequestObjectResult(problemDetails);
    }

    private static string ToFieldName(string key)
    {
        var name = key.TrimStart('$', '.');
        return string.IsNullOrEmpty(name) ? BodyField : JsonNamingPolicy.CamelCase.ConvertName(name);
    }

    private static string ToMessage(string field, string key, string? attemptedValue)
    {
        if (field == BodyField)
            return "O corpo da requisição está vazio ou em formato inválido.";

        if (key.StartsWith('$'))
            return "Valor em formato inválido.";

        return $"O valor '{attemptedValue}' não é válido.";
    }
}
