using AssistMe.Data;
using System.Collections.Generic;
using System.Web.Mvc;

namespace AssistMe.Controllers
{
    public class SidebarController : Controller
    {
        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new List<AFileInfo>();
            model.AddRange(DummyData.GetDummyStructure().Children);

            return PartialView(model);
        }
    }
}
