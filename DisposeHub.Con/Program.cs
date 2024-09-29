using Common.Lib;
using FreeScheduler;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static FreeSql.Internal.GlobalFilter;
using static System.Net.Mime.MediaTypeNames;
using FileInfo = Common.Lib.FileInfo;

namespace DisposeHub.Con
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //// 配置客户端ID 和名称
            //ConfigInfo.Instance.SetConId("12345678");
            //ConfigInfo.Instance.SetConName("客户端一号机");

            //return;


            // webSocket
            await WsMethod();
            //await Task.Delay(0);

            //// 任务热重载
            //await TaskDLLLoad();

            //// 调度任务
            //ScheduleTasks();

            #region dll热重载 + 调度任务

            //// dll热重载 + 调度任务
            //var list = new List<LoadDLL>();
            //Console.Out.WriteLine("加载DLL");

            ////AddFileDll(list, "DemoTask.dll");
            //AddFileDll(list, "TestTask.dll");

            //// 创建任务并执行
            //foreach (var item in list)
            //{
            //    item.Schedule("*/2 * * * * *");
            //}

            //Task.Delay(10000).Wait();

            //// 暂停任务
            //foreach (var item in list)
            //{
            //    item.PauseTask();
            //}

            //Task.Delay(10000).Wait();

            //// 恢复任务
            //foreach (var item in list)
            //{
            //    item.ResumeTask();
            //}

            //Task.Delay(10000).Wait();

            //// 删除任务
            //foreach (var item in list)
            //{
            //    item.RemoveTask();
            //}

            Console.ReadLine();

            #endregion
        }

        private static async Task TaskDLLLoad()
        {
            var list = new List<LoadDLL>();
            Console.Out.WriteLine("加载DLL");

            AddFileDll(list, "DemoTask.dll");
            AddFileDll(list, "TestTask.dll");

            foreach (var item in list)
            {
                item.StartTask();
            }
            await Console.Out.WriteLineAsync("任务开启");
            SpinWait.SpinUntil(() => false, 5000);
            foreach (var item in list)
            {
                var s = item.UnLoad();
                SpinWait.SpinUntil(() => false, 1000);
                await Console.Out.WriteLineAsync($"任务卸载：{s}");
            }
        }

        private static void AddFileDll(List<LoadDLL> list, string dllFileName)
        {
            var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskDLL");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            // 检查去重
            var checkFile = list.FirstOrDefault(t => t.FileName == dllFileName);
            if (checkFile != null)
            {
                return;
            }

            var dllPath = Path.Combine(dirPath, dllFileName);
            var loadDll = new LoadDLL();
            loadDll.FileName = dllFileName;
            loadDll.LoadFile(dllPath);
            list.Add(loadDll);
        }

        private static async Task WsMethod()
        {
            //var webSocketClient = await CreateAsync("ws://localhost:5025/ws");
            var webSocketClient = new WebSocketClient("wss://localhost:7122/ws");
            //var webSocketClient = new WebSocketClient("wss://localhost:5001/ws");
            var webSocket = await webSocketClient.ConnectAsync();

            if (webSocket != null)
            {
                Console.WriteLine("服务开始执行!");
                _ = Task.Run(async () =>
                {
                    webSocket = await ReceiveMsg(webSocketClient, webSocket);
                });

                // 发送基础信息
                var userId = Guid.NewGuid().ToString();
                var sendInfo = new WsDataModel
                {
                    ID = userId,
                    ConId = ConfigInfo.Instance.GetConId(),
                    ConName = ConfigInfo.Instance.GetConName(),
                    Action = WsAction.执行端基础信息,
                    DataJson = $"用户ID: {userId}",
                };

                var sendInfoStr = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendInfo));
                await webSocket.SendAsync(sendInfoStr, WebSocketMessageType.Text, true, CancellationToken.None);

                // 存储管理端链接信息
                SocketManager.Init(userId, webSocket);

                #region 传输文本

                // 通信传输文本
                string text = "";
                Console.WriteLine("开始输入：");
                while (text != "exit")
                {
                    text = Console.ReadLine();

                    var sendMsg = new WsDataModel
                    {
                        Action = WsAction.请求任务列表,
                        DataJson = text,
                    };

                    if (webSocket.State != WebSocketState.Open)
                    {
                        webSocket = await webSocketClient.ConnectWithRetry();
                    }

                    var sendStr = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendMsg));
                    await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);
                }

                #endregion

                #region 传输文件

                //// 通讯传输文件
                //var filePath = "C:\\Users\\Administrator\\Desktop\\SendFBWebHook.rar";
                //using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                //{
                //    var buffer = new byte[1024];
                //    var bytesRead = 0;

                //    List<byte> totalBytes = new List<byte>();

                //    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                //    {
                //        var segment = new ArraySegment<byte>(buffer, 0, bytesRead);
                //        totalBytes.AddRange(segment);
                //    }

                //    await webSocket.SendAsync(new ArraySegment<byte>(totalBytes.ToArray(), 0, totalBytes.Count), WebSocketMessageType.Binary, true, CancellationToken.None);
                //}

                #endregion

                #region 传输文本，文件用byte数组

                //List<byte> totalBytes = new List<byte>();

                //var filePath = "C:\\Users\\Administrator\\Desktop\\JsonString.log";
                //using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                //{
                //    var buffer = new byte[1024];
                //    var bytesRead = 0;

                //    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                //    {
                //        var segment = new ArraySegment<byte>(buffer, 0, bytesRead);
                //        totalBytes.AddRange(segment);
                //    }
                //}

                //var fileInfo = new FileInfo
                //{
                //    FileName = "JsonString.log",
                //    Base64 = Convert.ToBase64String(totalBytes.ToArray())
                //};

                //var sendMsg = new WsDataModel
                //{
                //    Action = WsAction.请求任务列表,
                //    DataJson = JsonSerializer.Serialize(fileInfo),
                //};

                //if (webSocket.State != WebSocketState.Open)
                //{
                //    webSocket = await webSocketClient.ConnectWithRetry();
                //}

                //var msg2 = $"{JsonSerializer.Serialize(sendMsg)}\0";
                //var sendStr = Encoding.UTF8.GetBytes(msg2);
                //await webSocket.SendAsync(sendStr, WebSocketMessageType.Text, true, CancellationToken.None);

                #endregion


                //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("服务连接失败!");
            }
            Console.WriteLine("服务执行完毕!");
            Console.ReadLine();
        }

        private static async Task<ClientWebSocket> ReceiveMsg(WebSocketClient webSocketClient, ClientWebSocket webSocket)
        {
            var id = Guid.NewGuid().ToString("N");
            var buffer = ArrayPool<byte>.Shared.Rent(1024);

            var ms = new MemoryStream();

            try
            {
                do
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result;
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

                    // 重连
                    webSocket = await webSocketClient.ConnectWithRetry();
                    await Console.Out.WriteLineAsync("重连成功");
                } while (true);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void DisposeWsMsg(ClientWebSocket webSocket, string text)
        {
            var data = JsonSerializer.Deserialize<WsDataModel>(text);
            if (data != null)
            {
                switch (data.Action)
                {
                    case WsAction.执行端基础信息:
                        Console.WriteLine("基础信息：{0}", data.DataJson);
                        break;
                    case WsAction.请求任务列表:
                        Console.WriteLine("请求任务列表：{0}", data.DataJson);
                        DisposeDLL(data.DataJson);
                        break;
                    case WsAction.加载并执行任务:
                        Console.WriteLine("加载并执行任务：{0}", data.DataJson);
                        LoadRunTask(data.DataJson);
                        break;
                    case WsAction.任务暂停:
                        Console.WriteLine("任务暂停：{0}", data.DataJson);
                        PauseTask(data.DataJson);
                        break;
                    case WsAction.任务恢复:
                        Console.WriteLine("任务恢复：{0}", data.DataJson);
                        ResumeTask(data.DataJson);
                        break;
                    case WsAction.任务删除:
                        Console.WriteLine("任务删除：{0}", data.DataJson);
                        RemoveTask(data.DataJson);
                        break;
                    case WsAction.消费者任务调整:
                        Console.WriteLine("消费者任务调整：{0}", data.DataJson);
                        AdjustConsumeTask(data.DataJson);
                        break;
                }
            }
        }

        

        private static void DisposeDLL(string dataJson)
        {
            var fileInfo = JsonSerializer.Deserialize<FileInfo>(dataJson);
            if (fileInfo != null)
            {
                var dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskDLL");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var filePath = Path.Combine(dirPath, fileInfo.FileName);

                // base64字符串转文件
                byte[] byteArray = Convert.FromBase64String(fileInfo.Base64);
                File.WriteAllBytes(filePath, byteArray);
            }
        }

        private static List<LoadDLL> list = new List<LoadDLL>();

        private static void LoadRunTask(string dataJson)
        {
            // 读取任务名和执行频率
            var taskInfo = JsonSerializer.Deserialize<LoadRunTaskModel>(dataJson);
            if (taskInfo == null)
            {
                return;
            }

            // 加载DLL
            Console.Out.WriteLine($"加载DLL: {taskInfo.TaskName}");
            AddFileDll(list, taskInfo.TaskName);
            var dllTask = list.FirstOrDefault(t => t.FileName == taskInfo.TaskName);
            if (dllTask == null)
            {
                Console.WriteLine($"任务加载失败：{taskInfo.TaskName}");
                return;
            }

            // 判断任务类型，根据任务类型来规划执行
            switch (dllTask._task.TaskType)
            {
                case TaskType.定时循环任务:
                    Console.Out.WriteLine($"添加调度并运行: {taskInfo.TaskName}");
                    dllTask.Schedule(taskInfo.CornExp);
                    break;
                case TaskType.临时单次任务:
                    Console.Out.WriteLine($"临时单次任务运行：{taskInfo.TaskName}");
                    dllTask.RunTempTask();
                    break;
            }
        }

        private static void PauseTask(string taskName)
        {
            // 寻找任务
            var dllTask = list.FirstOrDefault(t => t.FileName == taskName);
            if (dllTask == null)
            {
                Console.WriteLine($"任务暂停失败：{taskName}");
                return;
            }

            dllTask.PauseTask();
        }

        private static void ResumeTask(string taskName)
        {
            // 寻找任务
            var dllTask = list.FirstOrDefault(t => t.FileName == taskName);
            if (dllTask == null)
            {
                Console.WriteLine($"任务恢复失败：{taskName}");
                return;
            }

            dllTask.ResumeTask();
        }

        private static void RemoveTask(string taskName)
        {
            // 寻找任务
            var dllTask = list.FirstOrDefault(t => t.FileName == taskName);
            if (dllTask == null)
            {
                Console.WriteLine($"任务删除失败：{taskName}");
                return;
            }

            dllTask.RemoveTask();
        }

        private static void AdjustConsumeTask(string dataJson)
        {
            // 读取任务名和任务增减数量
            var taskInfo = JsonSerializer.Deserialize<ConsumeTaskModel>(dataJson);
            if (taskInfo == null)
            {
                return;
            }

            var dllTask = list.FirstOrDefault(t => t.FileName == taskInfo.TaskName);

            if (taskInfo.TaskCount > 0)
            {
                // 加载dll
                if (dllTask == null)
                {
                    AddFileDll(list, taskInfo.TaskName);

                    dllTask = list.FirstOrDefault(t => t.FileName == taskInfo.TaskName);
                    if (dllTask == null)
                    {
                        Console.WriteLine($"调整消费者任务数量失败，{taskInfo.TaskName}，数量：{taskInfo.TaskCount}");
                        return;
                    }
                }

                dllTask.LoadConsumeTask(taskInfo);
            }
            else
            {
                if (dllTask == null)
                {
                    return;
                }

                dllTask.AbortConsumeTask(taskInfo);
            }
        }


        public static async Task<ClientWebSocket> CreateAsync(string ServerUri)
        {
            var webSocket = new ClientWebSocket();
            webSocket.Options.RemoteCertificateValidationCallback = delegate { return true; };

            await webSocket.ConnectAsync(new Uri(ServerUri), CancellationToken.None);
            if (webSocket.State == WebSocketState.Open)
            {
                return webSocket;
            }
            return null;
        }

        static FreeScheduler.Scheduler _scheduler;
        static IFreeSql _fsql;


        public static void ScheduleTasks()
        {
            _fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, "data source=:memory:")
                .UseAutoSyncStructure(true)
                .UseNoneCommandParameter(true)
                //.UseMonitorCommand(cmd => Console.WriteLine($"=========sql: {cmd.CommandText}\r\n"))
                .Build();

            _scheduler = new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {
                    // 任务的执行主体部分

                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {task.Topic} 被执行，还剩 {_scheduler.QuantityTask} 个循环任务");

                    //if (task.CurrentRound > 5)
                    //    task.Status = FreeScheduler.TaskStatus.Completed;
                })
                .UseCustomInterval(task =>
                {
                    //return TimeSpan.FromSeconds(5);
                    return null;
                })
                .UseStorage(_fsql)
                .Build();

            //// 人工执行，创建任务，到指定时间运行，获取任务ID
            //var taskId = _scheduler.AddTask($"test_task_{DateTime.Now.ToString("g")}", $"test_task01_body{DateTime.Now.ToString("g")}", new[] { 20, 30, 30, 30, 50, 50, 50, 50, 110, 110 });
            //Console.WriteLine($"创建任务：{taskId}");

            //// 暂停任务
            //_scheduler.PauseTask(taskId);
            //Console.WriteLine($"暂停任务：{taskId}");

            //// 恢复任务
            //_scheduler.ResumeTask(taskId);
            //Console.WriteLine($"恢复任务：{taskId}");

            //// 结束任务
            //_scheduler.RemoveTask(taskId);
            //Console.WriteLine($"删除任务：{taskId}");

            //// 创建任务，并立即执行
            //var taskNowId = $"test_task01_body{DateTime.Now.ToString("g")}";
            //_scheduler.RunNowTask(taskNowId);
            //Console.WriteLine($"创建任务并立即执行：{taskNowId}");

            //// 运行临时任务
            //var tempTaskId = _scheduler.AddTempTask(TimeSpan.FromSeconds(5), ActionCallback);

            //// 删除临时任务
            //_scheduler.RemoveTempTask(tempTaskId);
            //Console.WriteLine($"删除临时任务：{tempTaskId}");


            // 创建任务，Corn表达式
            var customerTaskId = _scheduler.AddTaskCustom($"test_customtask_{DateTime.Now.ToString("g")}", $"test_customtask01_body{DateTime.Now.ToString("g")}", "0/1 * * * * ?");

            //Task.Delay(10000).Wait();

            //// 暂停任务
            //_scheduler.PauseTask(customerTaskId);
            //Console.WriteLine($"暂停任务Corn: {customerTaskId}");

            //// 恢复任务
            //_scheduler.ResumeTask(customerTaskId);
            //Console.WriteLine($"恢复任务Corn: {customerTaskId}");

            //// 删除任务
            //_scheduler.RemoveTask(customerTaskId);
            //Console.WriteLine($"删除任务Corn: {customerTaskId}");
        }

        private static void ActionCallback()
        {
            Console.WriteLine($"临时任务触发");
        }
    }


}
