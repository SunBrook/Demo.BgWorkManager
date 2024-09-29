using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public abstract class TaskJob
    {
        #region 需要实现的成员

        /// <summary>
        /// 版本号
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// 业务分组
        /// </summary>
        public abstract string TaskGroup { get; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public abstract TaskType TaskType { get; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public abstract string Describe { get; }

        /// <summary>
        /// 执行频率
        /// </summary>
        public abstract string Corn { get; set; }

        /// <summary>
        /// 执行预估最大时长 天 时 分 秒 毫秒
        /// </summary>
        public abstract TimeSpan EstMaxExecuteTime { get; set; }

        /// <summary>
        /// 调试文件夹
        /// </summary>
        public abstract string FoldName { get; }

        /// <summary>
        /// 成功日志
        /// </summary>
        public abstract string SuccessLog { get; }

        /// <summary>
        /// 失败日志
        /// </summary>
        public abstract string ErrorLog { get; }

        #endregion

        public abstract void Work(string taskId = null);
    }
}
