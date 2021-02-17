using System.Web;
using System.Web.Optimization;

namespace SEPRET
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.min.js",
                      "~/Scripts/bootstrap-material-design.min.js",
                      "~/Scripts/perfect-scrollbar.jquery.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/sweetalert2.js",
                      "~/Scripts/jquery.validate.min.js",
                      "~/Scripts/jquery.bootstrap-wizard.js",
                      "~/Scripts/bootstrap-selectpicker.js",
                      "~/Scripts/bootstrap-datetimepicker.min.js",
                      "~/Scripts/jquery.dataTables.min.js",
                      "~/Scripts/bootstrap-tagsinput.js",
                      "~/Scripts/jasny-bootstrap.min.js",
                      "~/Scripts/fullcalendar.min.js",
                      "~/Scripts/jquery-jvectormap.js",
                      "~/Scripts/nouislider.min.js",
                      "~/Scripts/arrive.min.js",
                      "~/Scripts/chartist.min.js",
                      "~/Scripts/bootstrap-notify.js",
                      "~/Scripts/material-dashboard.js",
                      "~/Scripts/iv-viewer.min.js",
                      "~/Scripts/jquery.timeago.js",
                      "~/Scripts/jquery.timeago.es.js",
                      //"~/Scripts/jquery.kendo-pdf.js",
                      //"~/Scripts/jquery.kendo.all.min.js",
                      "~/Assets/js/Site.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/material-dashboard.css",
                      "~/Content/fontawesome.min.css",
                      "~/Content/iv-viewer.min.css",
                      "~/Content/kendo-pdf-visor.css",
                      "~/Content/site.css"));
        }
    }
}
