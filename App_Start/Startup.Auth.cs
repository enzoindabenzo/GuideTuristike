using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using DK1.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

[assembly: OwinStartup(typeof(DK1.Startup))]

namespace DK1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager, and sign-in manager
            app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // IMPORTANT: Order matters! Set up cookie authentication FIRST
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                SlidingExpiration = true,
                CookieName = "YourAppAuth", // Give it a specific name
                CookieSecure = CookieSecureOption.SameAsRequest,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            // External sign-in cookie
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Two-factor authentication cookies
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Configure external providers
            ConfigureAzureADAuthentication(app);
            ConfigureGoogleAuthentication(app);
            ConfigureFacebookAuthentication(app);
        }

        private void ConfigureAzureADAuthentication(IAppBuilder app)
        {
            try
            {
                var clientId = System.Configuration.ConfigurationManager.AppSettings["Azure:ClientId"];
                var clientSecret = System.Configuration.ConfigurationManager.AppSettings["Azure:ClientSecret"];
                var authority = System.Configuration.ConfigurationManager.AppSettings["Azure:Authority"];

                System.Diagnostics.Debug.WriteLine($"Azure AD Config - ClientId: {clientId ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"Azure AD Config - Authority: {authority ?? "NULL"}");

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(authority))
                {
                    app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Authority = authority,
                        PostLogoutRedirectUri = "https://localhost:44352/",
                        ResponseType = OpenIdConnectResponseType.CodeIdToken,
                        Scope = OpenIdConnectScope.OpenIdProfile + " email",
                        TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = false, // Set to false for multi-tenant
                            NameClaimType = "name"
                        },
                        Notifications = new OpenIdConnectAuthenticationNotifications
                        {
                            AuthenticationFailed = context =>
                            {
                                System.Diagnostics.Debug.WriteLine($"Azure AD Auth Failed: {context.Exception?.Message}");
                                context.OwinContext.Response.Redirect("/Account/Login?error=azure_failed");
                                context.HandleResponse();
                                return Task.FromResult(0);
                            },
                            AuthorizationCodeReceived = context =>
                            {
                                System.Diagnostics.Debug.WriteLine("Azure AD: Authorization code received");
                                return Task.FromResult(0);
                            },
                            SecurityTokenValidated = context =>
                            {
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine("Azure AD: Token validated");
                                    var identity = context.AuthenticationTicket.Identity;
                                    if (identity != null)
                                    {
                                        // Get claims from the token
                                        var email = identity.FindFirst("preferred_username")?.Value ??
                                                   identity.FindFirst("email")?.Value ??
                                                   identity.FindFirst("upn")?.Value;
                                        var name = identity.FindFirst("name")?.Value;
                                        var objectId = identity.FindFirst("oid")?.Value ??
                                                     identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                                        System.Diagnostics.Debug.WriteLine($"Azure AD Claims - Email: {email ?? "null"}, Name: {name ?? "null"}, ObjectId: {objectId ?? "null"}");

                                        // Add custom claims
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            identity.AddClaim(new System.Security.Claims.Claim("AzureEmail", email));
                                            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email));
                                        }
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            identity.AddClaim(new System.Security.Claims.Claim("AzureName", name));
                                        }
                                        if (!string.IsNullOrEmpty(objectId))
                                        {
                                            identity.AddClaim(new System.Security.Claims.Claim("AzureObjectId", objectId));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Azure AD SecurityTokenValidated Error: {ex.Message}");
                                }
                                return Task.FromResult(0);
                            }
                        }
                    });

                    System.Diagnostics.Debug.WriteLine("Azure AD authentication configured successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Azure AD configuration incomplete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Azure AD setup error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ConfigureGoogleAuthentication(IAppBuilder app)
        {
            try
            {
                var clientId = System.Configuration.ConfigurationManager.AppSettings["Google:ClientId"];
                var clientSecret = System.Configuration.ConfigurationManager.AppSettings["Google:ClientSecret"];

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Scope = { "profile", "email" }
                    });

                    System.Diagnostics.Debug.WriteLine("Google authentication configured");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Google auth error: {ex.Message}");
            }
        }

        private void ConfigureFacebookAuthentication(IAppBuilder app)
        {
            try
            {
                var appId = System.Configuration.ConfigurationManager.AppSettings["Facebook:AppId"];
                var appSecret = System.Configuration.ConfigurationManager.AppSettings["Facebook:AppSecret"];

                if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appSecret))
                {
                    app.UseFacebookAuthentication(new FacebookAuthenticationOptions
                    {
                        AppId = appId,
                        AppSecret = appSecret,
                        Scope = { "public_profile", "email" },
                        Provider = new FacebookAuthenticationProvider
                        {
                            OnAuthenticated = async context =>
                            {
                                if (context.Identity != null && !string.IsNullOrEmpty(context.Email))
                                {
                                    context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookEmail", context.Email));
                                    context.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, context.Email));
                                }
                                await Task.FromResult(0);
                            }
                        }
                    });

                    System.Diagnostics.Debug.WriteLine("Facebook authentication configured");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facebook auth error: {ex.Message}");
            }
        }
    }
}