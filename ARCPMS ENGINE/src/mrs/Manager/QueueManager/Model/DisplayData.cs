using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model
{
    class DisplayData
    {
        public int queuePkId { get; set; }
        public int gateNumber { get; set; }
        public string cardId { get; set; }
        public string patronName { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public string CarWashStatus { get; set; }
    }
}
