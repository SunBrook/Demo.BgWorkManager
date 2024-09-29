using System;
using System.Threading.Tasks;
using System.Linq;
using Common.Lib;
using DisposeHub.Con;

namespace Tasks.Lib.Demo
{
    /// <summary>
    /// 消费者任务：同时开启多个
    /// </summary>
    public class ConsumeTask : TaskJob
    {
        public override string Version => "1.1.0";

        public override string TaskGroup => "查询任务";

        public override TaskType TaskType => TaskType.消息队列消费者;

        public override string Describe => "【消息队列消费者】查询任务";

        public override string Corn { get => "0/2 * * * * *"; set { Corn = value; } }

        public override TimeSpan EstMaxExecuteTime { get => new TimeSpan(1, 0, 0); set { EstMaxExecuteTime = value; } }

        public override string FoldName => "Consumes";

        public override string SuccessLog => "ConsumeTask";

        public override string ErrorLog => "ConsumeTask_Error";


        public override void Work(string taskId = null)
        {
            while (!LoadDLL.TaskAliveDict.ContainsKey(taskId))
            {
                System.Threading.Thread.Sleep(1);
            }

            while (LoadDLL.TaskAliveDict[taskId])
            {
                try
                {
                    SocketManager.SendTaskState(nameof(ConsumeTask), TaskState.任务开始, $"任务开始了: {taskId}");

                    System.Threading.Thread.Sleep(2000);

                    Console.WriteLine($"【消费者任务】 {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                    var sp = new Random();
                    if (sp.Next(0, 2) == 0)
                    {
                        throw new Exception("程序异常");
                    }

                    //SocketManager.SendTaskState(nameof(ConsumeTask), TaskState.任务完成, "任务完成了");
                }
                catch (Exception ex)
                {
                    SocketManager.SendTaskState(nameof(ConsumeTask), TaskState.任务异常退出, ex.ToString());
                    // TODO Log
                }

                System.Threading.Thread.Sleep(1);
            }

            // 通知任务关闭
            SocketManager.SendTaskState(nameof(ConsumeTask), TaskState.任务关闭, $"任务关闭：{taskId}");

            // 删除键值
            var removeResult = LoadDLL.TaskAliveDict.TryRemove(taskId, out bool value);
            if (!removeResult)
            {
                Console.WriteLine("任务删除失败");
            }
        }
    }
}
