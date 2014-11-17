using AssistMe.Helpers;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public async Task<ActionResult> Login(CancellationToken token)
        {
            return await UpdateDatabase(token);
        }

        public ActionResult Logout(bool LogoutGoogle = false)
        {
            UserConfig.ResetConfig();
            HttpContext.Session.Clear();
            if (LogoutGoogle)
                return new RedirectResult("https://www.google.com/accounts/Logout");
            else
                return View("Login");
        }

        private async Task<ActionResult> UpdateDatabase(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "AssistMe"
                };

                var service = new DriveService(serviceInitializer);
                var oauthService = new Oauth2Service(serviceInitializer);
                HttpContext.Session["DriveService"] = service;
                HttpContext.Session["Oauth2Service"] = oauthService;

                var gDriveHelper = new GDriveHelper();
                gDriveHelper.InitBaseSystem();

                //var data = await result.Credential.RefreshTokenAsync(cancellationToken);
                //ViewBag.Message =  // "FILE COUNT IS: " + list.Items.Count();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}
