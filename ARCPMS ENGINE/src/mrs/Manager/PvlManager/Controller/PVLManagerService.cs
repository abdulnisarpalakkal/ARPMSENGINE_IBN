using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.PvlManager.Controller
{
    interface PVLManagerService
    {
        int InsertQueueForPalletBundle(PVLData objPVLData);
        bool ProcessPVLRequest(PVLData objPVLData);
        List<PathDetailsData> GetSlotAndPathForPVL(int queueId); //It will call the function in PVL package and 
                                                       //select path details from parking manager package uisng queue id
        bool TaskAfterPvlProcess(PVLData objPVLData);
       
       
    }
}
