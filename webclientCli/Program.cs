using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace waltonstine.demo.csharp.websockets.webclientcli
{
    class Program
    {
        public static async Task<HttpResponseMessage> PostString(string serverURL, string dataString)
        {
            StringContent strContent = new StringContent(dataString);

            HttpResponseMessage response = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    response = await client.PostAsync(serverURL, strContent);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"PostString: {ex.Message}");
                }
            }

            return response;
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: dotnet run <server URL> <src file path>");
                return;
            }

            string url  = args[0];
            string path = args[1];

            Console.WriteLine($"Send request to {url}");
            Task<HttpResponseMessage> task = PostString(url, path);

            task.Wait();

            HttpResponseMessage resp = task.Result;

            Console.WriteLine($"Got respons, status {resp.StatusCode}");
            if (resp.IsSuccessStatusCode)
            {
                Task<string> contentTask = resp.Content.ReadAsStringAsync();
                contentTask.Wait();

                Console.WriteLine($"Return from query content '{contentTask.Result}'");
            }
        }
    }
}
