using System.Net;
using ContaCerta.Application.Common.Exceptions;
using ContaCerta.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ContaCerta.Api.Middleware;

/// <summary>
/// Traduz excecoes das camadas internas para os codigos HTTP que o consumidor da API
/// realmente precisa distinguir: 400 para payload invalido, 404 para recurso
/// inexistente, 409 para conflito de estado, 422 para regra de negocio violada.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception excecao)
        {
            await TratarAsync(context, excecao);
        }
    }

    private async Task TratarAsync(HttpContext context, Exception excecao)
    {
        var (status, titulo) = excecao switch
        {
            ValidationAppException => (HttpStatusCode.BadRequest, "Erro de validacao"),
            NotFoundException => (HttpStatusCode.NotFound, "Recurso nao encontrado"),
            ConflictException => (HttpStatusCode.Conflict, "Conflito de estado"),
            DomainException => (HttpStatusCode.UnprocessableEntity, "Regra de negocio violada"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno")
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(excecao, "Erro nao tratado ao processar {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(excecao, "{Titulo} ao processar {Metodo} {Caminho}", titulo, context.Request.Method, context.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)status,
            Title = titulo,
            Detail = excecao.Message,
            Instance = context.Request.Path
        };

        if (excecao is ValidationAppException validationException)
        {
            problemDetails.Extensions["erros"] = validationException.Erros;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
