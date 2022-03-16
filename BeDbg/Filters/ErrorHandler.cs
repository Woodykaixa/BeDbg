using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using BeDbg.Filters;
using BeDbg.Models;
using Microsoft.AspNetCore.Http.Abstractions;

namespace BeDbg.Filters
{
	public class ErrorHandler
	{
		private readonly RequestDelegate next;

		public ErrorHandler(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				await WriteExceptionAsync(context, ex);
			}
		}

		private static async Task WriteExceptionAsync(HttpContext context, Exception exception)
		{
			var response = context.Response;
			var message = exception.InnerException == null ? exception.Message : exception.InnerException.Message;
			response.ContentType = "application/json";
			response.StatusCode = exception switch
			{
				UnauthorizedAccessException => 401,
				DirectoryNotFoundException => 404,
				_ => 500
			};

			await response.WriteAsJsonAsync(new ErrorResponse(message, exception.GetType().Name))
				.ConfigureAwait(false);
		}
	}
}

public static class ErrorHandlerMiddlewareExtensions
{
	public static IApplicationBuilder UseJsonErrorHandler(
		this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ErrorHandler>();
	}
}