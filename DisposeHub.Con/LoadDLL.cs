using Common.Lib;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace DisposeHub.Con
{
    public class LoadDLL
    {
        public TaskJob _task;
        public Thread _thread;

        FreeScheduler.Scheduler _scheduler;
        static IFreeSql _fsql;

        /// <summary>
        /// 核心程序集加载
        /// </summary>
        public AssemblyLoadContext _AssemblyLoadContext { get; set; }

        /// <summary>
        /// 获取程序集
        /// </summary>
        public Assembly _Assembly { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string filePath = string.Empty;

        /// <summary>
        /// 指定位置的插件库集合
        /// </summary>
        AssemblyDependencyResolver resolver { get; set; }

        public string FileName { get; set; }

        public bool LoadFile(string filePath)
        {
            this.filePath = filePath;
            try
            {
                resolver = new AssemblyDependencyResolver(filePath);
                _AssemblyLoadContext = new AssemblyLoadContext(Guid.NewGuid().ToString("N"), true);
                _AssemblyLoadContext.Resolving += _AssemblyLoadContext_Resolving;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var _Assembly = _AssemblyLoadContext.LoadFromStream(fs);
                    var Modules = _Assembly.Modules;
                    foreach (var item in _Assembly.GetTypes())
                    {
                        //if (item.GetInterface("ITask") != null)
                        //{
                        //    _task = (TaskJob)Activator.CreateInstance(item);
                        //    return true;
                        //}

                        if (item.GetMethod("Work") != null)
                        {
                            _task = (TaskJob)Activator.CreateInstance(item);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadFile:{ex}");
            }
            return false;
        }

        private Assembly? _AssemblyLoadContext_Resolving(AssemblyLoadContext context, AssemblyName name)
        {
            Console.WriteLine($"加载：{name.Name}");
            var path = resolver.ResolveAssemblyToPath(name);
            if (!string.IsNullOrEmpty(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return _AssemblyLoadContext.LoadFromStream(fs);
                }
            }
            return null;
        }

        public bool StartTask()
        {
            bool RunState = false;
            try
            {
                if (_task != null)
                {
                    _thread = new Thread(new ThreadStart(_Run));
                    _thread.IsBackground = true;
                    _thread.Start();
                    RunState = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartTask: {ex}");
            }
            return RunState;
        }

        private void _Run()
        {
            try
            {
                _task.Work();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"_Run 任务中断执行：{ex}");
            }
        }

        public bool UnLoad()
        {
            try
            {
                _thread?.Interrupt();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnLoad:{ex}");
            }
            finally
            {
                _thread = null;
            }

            _task = null;

            try
            {
                _AssemblyLoadContext?.Unload();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnLoad: {ex}");
            }
            finally
            {
                _AssemblyLoadContext = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return true;
        }

        string customerTaskId;

        List<Thread> taskThreadList;

        public static ConcurrentDictionary<string, bool> TaskAliveDict { get; private set; } = new ConcurrentDictionary<string, bool>();

        public void Schedule(string cron = "default")
        {
            //_fsql = new FreeSql.FreeSqlBuilder()
            //    .UseConnectionString(FreeSql.DataType.Sqlite, "data source=:memory:")
            //    .UseAutoSyncStructure(true)
            //    .UseNoneCommandParameter(true)
            //    //.UseMonitorCommand(cmd => Console.WriteLine($"=========sql: {cmd.CommandText}\r\n"))
            //    .Build();

            _scheduler = new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {
                    _task.Work();
                })
                .UseTimeZone(TimeSpan.FromHours(8))
                .UseCustomInterval(task =>
                {
                    var now = DateTime.UtcNow;
                    var nextTime = NCrontab.CrontabSchedule.Parse(task.IntervalArgument, new NCrontab.CrontabSchedule.ParseOptions { IncludingSeconds = true }).GetNextOccurrence(now);
                    if (nextTime < now) return TimeSpan.FromSeconds(5);
                    return nextTime.Subtract(now);
                })
                //.UseStorage(_fsql)
                .Build();

            customerTaskId = _scheduler.AddTaskCustom($"test_customtask_{DateTime.Now.ToString("g")}", _task.Describe, cron == "default" ? _task.Corn : cron);
        }

        public void PauseTask()
        {
            var result = _scheduler.PauseTask(customerTaskId);
            Console.WriteLine($"暂停任务Corn [{result}]: {customerTaskId}");
        }

        public void ResumeTask()
        {
            var result = _scheduler.ResumeTask(customerTaskId);
            Console.WriteLine($"恢复任务Corn [{result}]: {customerTaskId}");
        }

        public void RemoveTask()
        {
            var result = _scheduler.RemoveTask(customerTaskId);
            Console.WriteLine($"删除任务Corn [{result}]: {customerTaskId}");
        }

        public void RunTempTask()
        {
            _scheduler = new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {

                })
                .UseTimeZone(TimeSpan.FromHours(8))
                .UseCustomInterval(task =>
                {
                    var now = DateTime.UtcNow;
                    var nextTime = NCrontab.CrontabSchedule.Parse(task.IntervalArgument, new NCrontab.CrontabSchedule.ParseOptions { IncludingSeconds = true }).GetNextOccurrence(now);
                    if (nextTime < now) return TimeSpan.FromSeconds(5);
                    return nextTime.Subtract(now);
                })
                //.UseStorage(_fsql)
                .Build();

            // 可设置延迟多少秒执行，如果在执行前，可以删除。如果执行完成，后台会默认删除掉
            customerTaskId = _scheduler.AddTempTask(TimeSpan.FromSeconds(0), () =>
            {
                _task.Work();
            });
        }

        public void LoadConsumeTask(ConsumeTaskModel taskInfo)
        {
            if (taskThreadList == null)
            {
                taskThreadList = new List<Thread>();
            }

            for (int i = 0; i < taskInfo.TaskCount; i++)
            {
                Thread thread = new Thread(() =>
                {
                    var taskId = Guid.NewGuid().ToString("N").Substring(8);
                    bool isSuccess = LoadDLL.TaskAliveDict.TryAdd(taskId, true);

                    if (isSuccess)
                    {
                        _task.Work(taskId);
                    }
                    else
                    {
                        Console.WriteLine("任务创建失败");
                    }
                });

                thread.Start();
                taskThreadList.Add(thread);
            }

            Console.WriteLine("加载完成");
        }

        public void AbortConsumeTask(ConsumeTaskModel taskInfo)
        {
            if (taskThreadList == null || taskThreadList.Count == 0)
            {
                return;
            }

            var abortCount = taskInfo.TaskCount == 0 ? taskThreadList.Count : Math.Abs(taskInfo.TaskCount);
            var abortKeys = LoadDLL.TaskAliveDict.Take(abortCount).Select(t => t.Key).ToList();
            foreach (var key in abortKeys)
            {
                LoadDLL.TaskAliveDict[key] = false;
            }
        }
    }
}
