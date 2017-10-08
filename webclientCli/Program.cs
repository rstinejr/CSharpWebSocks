using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace waltonstine.demo.csharp.websockets.webclientcli
{
    class Program
    {
        private static async Task<HttpResponseMessage> PostString(string serverURL, string dataString)
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

        private static async Task<string> RetrieveUploadID(string url, string path)
        {
            Task<HttpResponseMessage> task = PostString(url, Path.GetFileName(path));

            task.Wait();

            HttpResponseMessage resp = task.Result;

            Console.WriteLine($"Got respons, status {resp.StatusCode}");
            if (!resp.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Request error: {resp.ToString()}");
                return null;
            }

            return await resp.Content.ReadAsStringAsync();
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

            if ( ! File.Exists(path))
            {
                Console.Error.WriteLine($"File '{path}' not found.");
                return;
            }

            Console.WriteLine($"Send request to {url} for upload ID.");

            Task<string> replyContent = RetrieveUploadID(url, path);
            if (replyContent != null)
            {
                replyContent.Wait();
                Console.WriteLine($"Upload ID is {replyContent.Result}");
            }

            return;
        }
    }
}
