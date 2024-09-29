using System;
using System.Threading.Tasks;
using System.Linq;
using Common.Lib;
using DisposeHub.Con;

namespace Tasks.Lib.Demo
{

    /// <summary>
    /// 临时单次任务
    /// </summary>
    public class TempTask : TaskJob
    {
        public override string Version => "1.1.0";

        public override string TaskGroup => "查询任务";

        public override TaskType TaskType => TaskType.临时单次任务;

        public override string Describe => "【定时任务】查询任务";

        public override string Corn { get => "0/5 * * * * *"; set { Corn = value; } }

        public override TimeSpan EstMaxExecuteTime { get => new TimeSpan(1, 0, 0); set { EstMaxExecuteTime = value; } }

        public override string FoldName => "Consumes";

        public override string SuccessLog => "TempTask";

        public override string ErrorLog => "TempTask_Error";


        public override void Work(string taskId = null)
        {
            try
            {
                SocketManager.SendTaskState(nameof(TempTask), TaskState.任务开始, "任务开始了");

                Console.WriteLine($"【临时单次任务】 {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // 随机进入异常
                var sp = new Random();
                if (sp.Next(0, 2) == 0)
                {
                    throw new Exception("程序异常");
                }

                SocketManager.SendTaskState(nameof(TempTask), TaskState.任务完成, "任务完成了");
            }
            catch (Exception ex)
            {
                SocketManager.SendTaskState(nameof(TempTask), TaskState.任务异常退出, ex.ToString());
            }

        }
    }
}
