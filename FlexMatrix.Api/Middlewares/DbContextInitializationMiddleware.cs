using FlexMatrix.Api.Data.DataBase;

namespace FlexMatrix.Api.Middlewares
{
    public class DbContextInitializationMiddleware
    {
        private readonly RequestDelegate _next;

        public DbContextInitializationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) // , ApplicationDbContext dbContext
        {
            //await dbContext.InitializeAsync();
            await _next(context);
        }
    }
}
