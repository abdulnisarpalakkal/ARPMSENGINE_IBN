using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.DB
{
    interface ClickTransferDaoService
    {
        /// <summary>
        /// get initial transfer path
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool GetInitialTransferPath(int queueId);
        /// <summary>
        /// get second part of transfer path
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        int GetDynamicTransferPath(int queueId);
        /// <summary>
        /// get next available path if needs
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        int GetAllocatePath(int queueId);
        /// <summary>
        /// update into db once transfer transaction finished
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool UpdateAfterTransfer(int queueId);
    }
}
