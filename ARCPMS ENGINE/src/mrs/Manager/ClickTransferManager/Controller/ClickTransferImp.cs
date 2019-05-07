using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.DB;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;

namespace ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.Controller
{
    class ClickTransferImp: ClickTransferService
    {

        ClickTransferDaoService objClickTransferDaoService = null;
        ParkingControllerService objParkingControllerService = null;
        QueueControllerService objQueueControllerService = null;
        
        public void ProcessTransfer(QueueData objQueueData)
        {
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();

            List<PathDetailsData> lstPathDetails = null;
         //   lstPathDetails = new List<PathDetailsData>();
            try
            {
            

                bool needIteration = false;
                do
                {
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objQueueData.queuePkId);
                    /******/
                    do
                    {
                        lstPathDetails = GetAllocatePath(objQueueData.queuePkId);
                        if (lstPathDetails == null) Thread.Sleep(1500);
                        /**checking transaction deleted or not****/
                        objQueueControllerService.CancelIfRequested(objQueueData.queuePkId);
                        /******/
                    } while (lstPathDetails == null);
                    objParkingControllerService.ExcecuteCommands(objQueueData);
                    needIteration = objParkingControllerService.GetIterationStatus(objQueueData.queuePkId);
                } while (needIteration);

                UpdateAfterTransfer(objQueueData.queuePkId);
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + " --TaskCanceledException 'ProcessTransfer':: " + errMsg.Message);

            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Exception 'ProcessTransfer':: " + errMsg.Message);

            }
            finally
            {

            }
            
        }


        public List<PathDetailsData> GetInitialTransferPath(int queueId)
        {
            if (objClickTransferDaoService == null) objClickTransferDaoService = new ClickTransferDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            objClickTransferDaoService.GetInitialTransferPath(queueId);
            return objParkingControllerService.GetPathDetails(queueId);



        }

        public List<PathDetailsData> GetDynamicTransferPath(int queueId)
        {
            if (objClickTransferDaoService == null) objClickTransferDaoService = new ClickTransferDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            int pathId = 0;
            pathId=objClickTransferDaoService.GetDynamicTransferPath(queueId);
            return objParkingControllerService.GetPathDetails(queueId, pathId);
        }
        public List<PathDetailsData> GetAllocatePath(int queueId)
        {
            if (objClickTransferDaoService == null) objClickTransferDaoService = new ClickTransferDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            int pathId = 0;
            pathId = objClickTransferDaoService.GetAllocatePath(queueId);
            return objParkingControllerService.GetPathDetails(queueId, pathId);
        }

        public bool UpdateAfterTransfer(int queueId)
        {
            if (objClickTransferDaoService == null) objClickTransferDaoService = new ClickTransferDaoImp();
            return objClickTransferDaoService.UpdateAfterTransfer(queueId);
        }
        public bool IsSameFloorTravel(List<PathDetailsData> lstPathDetails)
        {
            bool isSame = true;
            foreach (PathDetailsData pathDetails in lstPathDetails)
            {
                if (pathDetails.machineName.Contains("VLC"))
                {
                    isSame = false;
                    break;
                }

            }
            return isSame;
        }
    }
}
