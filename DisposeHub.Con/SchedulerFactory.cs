using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisposeHub.Con
{
    public class SchedulerFactory
    {
        private static SchedulerFactory _schedulerFactory;
        private static FreeScheduler.Scheduler _scheduler;
        private static IFreeSql _fsql;

        private static Action action;

        private SchedulerFactory()
        {
            if (_scheduler == null)
            {
                _scheduler = new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {
                    action();
                })
                .UseCustomInterval(task =>
                {
                    return TimeSpan.FromSeconds(5);
                })
                .UseStorage(_fsql)
                .Build();
            }
        }

    }
}
