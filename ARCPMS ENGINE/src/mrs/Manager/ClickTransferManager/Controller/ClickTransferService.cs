using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;



namespace ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.Controller
{
    interface ClickTransferService
    {
        /// <summary>
        /// control transfer transaction
        /// </summary>
        /// <param name="objQueueData"></param>
        void ProcessTransfer(QueueData objQueueData);
        /// <summary>
        /// get initial transfer path
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        List<PathDetailsData> GetInitialTransferPath(int queueId);
        /// <summary>
        /// get dynamic transfer path
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        List<PathDetailsData> GetDynamicTransferPath(int queueId);
        /// <summary>
        /// update after transfer process
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool UpdateAfterTransfer(int queueId);


    }
}
