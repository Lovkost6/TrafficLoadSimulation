using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrafficLoadSimulation
{
    internal class Results
    {
        public long countRequest { get; set; }
        public float avgCountRequest { get; set; }
        public float avgTimeResponse { get; set; }
        public long minTimeResponse { get; set; }
        public long maxTimeResponse { get; set; }
        public string errorMessage { get; set; }
        public long errorRate { get; set; }
        public long errors { get; set; }
        public ConcurrentDictionary<HttpStatusCode, long> responses { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(errorMessage) == false)
            {
                return errorMessage;
            }
            return $"Total requests sent: {countRequest}" + Environment.NewLine +
                $"Requests/second: {avgCountRequest}" + Environment.NewLine +
                $"Avg. response time: {avgTimeResponse}" + Environment.NewLine +
                $"Min respone time: {minTimeResponse}" + Environment.NewLine +
                $"Max response time: {maxTimeResponse}" + Environment.NewLine +
                $"Error rate: {errorRate}%" + Environment.NewLine+ 
                "Status code:" + Environment.NewLine +
                String.Join(Environment.NewLine, responses.Select(res => $"{res.Key}:{res.Value}")) + Environment.NewLine + 
                $"Errors: {errors}";
        }

    }
}
