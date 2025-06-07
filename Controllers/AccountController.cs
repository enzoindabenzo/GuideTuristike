using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DK1.Models;
using System.Diagnostics;

namespace DK1.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        [HttpGet]
        [Route("signin-oidc")]
        public ActionResult SignInOidc()
        {
            // This will be handled by the OWIN middleware
            // Just redirect to home or login success page
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Clear any existing authentication
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(
                    DefaultAuthenticationTypes.ApplicationCookie,
                    DefaultAuthenticationTypes.ExternalCookie,
                    DefaultAuthenticationTypes.TwoFactorCookie,
                    DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie
                );
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return View("ForgotPasswordConfirmation");
                }
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(provider))
            {
                Debug.WriteLine("ExternalLogin: Provider is null or empty.");
                return RedirectToAction("Login");
            }

            Debug.WriteLine($"ExternalLogin: Provider={provider}, ReturnUrl={returnUrl ?? "null"}");

            try
            {
                // Clear any existing external authentication cookies
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                // Construct the callback URL
                var callbackUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }, Request.Url.Scheme);
                Debug.WriteLine($"ExternalLogin: CallbackUrl={callbackUrl}");

                var properties = new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                    IsPersistent = false
                };

                // Add provider-specific properties
                if (provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
                {
                    properties.Dictionary["auth_type"] = "rerequest";
                    properties.Dictionary["scope"] = "email,public_profile";
                }
                else if (provider.Contains("OpenIdConnect") || provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    properties.Dictionary["scope"] = "openid profile email";
                    properties.Dictionary["prompt"] = "select_account"; // Allow user to select account
                }

                Debug.WriteLine($"ExternalLogin: Triggering {provider} authentication challenge.");
                return new ChallengeResult(provider, properties);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExternalLogin: Error triggering challenge for {provider}, Exception={ex.Message}");
                TempData["ErrorMessage"] = $"Authentication with {provider} failed. Please try again.";
                return RedirectToAction("Login");
            }
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            try
            {
                Debug.WriteLine("ExternalLoginCallback: Starting callback processing...");
                Debug.WriteLine($"ExternalLoginCallback: Request URL={Request.Url?.ToString() ?? "null"}");
                Debug.WriteLine($"ExternalLoginCallback: QueryString={Request.QueryString?.ToString() ?? "null"}");

                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    Debug.WriteLine("ExternalLoginCallback: loginInfo is null. Possible causes: Cookie cleared, callback URL mismatch, invalid client ID/secret, or user cancelled.");
                    TempData["ErrorMessage"] = "External authentication failed. Please try again.";
                    return RedirectToAction("Login");
                }

                Debug.WriteLine($"ExternalLoginCallback: Provider={loginInfo.Login?.LoginProvider ?? "Unknown"}");
                Debug.WriteLine($"ExternalLoginCallback: ProviderKey={loginInfo.Login?.ProviderKey ?? "Unknown"}");
                Debug.WriteLine($"ExternalLoginCallback: DefaultUserName={loginInfo.DefaultUserName ?? "Unknown"}");
                Debug.WriteLine($"ExternalLoginCallback: Email={loginInfo.Email ?? "Unknown"}");

                // Log all claims
                var claims = loginInfo.ExternalIdentity?.Claims;
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        Debug.WriteLine($"ExternalLoginCallback: Claim - {claim.Type}: {claim.Value}");
                    }
                }
                else
                {
                    Debug.WriteLine("ExternalLoginCallback: No claims available.");
                }

                // Try to get email or fallback to a unique identifier
                var email = loginInfo.Email ??
                            loginInfo.ExternalIdentity?.FindFirstValue(ClaimTypes.Email) ??
                            loginInfo.ExternalIdentity?.FindFirstValue("preferred_username") ??
                            loginInfo.ExternalIdentity?.FindFirstValue("AzureEmail");

                // Handle Microsoft-specific fallback
                if (string.IsNullOrEmpty(email) && (loginInfo.Login?.LoginProvider?.Contains("OpenIdConnect") == true ||
                                                   loginInfo.Login?.LoginProvider?.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) == true))
                {
                    var objectId = loginInfo.ExternalIdentity?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
                    var upn = loginInfo.ExternalIdentity?.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
                    var name = loginInfo.ExternalIdentity?.FindFirstValue(ClaimTypes.Name);

                    email = upn ??
                            name ??
                            loginInfo.DefaultUserName ??
                            $"azureuser_{objectId ?? Guid.NewGuid().ToString()}";
                    Debug.WriteLine($"ExternalLoginCallback: No email, using Microsoft fallback: {email}");
                }

                if (string.IsNullOrEmpty(email))
                {
                    Debug.WriteLine("ExternalLoginCallback: No email or username could be retrieved.");
                    TempData["ErrorMessage"] = $"Unable to retrieve email or username from {loginInfo.Login?.LoginProvider}. Please ensure your account has a verified email address.";
                    return View("ExternalLoginFailure");
                }

                Debug.WriteLine($"ExternalLoginCallback: Using email/username: {email}");

                // Try to sign in
                var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
                Debug.WriteLine($"ExternalLoginCallback: SignIn result: {result}");

                switch (result)
                {
                    case SignInStatus.Success:
                        Debug.WriteLine("ExternalLoginCallback: Sign-in successful, redirecting...");
                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        Debug.WriteLine("ExternalLoginCallback: Account locked out.");
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        Debug.WriteLine("ExternalLoginCallback: Requires verification.");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                    case SignInStatus.Failure:
                    default:
                        Debug.WriteLine("ExternalLoginCallback: Sign-in failed, showing confirmation page.");
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.LoginProvider = loginInfo.Login?.LoginProvider;
                        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExternalLoginCallback: Unexpected error: {ex.Message}");
                Debug.WriteLine($"ExternalLoginCallback: Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An unexpected error occurred during authentication. Please try again.";
                return RedirectToAction("Login");
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    Debug.WriteLine("ExternalLoginConfirmation: loginInfo is null");
                    return View("ExternalLoginFailure");
                }

                // Check if user already exists with this email
                var existingUser = await UserManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Add the external login to existing user
                    var addLoginResult = await UserManager.AddLoginAsync(existingUser.Id, info.Login);
                    if (addLoginResult.Succeeded)
                    {
                        await SignInManager.SignInAsync(existingUser, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        AddErrors(addLoginResult);
                    }
                }
                else
                {
                    // Create new user
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var result = await UserManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await UserManager.AddLoginAsync(user.Id, info.Login);
                        if (result.Succeeded)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    AddErrors(result);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie,
                DefaultAuthenticationTypes.TwoFactorCookie,
                DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie
            );

            // Clear session
            Session.Clear();
            Session.Abandon();

            // Clear response cookies
            Response.Cookies.Clear();

            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            Debug.WriteLine("ExternalLoginFailure: Authentication failed.");
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get => HttpContext.GetOwinContext().Authentication;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, AuthenticationProperties properties)
            {
                LoginProvider = provider;
                Properties = properties;
            }

            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                LoginProvider = provider;
                Properties = new AuthenticationProperties { RedirectUri = redirectUri };

                if (!string.IsNullOrEmpty(userId))
                {
                    Properties.Dictionary["XsrfId"] = userId;
                }
            }

            public string LoginProvider { get; set; }
            public AuthenticationProperties Properties { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.GetOwinContext().Authentication.Challenge(Properties, LoginProvider);
            }
        }

        #endregion
    }
}