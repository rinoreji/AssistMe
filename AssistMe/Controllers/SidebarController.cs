using AssistMe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    public class SidebarController : Controller
    {
        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new List<AFileInfo>();
            model.AddRange(UserConfig.DB.AssistMeDrive.Children.Where(f => f.IsFolder));
            return PartialView(model);
        }
    }
}
