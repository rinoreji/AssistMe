using System.Web.Optimization;

namespace AssistMe
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jQuery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/metisMenu.js",
                        "~/Scripts/template.js",
                        "~/Scripts/typeahead.bundle.js"
                        ));

            bundles.Add(new StyleBundle("~/bundles/styles").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/metisMenu.css",
                "~/Content/template.css",
                "~/Content/typeahead.css"
                ));
        }
    }
}