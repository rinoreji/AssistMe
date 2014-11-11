using AssistMe.Helpers;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            return View();
            if (string.IsNullOrWhiteSpace(UserConfig.DB.AssistMeDrive.id))
                return await UpdateDatabase(cancellationToken);

            return View();
        }

        //[ChildActionOnly]
        public ActionResult Drive()
        {
            return View();
        }

        public async Task<ActionResult> UpdateDatabase(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "Assist Me"
                };

                var service = new DriveService(serviceInitializer);
                var oauthService = new Oauth2Service(serviceInitializer);
                HttpContext.Session["DriveService"] = service;
                HttpContext.Session["Oauth2Service"] = oauthService;

                var gDriveHelper = new GDriveHelper();
                gDriveHelper.InitBaseSystem();

                //ViewBag.Message =  // "FILE COUNT IS: " + list.Items.Count();
                return View();
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}
