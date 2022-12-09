using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Samples.Players.Middleware;

public class ModelAttributeValidationFilter : ActionFilterAttribute
{
    public ModelAttributeValidationFilter()
    {
        Order = int.MaxValue;
    }

    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
        if (actionContext.ModelState.IsValid == false)
        {
            actionContext.Result = new BadRequestObjectResult(actionContext.ModelState);
        }
    }
}
