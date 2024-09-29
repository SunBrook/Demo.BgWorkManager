using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public class UseMsg
    {
        /// <summary>
        /// 当前用户
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// 发送人员
        /// </summary>
        public int ToUid { get; set; }

        /// <summary>
        /// 房间号
        /// </summary>
        public int RoomId { get; set; }

        public MsgAction Action { get; set; }
        public string MsgInfo { get; set; }
    }
}
