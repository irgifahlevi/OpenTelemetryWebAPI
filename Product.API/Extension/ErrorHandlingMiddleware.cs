using System.Text.Json;

namespace Product.API.Extension
{
    public class ErrorHandlingMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Menjalankan pipeline middleware selanjutnya
                await _next(context);
            }
            catch (AppException ex) // Pengecualian kustom untuk aturan bisnis
            {
                _logger.LogWarning(ex, "Business rule violated");
                await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex) // Pengecualian umum untuk kesalahan tak terduga
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, 500, "An internal server error occurred");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            // Menyiapkan respons error menggunakan ApiResponseHelper
            var response = ApiResponseHelper.Error<object>(message, statusCode);

            // Menetapkan status kode HTTP
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Menggunakan System.Text.Json untuk serialisasi
            var result = JsonSerializer.Serialize(response);

            // Mengirim response ke klien
            await context.Response.WriteAsync(result);
        }
    }
}
