using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace TrafficLoadSimulation
{
    internal class Requests
    {
        private HttpMethod MethodHttp { get; set; }
        private string Url { get; set; }

        private string Bearer { get; set; }

        public string? JsonBody { get; set; }

        HttpResponseMessage response;
        public Requests(string url, string methodHttp, string bearer, string? jsonBody)
        {
            Url = url;
            MethodHttp = new HttpMethod(methodHttp);
            Bearer = bearer;
            JsonBody = jsonBody;
        }

        public async Task<string> start()
        {
            var jsonContent = new StringContent(content: JsonBody, Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            var requestMessage = new HttpRequestMessage{
                Method = MethodHttp,
                RequestUri = new Uri(Url),
                Content = jsonContent
            };
            
            if (!string.IsNullOrEmpty(Bearer))
            {
                requestMessage.Headers.Add("Authorization", $"Bearer {Bearer}");
            }
            try
            {

                response = await httpClient.SendAsync(requestMessage);

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                var jsonResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var deserializedObject = JsonSerializer.Deserialize<object>(jsonResponse);
                    string formattedJson = JsonSerializer.Serialize(deserializedObject, options);
                    return formattedJson;

                }catch (Exception ex)
                {
                    return jsonResponse;
                }
            } catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
