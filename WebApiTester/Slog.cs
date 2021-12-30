using System.Linq;
using System.Net.Http;
using Serilog;

namespace WebApiTester
{
    public class Slog
    {
        public static void ApiCall(HttpClient client, string url, string method = "GET", string body=null)
        {
            var apicall = "";//"$"-H: api_key=" + client.DefaultRequestHeaders.GetValues("api_key").First() + "\r\n";
            
            apicall += $"{method} {client.BaseAddress}{url}";

            if (!string.IsNullOrWhiteSpace(body))
            {
                apicall += $"\r\nbody:\r\n{body}";
            }

            Log.Debug(apicall);
        }
    }
}
