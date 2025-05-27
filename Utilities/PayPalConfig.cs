using PayPal.Api;
using System.Collections.Generic;

namespace DK1.Utilities
{
    public static class PayPalConfig
    {
        public static Dictionary<string, string> GetConfig()
        {
            return new Dictionary<string, string>
            {
                { "mode", System.Configuration.ConfigurationManager.AppSettings["PayPalMode"] }
            };
        }

        private static string GetAccessToken()
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["PayPalClientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["PayPalClientSecret"];
            return new OAuthTokenCredential(clientId, clientSecret, GetConfig()).GetAccessToken();
        }

        public static APIContext GetAPIContext()
        {
            var apiContext = new APIContext(GetAccessToken())
            {
                Config = GetConfig()
            };
            return apiContext;
        }
    }
}