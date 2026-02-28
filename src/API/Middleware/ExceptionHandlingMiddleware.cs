using Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading;

namespace API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        ProblemDetails problem = exception switch
        {
            ValidationException ex => CreateValidationProblem(ex, context),
            BadRequestException ex => CreateProblem(StatusCodes.Status400BadRequest, "Bad Request", ex.Message, context),
            NotFoundException ex => CreateProblem(StatusCodes.Status404NotFound, "Not Found", ex.Message, context),
            DuplicatedException ex => CreateProblem(StatusCodes.Status409Conflict, "Conflict", ex.Message, context),
            JsonException ex => CreateProblem(StatusCodes.Status400BadRequest, "Invalid JSON", ex.Message, context),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "Internal Server Error",
                                         logger.IsEnabled(LogLevel.Debug) ? exception.Message : "Contacte al administrador", context)
        };

        LogExceptionWithContext(exception);

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail, HttpContext context) =>
        new()
        {
            Type = GetProblemTypeUri(status),
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

    private static ValidationProblemDetails CreateValidationProblem(ValidationException ex, HttpContext context)
    {
        Dictionary<string, string[]> errors = ex.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Type = GetProblemTypeUri(StatusCodes.Status400BadRequest),
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = "La solicitud no es válida.",
            Instance = context.Request.Path
        };
    }

    private static string GetProblemTypeUri(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        _ => "about:blank"
    };

    private void LogExceptionWithContext(Exception exception)
    {
        switch (exception)
        {
            case ValidationException:
                logger.LogWarning(exception, "Error de validación: {Message}", exception.Message);
                break;
            case BadRequestException:
                logger.LogWarning("Solicitud incorrecta: {Message}", exception.Message);
                break;
            case NotFoundException:
                logger.LogWarning("Recurso no encontrado: {Message}", exception.Message);
                break;
            case DuplicatedException:
                logger.LogWarning("Recurso duplicado: {Message}", exception.Message);
                break;
            default:
                logger.LogError(exception, "Error inesperado: {Type} - {Message}", exception.GetType().Name, exception.Message);
                break;
        }
    }
}