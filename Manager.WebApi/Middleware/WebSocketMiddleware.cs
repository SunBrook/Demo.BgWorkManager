using Common.Lib;
using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Manager.WebApi.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    // 如果是 webSocket 请求
                    try
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        if (webSocket != null)
                        {
                            var wsclient = new WebSocketClient
                            {
                                Uid = "",
                                WebSocket = webSocket
                            };
                            await Handle(wsclient);
                        }
                    }
                    catch (Exception ex)
                    {
                        await context.Response.WriteAsync(ex.Message);
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task Handle(WebSocketClient wsclient)
        {
            WebSocketClientCollection.AddClient(wsclient);

            var byteList = new List<byte>();
            var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);

            while (wsclient.WebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                var segment = new ArraySegment<byte>(buffer);
                do
                {
                    result = await wsclient.WebSocket.ReceiveAsync(segment, CancellationToken.None);
                    var partBytes = segment.Take(result.Count).ToList();
                    byteList.AddRange(partBytes);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await wsclient.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msgStr = Encoding.UTF8.GetString(byteList.ToArray());
                    Console.WriteLine(msgStr);

                    var useMsg = JsonSerializer.Deserialize<UseMsg>(msgStr);
                    if (useMsg != null)
                    {
                        wsclient.Uid = useMsg.Uid;
                        await HandlessMessageAsync(useMsg);
                    }
                    else
                    {
                        // 消息格式错误
                        throw new Exception($"webSocket 消息格式错误：{msgStr}");
                    }
                }
            }

            // 断开链接
            WebSocketClientCollection.RemoveClient(wsclient.Uid);
            
        }

        private async Task HandlessMessageAsync(UseMsg useMsg)
        {
            switch (useMsg.Action)
            {
                case MsgAction.文字:
                    break;
                case MsgAction.文件:
                    break;
                case MsgAction.加入聊天室:
                    // 写入到用户的房间表中，一个房间一行数据
                    var client = WebSocketClientCollection.GetClient(useMsg.Uid);
                    await JoinToRoom(client, useMsg);
                    break;
                case MsgAction.单聊:
                    var receClient = WebSocketClientCollection.GetClient(useMsg.ToUid.ToString());
                    if (receClient != null)
                    {
                        string msgStr = useMsg.MsgInfo;
                        var buffer = Encoding.UTF8.GetBytes(msgStr);
                        await receClient.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    break;
                case MsgAction.群聊:
                    var receClients = WebSocketClientCollection.GetALLClient().Where(t => t.Uid != useMsg.Uid);
                    if (receClients != null)
                    {
                        string msgStr = useMsg.MsgInfo;
                        var buffer = Encoding.UTF8.GetBytes(msgStr);
                        foreach (var item in receClients)
                        {
                            await item.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    break;
                case MsgAction.离开:
                    string get_uid = useMsg.Uid.ToString();
                    var receClient2 = WebSocketClientCollection.GetClient(get_uid);
                    if (receClient2 != null)
                    {
                        WebSocketClientCollection.RemoveClient(receClient2.Uid);
                        await receClient2.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    break;
            }
        }

        private async Task<bool> JoinToRoom(WebSocketClient? client, UseMsg useMsg)
        {
            string uid = useMsg.Uid;
            if (client != null)
            {
                client.Uid = uid;
            }
            if (WebSocketClientCollection.GetClient(uid) != null)
            {
                WebSocketClientCollection.AddClient(client);
            }
            string msgStr = useMsg.MsgInfo;

            Console.WriteLine(msgStr);

            byte[] by = Encoding.UTF8.GetBytes(msgStr);
            var buffer = new ArraySegment<byte>(by, 0, by.Length);

            // 通知所有人(排除自己)
            var clients = WebSocketClientCollection.GetALLClient().Where(t => t.Uid != uid);
            foreach (var item in clients)
            {
                await item.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            return true;
        }
    }

    public class WebSocketClient
    {
        public string? Uid { get; set; }
        public WebSocket? WebSocket { get; set; }
    }

    public class WebSocketClientCollection
    {
        private static List<WebSocketClient> _client = new List<WebSocketClient>();

        public static void AddClient(WebSocketClient client)
        {
            var clientNow = _client.FirstOrDefault(t => t.Uid == client.Uid);
            if (clientNow != null)
            {
                _client.Remove(clientNow);
            }
            _client.Add(client);
        }

        public static void RemoveClient(string clientId)
        {
            var clientNow = _client.FirstOrDefault(t => t.Uid == clientId);
            if (clientNow != null)
            {
                _client.Remove(clientNow);
            }
        }

        public static WebSocketClient? GetClient(string clientId)
        {
            return _client.FirstOrDefault(t => t.Uid == clientId);
        }

        public static List<WebSocketClient> GetALLClient()
        {
            return _client;
        }
    }

}
