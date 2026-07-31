using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger
        )
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
            catch (ConflictException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    title: "Conflito de dados",
                    detail: ex.Message
                );
            }
            catch (NotFoundException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    title: "Recurso Não Encontrado",
                    detail: ex.Message
                );
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (DomainException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status422UnprocessableEntity,
                    title: "Regra de Negócio Violada",
                    detail: ex.Message
                );
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    title: "Requisição Inválida",
                    detail: ex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no servidor");

                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    title: "Ocorreu um erro interno",
                    detail: "Ocorreu um erro inesperado no servidor. Tente novamente mais tarde."
                );
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail
        )
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path,
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static async Task HandleValidationExceptionAsync(
            HttpContext context,
            ValidationException ex
        )
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetail = new HttpValidationProblemDetails(ex.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de validação",
                Detail = ex.Message,
                Instance = context.Request.Path,
            };

            await context.Response.WriteAsJsonAsync(problemDetail);
        }
    }
}
