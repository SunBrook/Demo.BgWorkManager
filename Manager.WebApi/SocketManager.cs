using System.Net.WebSockets;

namespace Manager.WebApi
{
    /// <summary>
    /// 统一管理 Socket
    /// </summary>
    public class SocketManager
    {
        private static SocketManager manager;
        public static SocketManager Instance => GetInstance();

        private static SocketManager GetInstance()
        {
            if (manager == null)
            {
                manager = new SocketManager();
            }
            return manager;
        }

        private static List<SocketModel> SocketList = new List<SocketModel>();

        /// <summary>
        /// 根据 ID 获取Socket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WebSocket? GetSocketByID(string id)
        {
            var socket = SocketList.FirstOrDefault(t => t.ID == id)?.WebSocket ?? null;
            return socket;
        }

        /// <summary>
        /// 根据客户端ID获取Socket
        /// </summary>
        /// <param name="conId"></param>
        /// <returns></returns>
        public WebSocket? GetSocketByConId(string conId)
        {
            var socket = SocketList.FirstOrDefault(t => t.ConId == conId)?.WebSocket ?? null;
            return socket;
        }

        /// <summary>
        /// 新增WebSocket, 并返回ID
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public void AddSocket(WebSocket webSocket, string id, string conId, string conName)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            ClearDeathSockets();

            var socket = SocketList.FirstOrDefault(t => t.ID == id);
            if (socket == null)
            {
                var model = new SocketModel
                {
                    ID = id,
                    WebSocket = webSocket,
                    ConId = conId,
                    ConName = conName
                };
                SocketList.Add(model);
            }
        }

        /// <summary>
        /// 根据ID删除 WebSocket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveSocket(string id)
        {
            var removeItem = SocketList.FirstOrDefault(t => t.ID == id);
            if (removeItem == null)
            {
                return false;
            }

            return SocketList.Remove(removeItem);
        }

        /// <summary>
        /// 获取 Socket 列表
        /// </summary>
        /// <returns></returns>
        public List<SocketModel> GetSocketList()
        {
            ClearDeathSockets();
            return SocketList;
        }

        /// <summary>
        /// 清除链接状态异常的链接
        /// </summary>
        public void ClearDeathSockets()
        {
            var deathItems = SocketList
                .Where(t => t.WebSocket.State != WebSocketState.Open)
                .ToList();

            SocketList = SocketList.Except(deathItems).ToList();
        }
    }

    public class SocketModel
    {
        public string ID { get; set; }
        public string ConId { get; set; }
        public string ConName { get; set; }
        public WebSocket WebSocket { get; set; }
    }
}
