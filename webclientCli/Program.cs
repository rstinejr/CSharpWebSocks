using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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

        /*
         * SendBytes: Use WebSockets to send data to Controller.
         *            Executed from a worker task, so it's OK for this to be synchronous.
         */
        private static void SendBytes(Uri controllerUri, byte[] data)
        {
            Console.WriteLine($"Send {data.Length} bytes to {controllerUri}");
            ClientWebSocket sock = new ClientWebSocket();
            Task sockTask = sock.ConnectAsync(controllerUri, CancellationToken.None);
            if (!sockTask.Wait(5000))
            {
                throw new Exception($"Timeout attempting to connect to {controllerUri}");
            }

            sock.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
            Console.WriteLine($"{data.Length} bytes uploaded to {controllerUri}");
        }

        static string SRVR_PORT = "54321";

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: dotnet run <server IP or host> <src file path>");
                return;
            }

            string srvr = args[0];
            string path = args[1];

            if ( ! File.Exists(path))
            {
                Console.Error.WriteLine($"File '{path}' not found.");
                return;
            }

            Console.WriteLine($"Send request to {srvr} for upload ID.");

            Task<string> replyContent = RetrieveUploadID($"http://{srvr}:{SRVR_PORT}/upload", path);
            if (replyContent != null)
            {
                replyContent.Wait();
                Console.WriteLine($"Upload ID is {replyContent.Result}");
                string uploadURL = $"ws://{srvr}:{SRVR_PORT}/upload/{replyContent.Result}";

                /* TODO: it would be more robust to read and send the content in reasonable segments. That way, even
                 * really large files could be uplaoded.
                 */
                byte[] fileBytes = File.ReadAllBytes(path);

                Console.WriteLine($"upload file bytes to {uploadURL}");

                SendBytes(new Uri(uploadURL), fileBytes);
            }

            return;
        }
    }
}
