using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using Owin;
using Owin.Security.Providers.GitHub;
using DK1.Models;

namespace DK1
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager, and sign-in manager
            app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(60), // Set explicit expiration
                SlidingExpiration = true,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },
                CookieManager = new Microsoft.Owin.Host.SystemWeb.SystemWebCookieManager()
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure two-factor authentication cookies
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Configure GitHub authentication
            var githubClientId = System.Configuration.ConfigurationManager.AppSettings["GitHub:ClientId"];
            var githubClientSecret = System.Configuration.ConfigurationManager.AppSettings["GitHub:ClientSecret"];

            if (!string.IsNullOrEmpty(githubClientId) && !string.IsNullOrEmpty(githubClientSecret))
            {
                app.UseGitHubAuthentication(new GitHubAuthenticationOptions
                {
                    ClientId = githubClientId,
                    ClientSecret = githubClientSecret,
                    CallbackPath = new PathString("/signin-github"),
                    Scope = { "user:email" },
                    BackchannelTimeout = TimeSpan.FromSeconds(60),
                    Provider = new GitHubAuthenticationProvider
                    {
                        OnAuthenticated = async (context) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"GitHub Auth: ID={context.Id}, UserName={context.UserName ?? "null"}, Email={context.Email ?? "null"}");

                            // Add claims
                            context.Identity.AddClaim(new System.Security.Claims.Claim("GitHubId", context.Id ?? "unknown"));
                            context.Identity.AddClaim(new System.Security.Claims.Claim("GitHubUserName", context.UserName ?? "unknown"));

                            if (!string.IsNullOrEmpty(context.Email))
                            {
                                context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, context.Email));
                                context.Identity.AddClaim(new System.Security.Claims.Claim("GitHubEmail", context.Email));
                                System.Diagnostics.Debug.WriteLine($"GitHub Email Added: {context.Email}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("GitHub Email: Not provided");
                                context.Identity.AddClaim(new System.Security.Claims.Claim("EmailRequired", "true"));
                            }

                            await Task.FromResult(0);
                        },
                        OnReturnEndpoint = (context) =>
                        {
                            System.Diagnostics.Debug.WriteLine("GitHub Return Endpoint Called");
                            return Task.FromResult(0);
                        }
                    }
                });
            }

            // Configure Google authentication
            var googleClientId = System.Configuration.ConfigurationManager.AppSettings["Google:ClientId"];
            var googleClientSecret = System.Configuration.ConfigurationManager.AppSettings["Google:ClientSecret"];

            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                {
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret,
                    Scope = { "profile", "email" },
                    CallbackPath = new PathString("/signin-google")
                });
            }

            // Configure Facebook authentication
            var facebookAppId = System.Configuration.ConfigurationManager.AppSettings["Facebook:AppId"];
            var facebookAppSecret = System.Configuration.ConfigurationManager.AppSettings["Facebook:AppSecret"];

            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
            {
                app.UseFacebookAuthentication(new FacebookAuthenticationOptions
                {
                    AppId = facebookAppId,
                    AppSecret = facebookAppSecret,
                    CallbackPath = new PathString("/signin-facebook"),
                    Scope = { "public_profile", "email" },
                    BackchannelTimeout = TimeSpan.FromSeconds(60),
                    Provider = new FacebookAuthenticationProvider
                    {
                        OnAuthenticated = async (context) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Facebook Auth Success: {context.Name} ({context.Id})");

                            // Add claims
                            context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookId", context.Id));
                            context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookName", context.Name ?? "Unknown"));

                            if (!string.IsNullOrEmpty(context.Email))
                            {
                                context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, context.Email));
                                context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookEmail", context.Email));
                                System.Diagnostics.Debug.WriteLine($"Facebook Email: {context.Email}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Facebook Email: Not provided");
                                context.Identity.AddClaim(new System.Security.Claims.Claim("EmailRequired", "true"));
                            }

                            await Task.FromResult(0);
                        },
                        OnReturnEndpoint = (context) =>
                        {
                            System.Diagnostics.Debug.WriteLine("Facebook Return Endpoint Called");
                            return Task.FromResult(0);
                        }
                    }
                });
            }
        }
    }
}