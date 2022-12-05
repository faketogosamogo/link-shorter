using System.Net;
using FluentValidation;
using LinkShorter.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinkShorter.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ExceptionFilter>>();

        switch (context.Exception)
        {
            case ValidationException ex:
                var validationDetails = new ValidationProblemDetails(
                    ex.Errors
                        .GroupBy(c => c.PropertyName)
                        .ToDictionary(c => c.Key, c => c.Select(x => x.ErrorMessage).ToArray()))
                {
                    Title = ex.Message,
                };

                context.Result = new BadRequestObjectResult(validationDetails);
                
                break;
            
            case ShortLinkNotFoundException ex:
                var details = new ProblemDetails()
                {
                    Title = ex.Message,
                };
                
                context.Result = new NotFoundObjectResult(details);
                
                break;
            
            case ApplicationBaseException ex:
                var applicationExceptionDetails = new ProblemDetails()
                {
                    Title = ex.Message
                };
                
                context.Result = new BadRequestObjectResult(applicationExceptionDetails);
                
                break;

            case Exception ex:
                var error = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Hidden",
                };

                logger.LogError(ex, ex.Message);
                
                context.Result = new ObjectResult(error) { StatusCode = error.Status };
                
                break;
        }
    }
}