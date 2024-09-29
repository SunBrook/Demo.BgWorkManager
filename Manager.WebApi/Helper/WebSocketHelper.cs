using Common.Lib;
using Manager.WebApi.Model;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FileInfo = Common.Lib.FileInfo;

namespace Manager.WebApi.Helper
{
    public class WebSocketHelper
    {
        private static WebSocketHelper helper;
        public static WebSocketHelper Instance => GetInstance();
        private static WebSocketHelper GetInstance()
        {
            if (helper == null)
            {
                helper = new WebSocketHelper();
            }
            return helper;
        }


        public async Task WebSocketReceive(WebSocket webSocket)
        {
            var id = Guid.NewGuid().ToString("N");
            var buffer = ArrayPool<byte>.Shared.Rent(1024);

            var ms = new MemoryStream();

            try
            {
                WebSocketReceiveResult result;
                while (webSocket.State == WebSocketState.Open)
                {
                    List<byte> totalBytes = new List<byte>();

                    do
                    {
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely, result.CloseStatusDescription);
                        }

                        // 读取数据，然后执行操作
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var segment = new ArraySegment<byte>(buffer, 0, result.Count);
                            totalBytes.AddRange(segment);
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            await ms.WriteAsync(buffer, 0, result.Count);
                        }

                    } while (!result.EndOfMessage);


                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var totalStr = Encoding.UTF8.GetString(totalBytes.ToArray().AsSpan(0, totalBytes.Count));
                        var sendStr = Encoding.UTF8.GetBytes(totalStr);
                        //await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);
                        DisposeWsMsg(webSocket, totalStr);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // 二进制文件
                        var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        var filePath = Path.Combine(dirPath, $"{id}.rar");
                        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);

                        ms.Seek(0, SeekOrigin.Begin);
                        await ms.CopyToAsync(fileStream);

                        //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                        fileStream.Close();
                        fileStream.Dispose();
                        await Console.Out.WriteLineAsync("文件接收完成");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);

                ms.Close();
                ms.Dispose();
            }
        }

        private void DisposeWsMsg(WebSocket webSocket, string text)
        {
            var data = JsonSerializer.Deserialize<WsDataModel>(text);
            if (data != null)
            {
                SocketManager.Instance.AddSocket(webSocket, data.ID, data.ConId, data.ConName);
                switch (data.Action)
                {
                    case WsAction.执行端基础信息:
                        Console.WriteLine("基础信息：{0}", data.DataJson);
                        break;
                    case WsAction.请求任务列表:
                        Console.WriteLine("请求任务列表：{0}", data.DataJson);
                        break;
                    case WsAction.发送任务状态:
                        var statusInfo = JsonSerializer.Deserialize<TaskLogModel>(data.DataJson);
                        if (statusInfo != null)
                        {
                            Console.WriteLine("发送任务状态, 任务名：{0}，状态：{1}, 说明：{2}", statusInfo.TaskName, statusInfo.TaskState, statusInfo.Log);
                        }
                        break;
                }
            }
        }




        /// <summary>
        /// 发送 WebSocket 文本消息
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="text"></param>
        public async Task SendTextAsync(WebSocket webSocket, string text)
        {
            var sendStr = Encoding.UTF8.GetBytes(text);
            await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);
        }


        /// <summary>
        /// 发送 WebSocket 文本 + 文件base64 的 JSON 文件
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="disposeFile"></param>
        /// <returns></returns>
        public async Task SendFileJsonAsync(WebSocket webSocket, ToDisposeFile disposeFile)
        {
            List<byte> totalBytes = new List<byte>();

            using (var fileStream = new FileStream(disposeFile.FilePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[1024];
                var bytesRead = 0;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                {
                    var segment = new ArraySegment<byte>(buffer, 0, bytesRead);
                    totalBytes.AddRange(segment);
                }
            }

            var fileInfo = new FileInfo
            {
                FileName = disposeFile.FileName,
                Base64 = Convert.ToBase64String(totalBytes.ToArray())
            };

            var sendMsg = new WsDataModel
            {
                Action = WsAction.请求任务列表,
                DataJson = JsonSerializer.Serialize(fileInfo),
            };

            //if (webSocket.State != WebSocketState.Open)
            //{
            //    webSocket = await webSocketClient.ConnectWithRetry();
            //}

            var msg2 = $"{JsonSerializer.Serialize(sendMsg)}";
            var sendStr = Encoding.UTF8.GetBytes(msg2);
            await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);
        }



        public class WsUserModel
        {
            public string UserId { get; set; }
            public WebSocket WebSocket { get; set; }
        }

    }
}
