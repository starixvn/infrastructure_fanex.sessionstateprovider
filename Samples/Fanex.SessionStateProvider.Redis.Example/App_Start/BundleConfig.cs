namespace Fanex.SessionStateProvider.Redis.Example
{
    using System.Web.Optimization;

    public static class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/lib/jquery/jquery.js",
                        "~/lib/twitter-bootstrap/js/bootstrap.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/lib/twitter-bootstrap/css/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}