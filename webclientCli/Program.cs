using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
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

        private static async Task<string> RequestUploadToken(string url, string path)
        {
            Task<HttpResponseMessage> task = PostString(url, Path.GetFileName(path));

            task.Wait();

            HttpResponseMessage resp = task.Result;

            if (!resp.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Request error: {resp.ToString()}");
                return null;
            }


            return await resp.Content.ReadAsStringAsync();
        }

        /*
         * SendBytes: Write bytes to a web socket
         */
        private static void SendBytes(Uri controllerUri, byte[] data)
        {
            ClientWebSocket sock = new ClientWebSocket();
            Task sockTask = sock.ConnectAsync(controllerUri, CancellationToken.None);
            if (!sockTask.Wait(5000))
            {
                throw new Exception($"Timeout attempting to connect to {controllerUri}");
            }

            sock.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
            /* Ugly hack to get around Web Socket bug on Mint 14.4: spurious error on server side about dropping
             * web socket connection without performing close protocol.
             */
            Thread.Sleep(2000);


            Console.WriteLine($"{data.Length} bytes written to {controllerUri}, now close the connection.");
            try
            {
                sock.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                        "upload done.", CancellationToken.None).Wait();
            }
            catch (Exception)
            {
                // It seems that the connection is closed. :)
            }
        }

        static string SRVR_PORT = "54321";  // At the network level, this is an unsigned short

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

            Task<string> replyContent = RequestUploadToken($"http://{srvr}:{SRVR_PORT}/upload", path);
            if (replyContent != null)
            {
                replyContent.Wait();
                Console.WriteLine($"Upload token is {replyContent.Result}");
                string uploadURL = $"ws://{srvr}:{SRVR_PORT}/upload/{replyContent.Result}";

                /* TODO: it would be more robust to read and send the content in reasonable segments. That way, even
                 * really large files could be uplaoded.
                 */
                byte[] fileBytes = File.ReadAllBytes(path);

                SendBytes(new Uri(uploadURL), fileBytes);
            }

            return;
        }
    }
}
