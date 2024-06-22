using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace TrafficLoadSimulation
{
    internal class RequestUnderLoad
    {
        public string MethodHttp { get; set; }
        public string Url { get; set; }

        public int VirtualUser { get; set; }

        public int TestDuration { get; set; }

        public long countRequest => responses.Values.Sum() + countRequestError;

        public long countRequestError = 0;


        public long errorRate => ((countRequestError + responses.FirstOrDefault(x => x.Key == HttpStatusCode.ServiceUnavailable).Value) * 100)/(countRequest==0?1:countRequest);
        //responses.FirstOrDefault(x=> x.Key == HttpStatusCode.ServiceUnavailable).Value
        public float avgCountRequest;

        public float avgTimeRequest;

        public ConcurrentBag<long> sumTime = new ConcurrentBag<long>();
        
        public ConcurrentDictionary<HttpStatusCode, long> responses;

        public DateTime end;
        internal async Task<Results> startTest()
        {
            /*if(testConnection() == false)
            {
                return new Results { errorMessage = "Отсутсвует соединение с сервером" };
            }*/

            responses = new ConcurrentDictionary<HttpStatusCode, long>();

            end = DateTime.Now.AddMinutes(TestDuration);
           
            var task = Enumerable.Range(0, VirtualUser).Select(async _ => await Task.Run(GetAsync)).ToArray();
            await Task.WhenAll(task);


            var results = new Results {countRequest = countRequest,avgCountRequest = ((float)countRequest/(float)TestDuration)/60.0f,
                avgTimeResponse = sumTime.Sum()/countRequest, minTimeResponse = sumTime.Min() , maxTimeResponse = sumTime.Max(), errorRate = errorRate, responses = responses, errors = countRequestError};
            return results;
           
        }

        /*internal bool testConnection() {
            try
            {
                var client = new TcpClient();
                client.Connect("localhost", 7043);
                if (client.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

        }*/

        async Task GetAsync()
        {
            for (;  end >= DateTime.Now;)
            {
                var time = Stopwatch.StartNew();
                var request = new HttpClient();
                try
                {
                    var response = await request.GetAsync(Url);
                    responses.AddOrUpdate(response.StatusCode, 1, (key, oldValue) => oldValue + 1);

                    
                } catch (Exception ex)
                {
                    countRequestError++;
                }
                finally
                {
                    sumTime.Add(time.ElapsedMilliseconds);
                    time.Stop();
                }
            }

            //response.EnsureSuccessStatusCode();

            //var jsonResponse = await response.Content.ReadAsStringAsync();
            //return jsonResponse;
        }

        async Task<string> PostAsync(HttpClient httpClient,string url ,string body)
        {
            using StringContent jsonContent = new(body,Encoding.UTF8,"application/json");

            using HttpResponseMessage response = await httpClient.PostAsync(
                url,
                jsonContent);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        private static HttpClient sharedClient = new();
    }
}
