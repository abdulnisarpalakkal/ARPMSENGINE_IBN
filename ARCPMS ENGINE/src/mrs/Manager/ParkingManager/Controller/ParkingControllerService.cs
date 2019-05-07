using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;


namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller
{
    interface ParkingControllerService
    {
        /// <summary>
        /// for adding new request into queue
        /// </summary>
        /// <param name="objQueueData"></param>
        void AddRequestIntoQueue(QueueData objQueueData);
        void EntryCarProcessing(QueueData objQueueData);
        void ExitCarProcessing(QueueData objQueueData);
        /// <summary>
        /// for iterating queue entries : on 16apr17
        /// </summary>
        void ProcessQueue();
        void IterateForPath(QueueData objQueueData);
        void ExcecuteCommands(QueueData objQueueData);
        void ExcecuteCommandsForPMS(QueueData objQueueData);
        List<Model.PathDetailsData> GetDynamicPath(int queueId);
        List<Model.PathDetailsData> GetInitialPath(int queueId);
        List<Model.PathDetailsData> GetVLCDynamicPath(int queueId);
        List<Model.PathDetailsData> GetAllocatePath(int queueId);
        
        bool CallResetProcedure();
       
        CMData ConvertToCMData(PathDetailsData objPathDetailsData);
        VLCData ConvertToVLCData(PathDetailsData objPathDetailsData);
        EESData ConvertToEESData(PathDetailsData objPathDetailsData);
        ErrorData ConvertToErrorData(PathDetailsData objPathDetailsData);
        bool UpdateMachineBlockStatus(string unblockMachine);
        List<Model.PathDetailsData> GetPathDetails(int queueId, int pathId = 0);
        bool UpdateLiveCommandOfCM(ErrorData objErrorData);
        void HomePositionMoveTrigger();
        void CheckCommandDoneInParallel(PathDetailsData objPathDetailsData, CMData objCMData);
        bool FindOptimizedPath(int queueId);
        bool GetIterationStatus(int queueId);
        void CallResumeProcedure();
        void VLCHomePositionMoveTrigger();
        //29Oct18
        void ProcessQueueDefault();
        void ProcessQueueForEES();
        void ProcessQueueForLCM();
        void ProcessQueueForUCM();
        void ProcessQueueForVLC();
        void FindPath(QueueData objQueueData);
    }
}
