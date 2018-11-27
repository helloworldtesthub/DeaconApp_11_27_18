using System;
using DeaconCCGManagement.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using DeaconCCGManagement.Models;

namespace DeaconCCGManagement
{
    public partial class Startup
    {
      
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and sign-in 
            // manager to use a single instance per request
            app.CreatePerOwinContext(CcgDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store 
            // information for the signed in user.
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Auth/SignIn"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security 
                    // stamp when the user logs in. 
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, CCGAppUser>(
                        validateInterval: TimeSpan.FromMinutes(30), 
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            
        }
    }
}