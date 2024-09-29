using Common.Lib;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DisposeHub.Con
{
    /// <summary>
    /// 管理链接，包含放松信息
    /// </summary>
    public class SocketManager
    {
        private static string _id;
        private static WebSocket _webSocket;

        public static void Init(string id, WebSocket webSocket)
        {
            _id = id;
            _webSocket = webSocket;
        }

        public static void SendTaskState(string taskName, TaskState state, string log)
        {
            var logModel = new TaskLogModel
            {
                TaskName = taskName,
                TaskState = state,
                Log = log
            };

            var msgModel = new WsDataModel
            {
                ID = _id,
                Action = WsAction.发送任务状态,
                DataJson = JsonSerializer.Serialize(logModel)
            };

            var sendInfoStr = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msgModel));
            _webSocket.SendAsync(sendInfoStr, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }
    }
}
