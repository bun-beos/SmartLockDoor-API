namespace SmartLockDoor
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next) { _next = next; }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            Console.WriteLine(exception);
            context.Response.ContentType = "application/json";

            if (exception is NotFoundException notFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;

                await context.Response.WriteAsync(text: new BaseException()
                {
                    DevMessage = notFoundException.DevMessage,
                    UserMessage = notFoundException.UserMessage,
                }.ToString() ?? "");
            }
            else if (exception is ConflictException conflictException)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;

                await context.Response.WriteAsync(text: new BaseException()
                {
                    DevMessage = conflictException.DevMessage,
                    UserMessage = conflictException.UserMessage,
                }.ToString() ?? "");
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsync(text: new BaseException()
                {
                    UserMessage = "Lỗi hệ thống.",
#if DEBUG
                    DevMessage = exception.Message,
#else
                    DevMessage = "",
#endif
                }.ToString() ?? "");
            }
        }
    }
}
