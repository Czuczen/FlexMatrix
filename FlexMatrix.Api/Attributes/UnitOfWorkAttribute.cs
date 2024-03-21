using FlexMatrix.Api.Data.DataBase;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlexMatrix.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class UnitOfWorkAttribute : Attribute, IAsyncActionFilter
    {
        private readonly ILogger<UnitOfWorkAttribute> _logger;

        public UnitOfWorkAttribute(ILogger<UnitOfWorkAttribute> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
            if (unitOfWork == null)
                throw new InvalidOperationException("IUnitOfWork service not registered.");

            await unitOfWork.BeginTransaction();

            try
            {
                var resultContext = await next();

                if (resultContext.Exception == null || resultContext.ExceptionHandled) // a nie !resultContext.ExceptionHandled  ??
                {
                    await unitOfWork.SaveTransaction();
                }
                else
                {
                    await unitOfWork.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransaction();
                _logger.LogError("The method for which the transaction was created throw exception", ex);
                throw;
            }
        }
    }
}
