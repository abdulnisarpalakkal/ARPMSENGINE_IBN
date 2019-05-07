using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using System.Threading;
using System.IO;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.DB;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller;
using ARCPMS_ENGINE.src.mrs.Config;


namespace ARCPMS_ENGINE.src.mrs.Manager.PvlManager.Controller
{
    class PVLManagerImp:PVLManagerService
    {
        CMControllerService objCMControllerService = null;
        
        ErrorControllerService objErrorControllerService = null;
        ParkingControllerService objParkingControllerService = null;
        PVLControllerService objPVLControllerService = null;


        public int InsertQueueForPalletBundle(PVLData objPVLData)
        {
            int pathId = 0;
            if (objPVLControllerService==null) objPVLControllerService=new PVLControllerImp();
            if (objPVLData.isStore)
            {
                pathId = objPVLControllerService.FindPalletStoringSlotAndPath(objPVLData);
            }
            else
            {
                pathId = objPVLControllerService.FindPalletGettingSlotAndPath(objPVLData);
            }
            return pathId;
        }

        public bool ProcessPVLRequest(PVLData objPVLData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered ProcessPVLRequest : PVL=" + objPVLData.machineCode + ", isStore=" + objPVLData.isStore);
            int pathId = 0;

            QueueData objQueueData = new QueueData();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            List<PathDetailsData> lstPathDetails = null;
            pathId = InsertQueueForPalletBundle(objPVLData);
            bool processStatus=false;
            if (pathId != 0)
            {

                objPVLData.queueId = pathId;

                lstPathDetails = GetSlotAndPathForPVL(objPVLData.queueId);

                objQueueData.pathPkId = objPVLData.queueId;
                objParkingControllerService.ExcecuteCommandsForPMS(objQueueData);
                TaskAfterPvlProcess(objPVLData);
                processStatus = true;
            }
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting ProcessPVLRequest : PVL=" + objPVLData.machineCode + ", isStore=" + objPVLData.isStore
                + ", processStatus=" + processStatus + ", pathId=" + pathId);
            return processStatus;
            
        }

        public List<PathDetailsData> GetSlotAndPathForPVL(int pathId)
        {
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            return objParkingControllerService.GetPathDetails(0, pathId);
        }

        public bool TaskAfterPvlProcess(PVLData objPVLData)
        {
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();
            objPVLControllerService.UpdateAfterPVLTask(objPVLData);
            return true;

        }
        
    }
}
