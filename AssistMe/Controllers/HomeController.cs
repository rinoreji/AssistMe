using AssistMe.Helpers;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.UrlText = UserConfig.User_Id;
            return View();
        }

        public ActionResult Drive()
        {
            return View();
        }
    }
}
