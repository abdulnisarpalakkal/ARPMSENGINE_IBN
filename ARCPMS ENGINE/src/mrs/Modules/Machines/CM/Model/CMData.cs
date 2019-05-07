using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model
{
    [Serializable]
    class CMData
    {
        public int cmPkId { get; set; }
        public string cmName { get; set; }
        public int floor { get; set; }
        public int actualAisleMin { get; set; }
        public int actualAisleMax { get; set; }
        public int virtualAisleMin { get; set; }
        public int virtualAisleMax { get; set; }
        public int homeAisle { get; set; }
        public string machine { get; set; }
        public string cmChannel { get; set; }
        public string machineCode { get; set; }
        public bool isBlocked { get; set; }
        public int positionAisle { get; set; }
        public int homeRow { get; set; }
        public int positionRow { get; set; }
        public int status { get; set; }
        public int autoMode { get; set; }
        public int blockedForHome { get; set; }
        public int crossReferenceClear { get; set; }
       
        public int isRotating { get; set; }
        public string collapseMachine { get; set; }
        public int moveSide { get; set; }
        
       
        public int isPalletPresent { get; set; }
        public int floorCmIndex { get; set;}
        public int queueId { get; set; }

        public int destFloor { get; set; }
        public int destAisle { get; set; }
        public int destRow { get; set; }
        public string command { get; set; }
        public bool isDone { get; set; }
        public int destRefAisle { get; set; }

        public string remCode { get; set; }

        public int idleCount { get; set; }
        public bool isHomeMove { get; set; }
        public bool needToPush { get; set; }
        public bool isWait { get; set; }
        public bool CMPresentOnSide { get; set; }
        public ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType { get; set; }
        public int cmdVal { get; set; }
        public string interactMachine { get; set; }


        //for auto refresh
        public bool isClearing { get; set; }
        public string pivotCMCode { get; set; }

        public int requestType { get; set; }
        
       
    }
}
