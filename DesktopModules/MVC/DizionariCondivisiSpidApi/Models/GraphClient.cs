using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Models
{
    public class GraphClient
    {
        private const string aadInstance = "https://login.microsoftonline.com/";
        private const string msGraphResourceId = "https://graph.microsoft.com/";
        private const string msGraphEndpoint = "https://graph.microsoft.com/";
        private const string msGraphVersion = "1.0";

        private enum GraphApiVersion
        {
            beta,
            latest
        }

        private ClientCredential Credential { get; set; }
        private AuthenticationContext AuthContext { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string Tenant { get; set; }

        public GraphClient(string clientId, string clientSecret, string tenant)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Tenant = tenant;
            AuthContext = new AuthenticationContext(aadInstance + tenant);
            Credential = new ClientCredential(clientId, clientSecret);
        }

        public string GetUser(string objectId)
        {
            return SendGraphRequest("/users/" + objectId, apiVersion: GraphApiVersion.beta);
        }

        private string SendGraphRequest(string api, string query = null, string body = null, GraphApiVersion apiVersion = GraphApiVersion.latest, HttpMethod httpMethod = null)
        {
            AuthenticationResult result = AuthContext.AcquireTokenAsync(msGraphResourceId, Credential).Result;

            using (HttpClient http = new HttpClient())
            {
                string url = msGraphEndpoint + (apiVersion == GraphApiVersion.latest ? msGraphVersion : "beta") + api;
                if (!string.IsNullOrEmpty(query))
                {
                    url += "&" + query;
                }

                HttpRequestMessage request = new HttpRequestMessage(httpMethod ?? HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                if (!string.IsNullOrEmpty(body))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                HttpResponseMessage response = http.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                {
                    string error = response.Content.ReadAsStringAsync().Result;
                }
                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}