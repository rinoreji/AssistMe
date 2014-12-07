using AssistMe.Helpers;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using log4net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private UserConfig userConfig
        {
            get
            {
                return (UserConfig)Session["UserConfig"];
            }
        }
        //
        // GET: /Account/

        public async Task<ActionResult> Login(CancellationToken token)
        {
            Session["UserConfig"] = new UserConfig();
            return await UpdateDatabase(token);
        }

        public ActionResult Logout(bool LogoutGoogle = false)
        {
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

                userConfig.GoogleAccessToken = result.Credential.Token.AccessToken;
                Session["UserConfig"] = userConfig;

                var service = new DriveService(serviceInitializer);
                var oauthService = new Oauth2Service(serviceInitializer);
                HttpContext.Session["DriveService"] = service;
                HttpContext.Session["Oauth2Service"] = oauthService;

                var gDriveHelper = new GDriveHelper();
                gDriveHelper.InitBaseSystem();

                if (log.IsDebugEnabled)
                    log.DebugFormat("Base init completed for user: {0}", userConfig.User_Id);

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
