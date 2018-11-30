using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DeaconCCGManagement.Infrastructure;
using Elmah;


namespace DeaconCCGManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Init the AutoMapper
            // AutoMapper maps the domain (data) models to the
            // view models and vice versa. e.g., Member ==> MemberViewModel
            AutoMapperBootstrapper.Initialize();          

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }

        private bool _sendEmail;
        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            if (e.Exception.GetBaseException() is HttpRequestValidationException) e.Dismiss();
        }

        void ErrorMail_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            if (e.Exception.GetBaseException() is HttpRequestValidationException) e.Dismiss();
        }

        void ErrorLog_Logged(object sender, ErrorLoggedEventArgs args)
        {
         
        }
    }
}
