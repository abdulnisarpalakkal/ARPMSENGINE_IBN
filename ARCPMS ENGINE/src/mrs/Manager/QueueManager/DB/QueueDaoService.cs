using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.DB
{
    interface QueueDaoService
    {
        QueueData InsertQueue(QueueData objQueueData);
        bool UpdateEESCarData(int queueId, ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType);
        int GetPendingQueueDataForProcessing();
        List<Model.QueueData> GetAllProcessingQId();
        //int GetPendingTransferForProcessing();
        Model.QueueData GetQueueData(int queueId);
        List<int> GetCancelledQueueId();
        /// <summary>
        /// get the cancellation type of given queueid
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        int GetCancelledStatus(int queueId);
        bool UpdateAbortedStatus(int queueId);
        List<Model.DisplayData> GetDisplayData();

        bool NeedToOptimizePath(int queueId);
        bool SetQueueStatus(int queueId, int processStatus);
        /// <summary>
        /// getting the request status: on 16Apr17
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        int GetQueueStatus(int queueId);
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

        /// <summary>
        /// set Reallocate flag of transaction
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool SetReallocateData(decimal queueId, string machineCode, int reallocateFlag);
        /// <summary>
        /// get hold request flag
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool GetHoldRequestFlagStatus(int queueId);
        /// <summary>
        /// Set hold requeust flag
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="holdReqStatus"></param>
        void SetHoldReqFlagStatus(int queueId, bool holdReqStatus);

        //29Oct18
        int GetQueueAvailableStatus(int queueId);
    }
}
