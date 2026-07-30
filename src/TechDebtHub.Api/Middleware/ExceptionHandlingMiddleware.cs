using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
            catch (NotFoundException exception)
            {
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, exception.Message);
            }
            catch (DomainException exception)
            {
                await WriteResponseAsync(
                    context,
                    StatusCodes.Status422UnprocessableEntity,
                    exception.Message
                );
            }
            catch (ArgumentException exeption)
            {
                await WriteResponseAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    exeption.Message
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ocorreu um erro não tratado");

                await WriteResponseAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno"
                );
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            int statusCode,
            string message
        )
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new { status = statusCode, error = message });
        }
    }
}
