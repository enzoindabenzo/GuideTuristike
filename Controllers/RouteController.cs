using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace DK1.Controllers
{
    public class RouteRequestModel
    {
        public double OriginLatitude { get; set; }
        public double OriginLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
    }

    public class RouteController : Controller
    {
        private const string ApiKey = "5b3ce3597851110001cf6248ea7e18438103425abc6af91e582ca4dd";
        private const string BaseUrl = "https://api.openrouteservice.org/v2/directions/driving-car";

        [HttpPost]
        public async Task<ActionResult> GetRoute()
        {
            try
            {
                // Read and parse the request body
                var input = await new System.IO.StreamReader(Request.InputStream).ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<RouteRequestModel>(input);

                if (model == null)
                    return new HttpStatusCodeResult(400, "Invalid JSON body");

                // Create request body with coordinates
                var body = new
                {
                    coordinates = new double[][]
                    {
                        new double[] { model.OriginLongitude, model.OriginLatitude },
                        new double[] { model.DestinationLongitude, model.DestinationLatitude }
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(body);

                using (var httpClient = new HttpClient())
                {
                    // Set up the request with proper authentication
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
                    requestMessage.Headers.Add("Authorization", $"Bearer {ApiKey}");
                    requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    // Send the request
                    var response = await httpClient.SendAsync(requestMessage);
                    var content = await response.Content.ReadAsStringAsync();

                    // Log the response for debugging
                    System.Diagnostics.Debug.WriteLine($"API Response: {content}");

                    if (!response.IsSuccessStatusCode)
                    {
                        // Return the error details for easier debugging
                        return new HttpStatusCodeResult((int)response.StatusCode,
                            $"API Error: {response.StatusCode}, Details: {content}");
                    }

                    return Content(content, "application/json");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Exception in GetRoute: {ex}");
                return new HttpStatusCodeResult(500, $"Internal error: {ex.Message}");
            }
        }
    }
}