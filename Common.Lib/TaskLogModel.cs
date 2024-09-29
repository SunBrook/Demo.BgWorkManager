using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public class TaskLogModel
    {
        public string TaskName { get; set; }
        public TaskState TaskState { get; set; }
        public string Log { get; set; }
    }
}
