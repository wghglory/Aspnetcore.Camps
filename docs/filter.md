# ActionFilter

Notice that now we write ModelState check everywhere (put/post) in controllers. 

```csharp
if (!ModelState.IsValid) return BadRequest(ModelState);
```

We may have some other common logic in future. It's much better to put these logic in ActionFilter. Let's create `Filters/ValidateModelAttribute.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aspnetcore.Camps.Api.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
```

Now put the attribute in the controller or action:

```csharp
[Route("api/[controller]")]
[ValidateModel]
public class CampsController : BaseController
```