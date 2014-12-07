using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AssistMe.Helpers
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private UserConfig userConfig
        {
            get
            {
                return (UserConfig)HttpContext.Current.Session["UserConfig"];
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Session["user"] != null &&
                userConfig != null &&
                userConfig.DB.AssistMeDrive != null &&
                !string.IsNullOrWhiteSpace(userConfig.DB.AssistMeDrive.Id);
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new { controller = "Account", action = "Login" }));
        }
    }
}