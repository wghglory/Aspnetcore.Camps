using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aspnetcore.Camps.Api.Controllers
{
    public abstract class BaseController : Controller
    {
        public const string Urlhelper = "URLHELPER";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            context.HttpContext.Items[Urlhelper] = this.Url;
        }
    }
}