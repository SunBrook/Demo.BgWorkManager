using System;
using System.Threading.Tasks;
using System.Linq;
using DisposeHub.Con;
using Common.Lib;

namespace Tasks.Lib
{
    /// <summary>
    /// 定时循环任务
    /// </summary>
    public class CircleTask : TaskJob
    {
        public override string Version => "1.1.0";

        public override string TaskGroup => "循环任务";

        public override TaskType TaskType => TaskType.定时循环任务;

        public override string Describe => "【定时任务】循环任务";

        public override string Corn { get => "0/10 * * * * *"; set { Corn = value; } }

        public override TimeSpan EstMaxExecuteTime { get => new TimeSpan(1, 0, 0); set { EstMaxExecuteTime = value; } }

        public override string FoldName => "Consumes";

        public override string SuccessLog => "CircleTask";

        public override string ErrorLog => "CircleTask_Error";


        public override void Work(string taskId = null)
        {
            try
            {
                SocketManager.SendTaskState(nameof(CircleTask), TaskState.任务开始, "任务开始了");

                Console.WriteLine($"【循环任务】任务逻辑操作 {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                //System.Threading.Thread.Sleep(2000);
                Task.Delay(1000).Wait();

                



                // 随机进入异常
                var sp = new Random();
                if (sp.Next(0, 2) == 0)
                {
                    throw new Exception("程序异常");
                }

                SocketManager.SendTaskState(nameof(CircleTask), TaskState.任务完成, "任务完成了");
            }
            catch (Exception ex)
            {
                SocketManager.SendTaskState(nameof(CircleTask), TaskState.任务异常退出, ex.ToString());
                // TODO Log
            }
        }
    }
}
