using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DisposeHub.Con
{
    public class WebSocketClient
    {
        private readonly string _url;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ManualResetEventSlim _reconnectResetEvent = new ManualResetEventSlim(false);

        public WebSocketClient(string url)
        {
            _url = url;
            ConnectAsync().ContinueWith(_ => ConnectWithRetry(), TaskContinuationOptions.OnlyOnFaulted);
        }

        public async Task<ClientWebSocket> ConnectAsync()
        {
            try
            {
                var cancellationToken = _cancellationTokenSource.Token;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        _webSocket = new ClientWebSocket();
                        await _webSocket.ConnectAsync(new Uri(_url), cancellationToken);
                        // 连接成功，执行你的逻辑
                        _reconnectResetEvent.Reset();
                        break;
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        // 连接失败，等待重连
                        Console.WriteLine($"Connect failed: {ex.Message}, retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException) { }
            return _webSocket;
        }

        public async Task<ClientWebSocket> ConnectWithRetry()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                //_reconnectResetEvent.Wait(_cancellationTokenSource.Token);
                return await ConnectAsync();
            }
            return null;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _reconnectResetEvent.Set();
            _webSocket?.Dispose();
        }

    }
}
