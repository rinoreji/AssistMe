using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AssistMe.Helpers
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Session["user"] != null &&
                UserConfig.DB.AssistMeDrive != null &&
                !string.IsNullOrWhiteSpace(UserConfig.DB.AssistMeDrive.Id);
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new { controller = "Account", action = "Login" }));
        }
    }
}