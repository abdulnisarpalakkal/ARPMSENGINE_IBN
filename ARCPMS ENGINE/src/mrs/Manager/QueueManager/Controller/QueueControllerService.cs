using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller
{
    interface QueueControllerService
    {
        void RequestListener();
       
        void ReadEntryKioskData();
        void ReadExitKioskData();
        void ReadQueueEntryData();
        void ReadQueueCancelData();
        bool UpdateAbortedStatus(int queueId);
        void CancelIfRequested(int queueId);
        void removeFinishedThread();
        bool UpdateEESCarData(int queueId, ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType);
        void CreateDispalyXML();
        Model.QueueData GetQueueData(int queueId);
        bool NeedToOptimizePath(int queueId);
        void SetTransactionAbortStatus(int queueId, int abortStatus);
        /// <summary>
        /// setting flag for holding transactions
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="holdStatus"></param>
        void SetHoldFlagStatus(int queueId, bool holdStatus);
        /// <summary>
        /// get status of holding flag
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool GetHoldFlagStatus(int queueId);
        bool Dispose();
        /// <summary>
        /// set Reallocate flag of transaction
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool SetReallocateData(decimal queueId, string machineCode, int reallocateFlag);

        /// <summary>
        /// for resuming 
        /// </summary>
        void DoResumeEngine();
    }
}
