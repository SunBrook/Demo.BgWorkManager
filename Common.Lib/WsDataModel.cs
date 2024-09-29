using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Lib
{
    public class WsDataModel
    {
        public string ID { get; set; }
        public string ConId { get; set; }
        public string ConName { get; set; }
        public WsAction Action { get; set; }
        public string DataJson { get; set; }
    }
}
