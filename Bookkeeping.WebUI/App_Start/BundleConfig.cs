using System.Web.Optimization;

namespace Bookkeeping.WebUi
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region scripts

            #region common

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/common").Include(
                "~/Scripts/app/common.js"));

            #endregion common

            #region login

            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                      "~/Scripts/app/login/login.js"));

            #endregion login

            #region manager

            bundles.Add(new ScriptBundle("~/bundles/manager-tasks").Include(
                      "~/Scripts/app/manager/managerTasks.js",
                      "~/Scripts/download.js"));

            #endregion manager

            #region administration

            bundles.Add(new ScriptBundle("~/bundles/administration-agents").Include(
                      "~/Scripts/app/administration/agents.js"));

            #endregion administration

            #region agent

            bundles.Add(new ScriptBundle("~/bundles/agent-tasks").Include(
                      "~/Scripts/app/agent/agentTasks.js"));

            bundles.Add(new ScriptBundle("~/bundles/agent-resolution").Include(
                      "~/Scripts/app/agent/agentResolution.js"));

            #endregion agent

            #endregion scripts

            #region styles
            //error
            bundles.Add(new StyleBundle("~/Content/css/error").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/error.css"));
            
            //login
            bundles.Add(new StyleBundle("~/Content/css/login").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/common.css",
                      "~/Content/css/login.css"));

            //administration
            bundles.Add(new StyleBundle("~/Content/css/administration").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/common.css",
                      "~/Content/css/administration.css"));

            //manager
            bundles.Add(new StyleBundle("~/Content/css/manager").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/common.css",
                      "~/Content/css/manager.css"));

            //agent
            bundles.Add(new StyleBundle("~/Content/css/agent").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/common.css",
                      "~/Content/css/agent.css"));

            #endregion styles
        }
    }
}
