using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model
{
    class QueueData
    {
        public int queuePkId { get; set; }
        public string eesCode { get; set; }
        public int eesNumber { get; set; }
        public string kioskId { get; set; }

        public string customerId { get; set; }
        public int customerPkId { get; set; }
        public int requestType { get; set; }
        public int priority { get; set; }
        public int status { get; set; }

        public DateTime procStartTime { get; set; }
        public DateTime procEndTime { get; set; }
        public string assignedThreadId { get; set; }
     //   public bool isHighCar { get; set; }
        public ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType { get; set; }
        
        public string gate { get; set; }
        public int cancelReqType { get; set; }
        public bool isAborted { get; set; }
        public int retrievalType { get; set; }
        public bool needWash { get; set; }

        public int washStatus { get; set; }
        public string patronName { get; set; }
        public bool isMember { get; set; }
        public string plateNumber { get; set; }

        public bool isEntry { get; set; }
        public int pathPkId { get; set; }
        public bool isExcecuteInitial { get; set; }

        //29Oct18
        public int MachineSearchFlag { get; set; }
        
    }
}
