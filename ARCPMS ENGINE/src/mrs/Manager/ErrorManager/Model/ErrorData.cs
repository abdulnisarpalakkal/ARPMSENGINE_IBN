using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model
{
    class ErrorData
    {
        public int triggerPkId { get; set; }
        public string machine { get; set; }
        public string command { get; set; }
        public int floor { get; set; }

        public int aisle { get; set; }
        public int floor_row { get; set; }
        public bool done { get; set; }
        public bool isTrigger { get; set; }

        public int nKey { get; set; }
        public string nValue { get; set; }
        public int queueId { get; set; }
        public string errorCode { get; set; }
        public int seqId { get; set; }

    }
}
