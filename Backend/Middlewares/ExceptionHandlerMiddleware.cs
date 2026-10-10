using MbyteWeb.Server.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MbyteWeb.Server.Middlewares
{
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        private readonly RequestDelegate next = next;
        private readonly ILogger<ExceptionHandlerMiddleware> logger = logger;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex) {
                HandleError(ex, out int code, out string message);
                if (code == 500) logger.LogError("{Message}", ex);
                else logger.LogInformation("{Message}", ex);
                httpContext.Response.StatusCode = code;
                await httpContext.Response.WriteAsync(message);
            }
        }

        private void HandleError(Exception ex, out int statusCode, out string message) {
            switch (ex) {
                case NotFoundException:
                    message = ex.Message;
                    statusCode = 404;
                    break;
                case BadRequestException:
                    message = ex.Message;
                    statusCode = 400;
                    break;
                case UnauthorizedException:
                    message = ex.Message;
                    statusCode = 401;
                    break;
                case ForbiddenException:
                    message = ex.Message;
                    statusCode = 403;
                    break;
                case PostgresException postgresException:
                    HandlePostgresExceptions(postgresException, out statusCode, out message);
                    break;
                case DbUpdateException dbUpdateException:
                    if (dbUpdateException.InnerException != null) {
                        HandleError(dbUpdateException.InnerException, out statusCode, out message);
                    } else {
                        message = "Hubo un error inesperado";
                        statusCode = 500;
                    }
                    break;
                case NpgsqlException npgsqlException:
                    if (npgsqlException.InnerException != null) {
                        HandleError(npgsqlException.InnerException, out statusCode, out message);
                    } else {
                        message = "Hubo un error inesperado";
                        statusCode = 500;
                    }
                    break;
                default:
                    message = "Hubo un error inesperado";
                    statusCode = 500;
                    break;
            }
        }

        private static void HandlePostgresExceptions(PostgresException ex, out int statusCode, out string message) {
            var code = ex.SqlState;
            if (code == "23505") { 
                statusCode = 400;
                message = "Ya existe un registro con estos datos";
            }else if (code.StartsWith("22") || code.StartsWith("23")) {
                statusCode = 400;
                message = "Parámetros inválidos";
            } else {
                statusCode = 500;
                message = "Hubo un error inesperado";
            }
        }
    }
}
