using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public enum TaskType
    {
        定时循环任务 = 1,
        临时单次任务 = 2,
        消息队列消费者 = 3
    }
}
