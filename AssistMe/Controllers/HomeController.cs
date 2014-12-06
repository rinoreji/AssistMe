using AssistMe.Helpers;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
