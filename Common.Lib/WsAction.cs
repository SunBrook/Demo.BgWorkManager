using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public enum WsAction
    {
        执行端基础信息 = 1,
        请求任务列表 = 2,
        发送任务状态 = 3,
        加载并执行任务 = 4,
        任务暂停 = 5,
        任务恢复 = 6,
        任务删除 = 7,
        消费者任务调整 = 8
    }
}
