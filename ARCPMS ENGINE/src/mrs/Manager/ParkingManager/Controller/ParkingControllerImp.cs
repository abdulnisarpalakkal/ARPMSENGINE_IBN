using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.DB;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.PalletManager.Controller;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.DBCon;

using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.DB;
using System.Collections.Concurrent;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.DB;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.DB;





namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller
{
    class ParkingControllerImp: ParkingControllerService
    {
        EESControllerService objEESControllerService = null;
        CMControllerService objCMControllerService = null;
        VLCControllerService objVLCControllerService = null;
        PVLControllerService objPVLControllerService = null;
        ErrorControllerService objErrorControllerService = null;
        DB.ParkingDaoService objParkingDaoService = null;
        QueueDaoService objQueueDaoService = null;
        QueueControllerService objQueueControllerService = null;
        PalletManagerService objPalletManagerService = null;
        VLCDaoService objVLCDaoService = null;
        ErrorDaoService objErrorDaoService = null;
        CMDaoService objCMDaoService = null;


        public static ConcurrentQueue<QueueData> RequestQueue = new ConcurrentQueue<QueueData>();
        public static ConcurrentQueue<QueueData> LCMSeachQueue = new ConcurrentQueue<QueueData>();
        public static ConcurrentQueue<QueueData> VLCSeachQueue = new ConcurrentQueue<QueueData>();
        public static ConcurrentQueue<QueueData> UCMSeachQueue = new ConcurrentQueue<QueueData>();
        public static ConcurrentQueue<QueueData> EESSeachQueue = new ConcurrentQueue<QueueData>();
        public static ConcurrentQueue<QueueData> DefaultSeachQueue = new ConcurrentQueue<QueueData>();
        
        
        static object pathSelectionLock = new object();
        object triggerUpdateLock = new object();
        private volatile bool _shouldStop;


        public void AddRequestIntoQueue(QueueData objQueueData)
        {
            RequestQueue.Enqueue(objQueueData);
        }
        public void EntryCarProcessing(QueueData objQueueData)
        {
            int JobStatus = 0;
            bool carAtEES = false;
            int pathStartFlag = 0;
            int printCounter = 0;
            List<Model.PathDetailsData> lstPathDetails = null;
            EESData objEESData = new EESData();
            if (objParkingDaoService==null) objParkingDaoService = new ParkingDaoImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            if (objQueueDaoService == null)
                objQueueDaoService = new QueueDaoImp();
            
            try
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId+":--Parking Started");
                pathStartFlag=objParkingDaoService.GetPathStartFlag(objQueueData.queuePkId);
                if (pathStartFlag == 0)
                {
                    objEESData.queueId = objQueueData.queuePkId;
                    if (objQueueData.eesNumber != 0)
                    {
                        objEESData.eesPkId = objQueueData.eesNumber;
                    }
                    else
                    {
                        objEESData.machineCode = objQueueData.eesCode;
                    }
                    objEESData = objEESControllerService.GetEESDetails(objEESData);

                    do
                    {


                        carAtEES = objEESControllerService.checkCarAtEES(objEESData);
                        if (carAtEES == false) Thread.Sleep(1000);


                        printCounter += 1;
                        if (printCounter > 3 && carAtEES == false)
                        {
                            printCounter = 0;
                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Waiting for CarAAt EES");
                            objQueueControllerService.SetTransactionAbortStatus(objQueueData.queuePkId,1);

                            //checking transaction deleted or not
                            if (objQueueDaoService.GetCancelledStatus(objQueueData.queuePkId) != 0) //remove deleted transactions
                            {
                                
                                objQueueDaoService.UpdateAbortedStatus(objQueueData.queuePkId);
                                throw new OperationCanceledException();
                            }
                           
                            //objQueueControllerService.CancelIfRequested(-1 * objQueueData.queuePkId);

                        }

                    } while (!carAtEES);
                    //int cardId = 0;
                    //bool isNumeric=Int32.TryParse(objQueueData.customerId, out cardId);
                    ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType = objEESControllerService.GetCarType(objEESData);
                    bool needUpdateCarType = false;
                    needUpdateCarType = objQueueData.carType == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.low
                                         && (carType == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.high
                                         || carType == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.medium);

                    needUpdateCarType = needUpdateCarType || objQueueData.carType == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.medium
                                         && carType == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.high;

                    needUpdateCarType = needUpdateCarType || !(objQueueData.customerId.Contains("SM_"));

                    if (needUpdateCarType)
                    {
                        objQueueData.carType = carType;
                        objQueueControllerService.UpdateEESCarData(objQueueData.queuePkId, objQueueData.carType); //update car type to the DB
                    }

                    // objEESControllerService.TakePhoto(objEESData);


                    printCounter = 0;

                    bool getCarReady = false;
                    bool isEESFloor = false;
                    int holdFlagCounter = 0;
                    do
                    {

                        getCarReady = objEESControllerService.checkCarReadyAtEntry(objEESData);
                        printCounter += 1;
                        holdFlagCounter += 1;
                        if (printCounter > 3 && getCarReady == false)
                        {
                            //checking transaction deleted or not
                            if (objQueueDaoService.GetCancelledStatus(objQueueData.queuePkId) != 0) //remove deleted transactions
                            {

                                objQueueDaoService.UpdateAbortedStatus(objQueueData.queuePkId);
                                throw new OperationCanceledException();
                            }
                           

                            Thread.Sleep(1000);
                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Wiating for car ready At Entry");
                            printCounter = 0;

                        }
                        if (holdFlagCounter>10)
                        {
                            holdFlagCounter = 0;
                            objQueueDaoService.SetHoldReqFlagStatus(objQueueData.queuePkId, true);
                            objQueueDaoService.SetHoldFlagStatus(objQueueData.queuePkId, true);
                            do{
                                //checking transaction deleted or not
                                if (objQueueDaoService.GetCancelledStatus(objQueueData.queuePkId) != 0) //remove deleted transactions
                                {

                                    objQueueDaoService.UpdateAbortedStatus(objQueueData.queuePkId);
                                    throw new OperationCanceledException();
                                }
                           
                                 Thread.Sleep(2000);
                            } while (objQueueDaoService.GetHoldRequestFlagStatus(objQueueData.queuePkId));
                            objQueueDaoService.SetHoldFlagStatus(objQueueData.queuePkId, false);
                        }
                        if (getCarReady)
                        {
                            objEESData.command = OpcTags.EES_Auto_ID_Generator;
                            objEESControllerService.ExcecuteEESGetCar(objEESData);
                        }
                    } while (getCarReady == false);


                }

                RequestQueue.Enqueue(objQueueData);
               // IterateForPath(objQueueData);
                
            

            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + " --TaskCanceledException 'Entry Car Processing':: " + errMsg.Message);
                
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Exception 'Entry Car Processing':: " + errMsg.Message);
              
            }
            finally
            {
              
            }
            
        }

        public void ExitCarProcessing(QueueData objQueueData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Retrieval Started");

            objParkingDaoService = new ParkingDaoImp();
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
           

            try
            {
                objQueueControllerService.CreateDispalyXML();
                //checking car type status: bug: 8.4.15.1
                objQueueData.carType = objParkingDaoService.IsExitCarHigh(objQueueData.queuePkId);

               
                           

                RequestQueue.Enqueue(objQueueData);
                //IterateForPath(objQueueData);
               

             
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + " --TaskCanceledException 'ExitCarProcessing':: " + errMsg.Message);
              
            }
            catch (Exception ex)
            {
               
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Exception 'ExitCarProcessing':: " + ex.Message);
                
            }
         
        }

        public void ProcessQueue()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueue()");
            if (objQueueDaoService == null)
                objQueueDaoService = new QueueDaoImp();
            if (objParkingDaoService == null)
                objParkingDaoService = new DB.ParkingDaoImp();
            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (RequestQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!RequestQueue.TryDequeue(out objQueueData))
                            continue;
                        // QueueData objQueueData = RequestQueue.Dequeue();

                        try
                        {


                            if (!PreCheckQueueStatusBeforeFindingPath(objQueueData))
                                continue;
                            objQueueData.MachineSearchFlag = objParkingDaoService.GetMachineSearchFlag(objQueueData.queuePkId);
                            switch (objQueueData.MachineSearchFlag)
                            {
                                case 1:
                                    LCMSeachQueue.Enqueue(objQueueData);
                                    break;
                                case 2:
                                    VLCSeachQueue.Enqueue(objQueueData);
                                    break;
                                case 3:
                                    UCMSeachQueue.Enqueue(objQueueData);
                                    break;
                                case 4:
                                    EESSeachQueue.Enqueue(objQueueData);
                                    break;
                                default:
                                    DefaultSeachQueue.Enqueue(objQueueData);
                                    break;

                            }

                        }

                        catch (Exception ex)
                        {
                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Exception 'ProcessQueue':: " + ex.Message + ":: " + ex.StackTrace);
                            objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, false);
                        }
                        finally
                        {

                        }



                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueue':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);
            
        }

     

        public void IterateForPath(QueueData objQueueData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + " : Entered 'IterateForPath' ");
            bool needIteration = false;
            List<Model.PathDetailsData> lstPathDetails = null;
            do
            {
                /**checking transaction deleted or not****/
                objQueueControllerService.CancelIfRequested(objQueueData.queuePkId);
                /******/

                lstPathDetails = GetAllocatePath(objQueueData.queuePkId);
                ExcecuteCommands(objQueueData);
                needIteration = objParkingDaoService.GetIterationStatus(objQueueData.queuePkId);
            } while (needIteration);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + " : Exitting 'IterateForPath' ");

        }
        public void ExcecuteCommands(QueueData objQueueData)
        {

            if (objEESControllerService==null) objEESControllerService = new EESControllerImp();
            if (objCMControllerService == null) objCMControllerService = new CMControllerImp();
            if (objVLCControllerService == null) objVLCControllerService = new VLCControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            if (objParkingDaoService == null) objParkingDaoService = new ParkingDaoImp();
            if (objPalletManagerService == null) objPalletManagerService = new PalletManagerImp();
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();
            

            ErrorData objErrorData = new ErrorData();
            EESData objEESData = new EESData();
            CMData objCMData = new CMData();
            VLCData objVLCData = new VLCData();
            PVLData objPVLData = new PVLData();
            bool isLiveCmdUpdated=false;
            int queuePkId = 0;
            int eesState = 0;

            int autoRefreshCnt = 0;
            int autoRefreshLmt = 20;
            int pmsMode = 0;
            try
            {

                do
                {
                //foreach (Model.PathDetailsData pathDetails in lstPathDetails.Where(a => a.done == false))
                //{
                    if(objQueueControllerService.NeedToOptimizePath(objQueueData.queuePkId))
                    {
                        FindOptimizedPath(objQueueData.queuePkId);
                    }
                    Model.PathDetailsData pathDetails = objParkingDaoService.GetEachPathDetailsRecord(objQueueData.queuePkId, objQueueData.pathPkId);

                    if (pathDetails == null || pathDetails.sequencePkId == 0)
                    {
                       // if (objQueueData.requestType == 0 || objQueueData.requestType == 1 || objQueueData.requestType == 5 || objQueueData.requestType == 6)
                     //   {
                            if (objParkingDaoService.GetIterationStatus(objQueueData.queuePkId))
                            {
                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Iterating transaction");
                                GlobalValues.threadsDictionary.Remove(objQueueData.queuePkId);
                                RequestQueue.Enqueue(objQueueData);
                            }
                            else
                            {
                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(objQueueData.queuePkId);
                                /******/
                                objParkingDaoService.CallProcedureAfterProcessCar(objQueueData.queuePkId);

                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Transaction Finished");

                            }
                    //    }
                    
                        break;
                    }

                    pathDetails.requestType = objQueueData.requestType;
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objQueueData.queuePkId);
                    /******/

                    objErrorData = null;
                    queuePkId = pathDetails.queueId;
                    autoRefreshCnt = 0;

                    switch (pathDetails.command)
                    {
                        #region Move
                        case OpcTags.CM_L2_Move_Cmd:

                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);

                            do{
                                isLiveCmdUpdated=objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                {
                                    Thread.Sleep(500);

                                    if (ParkConfig.IsAutoRefreshActive(objQueueData.requestType))
                                    {
                                        autoRefreshCnt++;
                                        if (autoRefreshCnt > autoRefreshLmt)
                                        {

                                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                                    + ": CM=" + objCMData.machineCode + " auto refresh in 'ExcecuteCommands' Move");
                                            if (string.IsNullOrEmpty(objCMData.interactMachine))
                                                objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.machineCode, 3);
                                            else
                                                objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.interactMachine, 3);
                                            autoRefreshCnt = 0;
                                            objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);  //refreshing 
                                            throw new OperationCanceledException();
                                        }
                                    }
                                }

                            } while (!isLiveCmdUpdated);

                            if (!pathDetails.confirmTrigger)
                                objCMData.isDone = objCMControllerService.CMMove(objCMData);
                            else
                            {
                                TriggerData objTriggerData = new TriggerData();
                                objTriggerData.TriggerEnabled = true;
                                objTriggerData.MachineCode = objCMData.machineCode;
                                objTriggerData.category = TriggerData.triggerCategory.NA;
                                objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                            }
                            
                           
                            if (!pathDetails.parallelMove)
                                objCMControllerService.CheckCMCommandDone(objCMData);
                            if (!objQueueControllerService.NeedToOptimizePath(objQueueData.queuePkId))
                            {
                                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                                UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            }

                            break;
                        #endregion
                        #region Put
                        case OpcTags.CM_L2_Put_Cmd:

                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            do
                            {
                                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                {
                                    Thread.Sleep(500);
                                    if (ParkConfig.IsAutoRefreshActive(objQueueData.requestType))
                                    {
                                        autoRefreshCnt++;


                                        if (autoRefreshCnt > autoRefreshLmt )
                                        {
                                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                                    + ": CM=" + objCMData.machineCode + " auto refresh in 'ExcecuteCommands' Put");

                                            objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.machineCode, 3);
                                            autoRefreshCnt = 0;
                                            objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);  //refreshing 
                                            throw new OperationCanceledException();
                                        }
                                    }
                                }
                            } while (!isLiveCmdUpdated);
                            if(pathDetails.interactMachineCode.Contains("VLC"))
                            {
                                objVLCData = new VLCData();
                                objVLCData.machineCode = pathDetails.interactMachineCode;
                                objVLCData=objVLCControllerService.GetVLCDetails(objVLCData);
                                objVLCData.queueId = pathDetails.queueId;
                                objVLCData.destFloor = pathDetails.floor;
                                while (objVLCControllerService.VLCAtFloor(objVLCData) != pathDetails.floor)
                                {
                                    objVLCControllerService.VLCMove(objVLCData);
                                    if(objVLCControllerService.IsVLCDisabled(objVLCData.machineCode))
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (pathDetails.interactMachineCode.Contains("PVL"))
                            {
                                objPVLData = new PVLData();
                                objPVLData.machineCode = pathDetails.interactMachineCode;
                                objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                objPVLData.queueId = pathDetails.queueId;
                                objPVLData.destFloor = pathDetails.floor;

                                bool isPVLReady = false;
                                do
                                {
                                    Logger.WriteLogger(GlobalValues.PMS_LOG, "ExcecuteCommands:>Entering PVL Ready check :  Path Id=" + pathDetails.queueId
                        + " ; Is ready " + objPVLData.machineCode + "?");
                                    isPVLReady = objPVLControllerService.ConfirmPVLReadyPUTByCM(objPVLData.machineCode);
                                    Logger.WriteLogger(GlobalValues.PMS_LOG, "ExcecuteCommands:>Exit PVL Ready Check:  Path Id=" + pathDetails.queueId
                       + " ; Is ready " + objPVLData.machineCode + "= " + isPVLReady);

                                    if (objCMControllerService.NeedToShowTrigger(objCMData).TriggerEnabled)
                                        break;
                                    if (!isPVLReady)
                                        Thread.Sleep(500);
                                } while (!isPVLReady);

                            }
                            else if (pathDetails.interactMachineCode.Contains("EES"))
                            {
                                bool isEESReady=false;
                                pmsMode = objPalletManagerService.GetCurrentPMSMode();
                                autoRefreshCnt = 0;
                                do
                                {
                                    isEESReady=objEESControllerService.ConfirmEESReadyForREMLock(pathDetails.interactMachineCode);
                                    /**checking transaction deleted or not****/
                                    objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                    /******/
                                    if (objEESControllerService.IsEESDisabled(pathDetails.interactMachineCode))
                                    {
                                        break;
                                    }

                                    if (!isEESReady)
                                    {
                                        Thread.Sleep(500);
                                        if (ParkConfig.IsAutoRefreshActive(objQueueData.requestType))
                                        {
                                            autoRefreshCnt++;
                                            if (autoRefreshCnt > (pmsMode == 1 ? (autoRefreshLmt + 10) : autoRefreshLmt) )
                                            {
                                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                                        + ": CM=" + objCMData.machineCode + " auto refresh in 'ExcecuteCommands' waiting ees ready");

                                                objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.machineCode, 3);
                                                autoRefreshCnt = 0;
                                                objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);  //refreshing 
                                                throw new OperationCanceledException();
                                            }
                                        }
                                    }
                                       
                                } while (!isEESReady);
                            }
                            if (!pathDetails.confirmTrigger)   // this is for car wash. more dirty car will not be allowed to go for car wash
                                objCMData.isDone = objCMControllerService.CMPut(objCMData);
                            else
                            {
                                TriggerData objTriggerData = new TriggerData();
                                objTriggerData.TriggerEnabled = true;
                                objTriggerData.MachineCode = objCMData.machineCode;
                                objTriggerData.category = TriggerData.triggerCategory.NA;
                                objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                            }
                            
                           // Task.Factory.StartNew(() => objParkingControllerService.ExitCarProcessing(objQueueData), GlobalValues.threadsDictionary[objQueueData.queuePkId].Token);
                            if (pathDetails.interactMachineCode.Contains("EES") && !objEESControllerService.IsEESDisabled(pathDetails.interactMachineCode))
                            {
                                PathDetailsData cmPutPathDetails = new PathDetailsData();
                                CMData cmPutData = new CMData();

                                cmPutPathDetails = BasicConfig.Clone<PathDetailsData>(pathDetails);
                                cmPutData = BasicConfig.Clone<CMData>(objCMData);

                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/

                                Task.Factory.StartNew(() => CheckCommandDoneInParallel(cmPutPathDetails, cmPutData));
                            }
                            else
                            {
                                if (!pathDetails.parallelMove)
                                    objCMControllerService.CheckCMCommandDone(objCMData);
                                if (!objQueueControllerService.NeedToOptimizePath(objQueueData.queuePkId))
                                {
                                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                                    UpdateMachineBlockStatus(pathDetails.unblockMachine);
                                }
                            }
                         
                            break;
                        #endregion
                        #region Get
                        case OpcTags.CM_L2_Get_Cmd:
                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            do
                            {
                                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                {
                                    Thread.Sleep(500);
                                    if (ParkConfig.IsAutoRefreshActive(objQueueData.requestType))
                                    {
                                        autoRefreshCnt++;


                                        if (autoRefreshCnt > autoRefreshLmt )
                                        {
                                            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                                    + ": CM=" + objCMData.machineCode + " auto refresh in 'ExcecuteCommands' Get");

                                            objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.interactMachine, 3);
                                            autoRefreshCnt = 0;
                                            objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);  //refreshing 
                                            throw new OperationCanceledException();
                                        }
                                    }
                                }
                            } while (!isLiveCmdUpdated);
                            if (pathDetails.interactMachineCode.Contains("VLC"))
                            {
                                objVLCData = new VLCData();
                                objVLCData.machineCode = pathDetails.interactMachineCode;
                                objVLCData = objVLCControllerService.GetVLCDetails(objVLCData);
                                objVLCData.queueId = pathDetails.queueId;
                                objVLCData.destFloor = pathDetails.floor;
                                while (objVLCControllerService.VLCAtFloor(objVLCData) != pathDetails.floor)
                                {
                                    objVLCControllerService.VLCMove(objVLCData);
                                    if (objVLCControllerService.IsVLCDisabled(objVLCData.machineCode))
                                    {
                                        break;
                                    }
                                    /**checking transaction deleted or not****/
                                    objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                    /******/
                                }
                            }
                            else if (pathDetails.interactMachineCode.Contains("PVL"))
                            {
                                objPVLData = new PVLData();
                                objPVLData.machineCode = pathDetails.interactMachineCode;
                                objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                objPVLData.queueId = pathDetails.queueId;
                                objPVLData.destFloor = pathDetails.floor;
                                //TODO: need to check PVL healthy status

                            }
                            objCMData.isDone = objCMControllerService.CMGet(objCMData);
                            if (!pathDetails.parallelMove)
                            {
                                
                                    objCMControllerService.CheckCMCommandDone(objCMData);
                                    
                                
                            }
                            if (!objQueueControllerService.NeedToOptimizePath(objQueueData.queuePkId))
                            {
                                if (pathDetails.callUpadate)
                                    objParkingDaoService.UpdateSlotAfterGetCarFromSlot(pathDetails.queueId);
                                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                                UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            }
                            break;
                        #endregion
                        #region Rotate
                        case OpcTags.LCM_Rotate_to_0_from_L2:

                            
                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objCMData.carType = objQueueData.carType;
                            objErrorData = ConvertToErrorData(pathDetails);
                            bool isAlreadyRotated = objParkingDaoService.IsRotated(pathDetails.queueId);
                            bool isSkipped = !objCMDaoService.GetCMRotationEnabledStatus(objCMData.machineCode);



                            if (!isAlreadyRotated && !isSkipped)
                            {
                                isSkipped = true;
                                int triggerUpdateCnt = 0;
                                int triggerUpdateCheckLimit = 3;
                                if (objCMData.cmdVal == 1)
                                    triggerUpdateCheckLimit = 10;
                                else if (objCMData.cmdVal == 2)
                                    triggerUpdateCheckLimit = 500;
                                do
                                {
                                    isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                    triggerUpdateCnt++;
                                    if (!isLiveCmdUpdated)
                                        Thread.Sleep(500);
                                   
                                } while (!isLiveCmdUpdated && triggerUpdateCnt < triggerUpdateCheckLimit);
                                if (triggerUpdateCnt < triggerUpdateCheckLimit)    // if l2 cant able to update in 3 seconds, it will skip the rotation
                                {
                                    isSkipped = false;
                                    objCMData.isDone = objCMControllerService.CMRotate(objCMData);

                                    if (objCMData.isDone)
                                    {
                                        objCMControllerService.CheckCMRotateDone(objCMData);
                                        objParkingDaoService.UpdateRotaion(pathDetails.queueId, true);
                                    }
                                    
                                }
                            }
                            if (isAlreadyRotated || isSkipped)
                                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData); //for updating the sequence number

                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true); //note: this should call even if failed to execute,
                                                                                                                      // otherwise slot will not update
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);

                            break;
                        #endregion
                        #region CP_Start
                        case OpcTags.VLC_CP_Start:
                          



                            if (pathDetails.machine.Contains("VLC"))
                            {
                                objVLCData = null;
                                objVLCData = ConvertToVLCData(pathDetails);
                                objErrorData = ConvertToErrorData(pathDetails);
                                objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);

                                objVLCData.isDone = objVLCControllerService.VLCMove(objVLCData);
                                if (!pathDetails.parallelMove)
                                    objVLCControllerService.CheckVLCCommandDone(objVLCData);

                                if (!objQueueControllerService.NeedToOptimizePath(objQueueData.queuePkId))
                                {
                                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objVLCData.machineCode, true);
                                    UpdateMachineBlockStatus(pathDetails.unblockMachine);
                                }

                            }
                            else //PVL
                            {

                                objPVLData = null;
                                objPVLData = ConvertToPVLData(pathDetails);
                                objErrorData = ConvertToErrorData(pathDetails);
                                objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);

                                objPVLData.isDone = objPVLControllerService.PVLMove(objPVLData);
                                if (!pathDetails.parallelMove)
                                    objPVLControllerService.CheckPVLCommandDone(objPVLData);
                                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objPVLData.machineCode, true);

                                UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            }




                            break;
                        #endregion
                        #region EES_Get_Car
                        case OpcTags.EES_Auto_ID_Generator:

                            objEESData = null;
                            objEESData = ConvertToEESData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                            //objEESData.isDone = objEESControllerService.ExcecuteEESGetCar(objEESData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objEESData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            break;
                        #endregion
                        #region EES_Mode
                        case OpcTags.EES_Mode:

                            pmsMode=objPalletManagerService.GetCurrentPMSMode();
                            objEESData = null;
                            objEESData = ConvertToEESData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                            bool needToChangeMode = false;
                            if (pmsMode != 2)
                            {

                                if (pathDetails.cmd_val_in_number == GlobalValues.EESExit)
                                {
                                    objEESData.eesMode = GlobalValues.EESExit;

                                    bool isReady = false;
                                    do
                                    {
                                        needToChangeMode = objEESControllerService.IsEESEntryInOPC(objEESData);
                                        isReady = objEESControllerService.IsEESReadyForParkingChangeMode(objEESData);
                                        if (objEESControllerService.IsEESDisabled(objEESData.machineCode))
                                            break;
                                        /**checking transaction deleted or not****/
                                        objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                        /******/
                                        if (!isReady && needToChangeMode)
                                            Thread.Sleep(500);
                                    } while (!isReady && needToChangeMode);
                                    if (needToChangeMode)
                                        objEESData.isDone = objEESControllerService.ChangeMode(objEESData);


                                }
                                else if (pathDetails.cmd_val_in_number == GlobalValues.EESEntry)
                                {
                                    if (objEESControllerService.IsEESEntryInCurrentModeInDB(objEESData.machineCode))
                                    {

                                        objEESData.eesMode = GlobalValues.EESEntry;
                                        Thread.Sleep(3000);
                                        bool isReady = false;
                                        do
                                        {
                                            needToChangeMode = !objEESControllerService.IsEESEntryInOPC(objEESData);
                                            isReady = objEESControllerService.IsEESReadyForParkingChangeModeBack(objEESData);
                                            /**checking transaction deleted or not****/
                                            objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                            /******/
                                            if (!isReady && needToChangeMode)
                                                Thread.Sleep(500);
                                        } while (!isReady && needToChangeMode);
                                        if (needToChangeMode)
                                        {

                                            objEESData.isDone = objEESControllerService.ChangeMode(objEESData);
                                        }
                                    }
                                }


                            }
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objEESData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);

                            if (pathDetails.cmd_val_in_number == GlobalValues.EESEntry
                                && objEESControllerService.IsEESBlockedInDBForPMS(objEESData.machineCode))
                            {
                                //release ees block for PMS which is blocked before giving payment done command
                                objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode, false);
                            }
                            break;
                        #endregion
                        #region EES_Payment_Is_Done
                        case OpcTags.EES_Payment_Is_Done:

                           
                            objEESData = null;
                            objEESData = ConvertToEESData(pathDetails);

                            // block EES for PMS because In morning mode or normal mode, it may need to change back to morning mode
                            objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode,true);

                            objErrorData = ConvertToErrorData(pathDetails);
                     
                             eesState=0;
                             do
                             {
                                 eesState = objEESControllerService.GetEESState(objEESData);
                                 /**checking transaction deleted or not****/
                                 objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                 /******/
                                 if (eesState != 204 && eesState != 206)
                                     Thread.Sleep(800);
                             } while (eesState != 204 && eesState != 206 );


                            objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                            objQueueControllerService.CreateDispalyXML(); // for showing the gate number
                            objEESData.isDone = objEESControllerService.ExcecutePaymentDone(objEESData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objEESData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);
                           
                            objParkingDaoService.UpdateCarReachedAtEESTime(pathDetails.queueId);
                            string cardId=objParkingDaoService.GetCardIdUsingQueueId(pathDetails.queueId);
                             bool hasCarAtEES=false;
                            do{
                                hasCarAtEES = objEESControllerService.checkCarAtEES(objEESData);
                                /**checking transaction deleted or not****/
                                objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (hasCarAtEES)
                                    Thread.Sleep(500);
                            } while (hasCarAtEES);
                            break;
                        #endregion
                      
                        #region EES_Cam_GetCmd
                        case OpcTags.EES_Cam_GetCmd:

                            QueueData objQueueData1 = null;
                            objEESData = null;
                            objEESData = ConvertToEESData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                          
                            objEESData.isEntry = false;
                            objQueueData1 = objQueueControllerService.GetQueueData(objEESData.queueId);
                            objEESData.isEntry = objQueueData1.isEntry;
                            objEESData.customerPkId = objQueueData1.customerPkId;

                            objQueueControllerService.CreateDispalyXML(); // for showing the gate number


                             eesState=0;
                             do
                             {
                                 eesState = objEESControllerService.GetEESState(objEESData);
                                 /**checking transaction deleted or not****/
                                 objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                 /******/
                                 if (eesState != 204 && eesState != 108 && eesState != 206)
                                     Thread.Sleep(800);
                             } while (eesState != 204 && eesState != 108 && eesState != 206 );
                            objEESData.isDone = objEESControllerService.TakePhoto(objEESData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objEESData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);


                            break;
                        #endregion
                        default:

                            break;
                    }

                } while (true);
            }
            catch (OperationCanceledException errMsg)
            {
               
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queuePkId + " --TaskCanceledException 'ExecuteCommands':: " + errMsg.Message);
                //if (!(objQueueData.requestType == 0 || objQueueData.requestType == 1 || objQueueData.requestType == 5 || objQueueData.requestType == 6))
                //{
                //    throw new OperationCanceledException();
                //}
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queuePkId + ":--Exception 'ExecuteCommands':: " + ex.Message + ":: " + ex.StackTrace);
                //if (!(objQueueData.requestType == 0 || objQueueData.requestType == 1 || objQueueData.requestType == 5 || objQueueData.requestType == 6))
                //{
                //    if (ex is TaskCanceledException)
                //        throw new Exception();
                //}
            }
            finally
            {
            }
        }
        public void ExcecuteCommandsForPMS(QueueData objQueueData)
        {
            
            objEESControllerService = new EESControllerImp();
            objCMControllerService = new CMControllerImp();
            objPVLControllerService = new PVLControllerImp();
            objErrorControllerService = new ErrorControllerImp();
            objParkingDaoService = new ParkingDaoImp();

            ErrorData objErrorData = new ErrorData();
            EESData objEESData = new EESData();
            CMData objCMData = new CMData();
            PVLData objPVLData = new PVLData();
            bool isLiveCmdUpdated = false;
            try
            {

               // foreach (Model.PathDetailsData pathDetails in lstPathDetails.Where(a => a.done == false))
              //  {
                do
                {

                    Model.PathDetailsData pathDetails = objParkingDaoService.GetEachPathDetailsRecord(objQueueData.queuePkId, objQueueData.pathPkId);

                    if (pathDetails == null || pathDetails.sequencePkId == 0 )
                        break;
                    Logger.WriteLogger(GlobalValues.PMS_LOG, "ExcecuteCommandsForPMS: Path Id=" + pathDetails.pathPkId + "; machine:" + pathDetails.machine
                        + " ; command:" + pathDetails.command);
                    objErrorData = null;

                    switch (pathDetails.command)
                    {
                        case OpcTags.CM_L2_Move_Cmd:

                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            // objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                            
                            do
                            {
                                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                //objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                    Thread.Sleep(500);
                            } while (!isLiveCmdUpdated);
                            objCMData.isDone = objCMControllerService.CMMove(objCMData);
                            if (!pathDetails.parallelMove)
                                objCMControllerService.CheckCMCommandDone(objCMData);
                            //objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);

                            break;
                        case OpcTags.CM_L2_Put_Cmd:
                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            do
                            {
                                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                //objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                    Thread.Sleep(500);
                            } while (!isLiveCmdUpdated);

                            if (pathDetails.interactMachineCode.Contains("PVL"))
                            {
                                bool isPVLReady = false;
                                do
                                {
                                    Logger.WriteLogger(GlobalValues.PMS_LOG, "ExcecuteCommandsForPMS:>Entering PVL Ready check :  Path Id=" + pathDetails.pathPkId
                        + " ; Is ready " + pathDetails.interactMachineCode + "?");
                                    isPVLReady = objPVLControllerService.ConfirmPVLReadyPUTByCM(pathDetails.interactMachineCode);
                                    Logger.WriteLogger(GlobalValues.PMS_LOG, "ExcecuteCommandsForPMS:>Exit PVL Ready Check:  Path Id=" + pathDetails.pathPkId
                       + " ; Is ready " + pathDetails.interactMachineCode + "= " + isPVLReady);
                                    if (objCMControllerService.NeedToShowTrigger(objCMData).TriggerEnabled)
                                        break;
                                    if (!isPVLReady)
                                        Thread.Sleep(500);
                                } while (!isPVLReady);

                            }


                            objCMData.isDone = objCMControllerService.CMPut(objCMData);
                            if (!pathDetails.parallelMove)
                                objCMControllerService.CheckCMCommandDone(objCMData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            break;
                        case OpcTags.CM_L2_Get_Cmd:
                            objCMData = null;
                            objCMData = ConvertToCMData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            do
                            {
                                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                                /**checking transaction deleted or not****/
                                //objQueueControllerService.CancelIfRequested(pathDetails.queueId);
                                /******/
                                if (!isLiveCmdUpdated)
                                    Thread.Sleep(500);
                            } while (!isLiveCmdUpdated);
                            objCMData.isDone = objCMControllerService.CMGet(objCMData);
                            if (!pathDetails.parallelMove)
                            {
                                objCMControllerService.CheckCMCommandDone(objCMData);
                                if (pathDetails.callUpadate)
                                    objPVLControllerService.TaskAfterGetPalletBundle(pathDetails.pathPkId);
                            }
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
                            UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            break;


                        case OpcTags.PVL_CP_Start:
                            objPVLData = null;
                            objPVLData = ConvertToPVLData(pathDetails);
                            objErrorData = ConvertToErrorData(pathDetails);
                            objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);

                            objPVLData.isDone = objPVLControllerService.PVLMove(objPVLData);
                            if (!pathDetails.parallelMove)
                                objPVLControllerService.CheckPVLCommandDone(objPVLData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objPVLData.machineCode, true);

                            UpdateMachineBlockStatus(pathDetails.unblockMachine);
                            break;

                        default:

                            break;
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Path Id:" + objQueueData.pathPkId + ":--Exception 'ExcecuteCommandsForPMS':: " + ex.Message+":: "+ex.StackTrace);
            }
            finally
            {
            }
        }

        public List<Model.PathDetailsData> GetDynamicPath(int queueId)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Entered 'GetDynamicPath' ");
            List<Model.PathDetailsData> lstDynamicPathDetails = null; // new List<PathDetails>();
            objParkingDaoService=new DB.ParkingDaoImp();

            try
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId,true);
                do
                {
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(queueId);
                    /******/

                    lock (pathSelectionLock)
                    {
                        lstDynamicPathDetails = objParkingDaoService.FindAndGetDynamicPath(queueId);

                    }
                    if (lstDynamicPathDetails == null) 
                        System.Threading.Thread.Sleep(1500);

                } while (lstDynamicPathDetails == null);

            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetDynamicPath':: " + errMsg.Message);
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetDynamicPath':: " + errMsg.Message);
            }
            finally
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, false);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Exitting 'GetDynamicPath'");
            }
            return lstDynamicPathDetails;
        }

        public List<Model.PathDetailsData> GetInitialPath(int queueId)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Entered 'GetInitialPath' ");
            List<Model.PathDetailsData> lstDynamicPathDetails = null; // new List<PathDetails>();
            if (objParkingDaoService==null) objParkingDaoService = new DB.ParkingDaoImp();
            try
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, true);
                do
                {
                    
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(queueId);
                    /******/
                    lock (pathSelectionLock)
                    {
                        lstDynamicPathDetails = objParkingDaoService.FindAndGetInitialPath(queueId);

                    }
                    if (lstDynamicPathDetails == null) 
                        System.Threading.Thread.Sleep(1500);

                } while (lstDynamicPathDetails == null);

            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetInitialPath':: " + errMsg.Message);
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetInitialPath':: " + errMsg.Message);
            }
            finally
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, false);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Exitting 'GetInitialPath' ");
            }
            return lstDynamicPathDetails;
        }
        public List<Model.PathDetailsData> GetVLCDynamicPath(int queueId)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Entered 'GetVLCDynamicPath' ");
            List<Model.PathDetailsData> lstDynamicPathDetails = null; // new List<PathDetails>();
            objParkingDaoService = new DB.ParkingDaoImp();

            try
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, true);
                do
                {
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(queueId);
                    /******/

                    lock (pathSelectionLock)
                    {
                        lstDynamicPathDetails = objParkingDaoService.FindAndGetVLCDynamicPath(queueId);

                    }
                    if (lstDynamicPathDetails == null)
                        System.Threading.Thread.Sleep(1500);

                } while (lstDynamicPathDetails == null);

            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetVLCDynamicPath':: " + errMsg.Message);
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetVLCDynamicPath':: " + errMsg.Message);
            }
            finally
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, false);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Exitting 'GetVLCDynamicPath'");
            }
            return lstDynamicPathDetails;
        }
        public List<Model.PathDetailsData> GetAllocatePath(int queueId)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Entered 'GetAllocatePath' ");
            List<Model.PathDetailsData> lstDynamicPathDetails = null; // new List<PathDetails>();
            objParkingDaoService = new DB.ParkingDaoImp();

            try
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, true);
                do
                {
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(queueId);
                    /******/

                    lock (pathSelectionLock)
                    {
                        lstDynamicPathDetails = objParkingDaoService.FindAndGetAllocationPath(queueId);
                      

                    }
                    if (lstDynamicPathDetails == null)
                        System.Threading.Thread.Sleep(2500);

                } while (lstDynamicPathDetails == null);

            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetAllocatePath':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + ":--Exception 'GetAllocatePath':: " + errMsg.Message);
                if (errMsg is TaskCanceledException)
                    throw new Exception();
            }
            finally
            {
                objParkingDaoService.UpdateSearchingPathStatus(queueId, false);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " : Exitting 'GetAllocatePath'");
            }
            return lstDynamicPathDetails;
        }

        public bool CallResetProcedure()
        {
            if (objParkingDaoService == null) objParkingDaoService = new DB.ParkingDaoImp();
            objParkingDaoService.ResetProcedureCall();
            return true;
        }
       



        public CMData ConvertToCMData(Model.PathDetailsData objPathDetailsData)
        {
            CMData convertCMData = new CMData();
            convertCMData.machineCode = objPathDetailsData.machine;
            convertCMData.cmChannel = objPathDetailsData.channel;
            convertCMData.command = objPathDetailsData.command;
            convertCMData.destFloor = objPathDetailsData.floor;
            convertCMData.destAisle = objPathDetailsData.aisle;
            convertCMData.destRow = objPathDetailsData.row;
            convertCMData.isDone = objPathDetailsData.done;
            convertCMData.queueId = objPathDetailsData.queueId;
            convertCMData.cmdVal = objPathDetailsData.cmd_val_in_number;
            convertCMData.interactMachine = objPathDetailsData.interactMachineCode;
            convertCMData.requestType = objPathDetailsData.requestType;
            if (string.IsNullOrEmpty(convertCMData.interactMachine) || convertCMData.command.Equals(OpcTags.CM_L2_Put_Cmd))
                convertCMData.pivotCMCode = convertCMData.machineCode;  //for auto refresh when clearing path
            else
                convertCMData.pivotCMCode = convertCMData.interactMachine;  //for auto refresh when clearing path
            return convertCMData;
            

        }

        public VLCData ConvertToVLCData(Model.PathDetailsData objPathDetailsData)
        {
            VLCData convertVLCData = new VLCData();
            convertVLCData.machineCode = objPathDetailsData.machine;
            convertVLCData.machineChannel = objPathDetailsData.channel;
            convertVLCData.command = objPathDetailsData.command;
            convertVLCData.destFloor = objPathDetailsData.floor;
            
            convertVLCData.isDone = objPathDetailsData.done;
            convertVLCData.queueId = objPathDetailsData.queueId;

            return convertVLCData;
        }
        public PVLData ConvertToPVLData(Model.PathDetailsData objPathDetailsData)
        {
            PVLData convertPVLData = new PVLData();
            convertPVLData.machineCode = objPathDetailsData.machine;
            convertPVLData.machineChannel = objPathDetailsData.channel;
            convertPVLData.command = objPathDetailsData.command;
            convertPVLData.destFloor = objPathDetailsData.floor;

            convertPVLData.isDone = objPathDetailsData.done;
            convertPVLData.queueId = objPathDetailsData.queueId;

            return convertPVLData;
        }
        public EESData ConvertToEESData(Model.PathDetailsData objPathDetailsData)
        {
            EESData convertEESData = new EESData();
            convertEESData.machineCode = objPathDetailsData.machine;
            convertEESData.machineChannel = objPathDetailsData.channel;
            convertEESData.command = objPathDetailsData.command;
            convertEESData.isDone = objPathDetailsData.done;
            convertEESData.queueId = objPathDetailsData.queueId;
            convertEESData.eesName = objPathDetailsData.machineName;

            return convertEESData;
        }

        public ErrorData ConvertToErrorData(Model.PathDetailsData objPathDetailsData)
        {
            ErrorData convertErrorData = new ErrorData();
            convertErrorData.machine = objPathDetailsData.machine;
            convertErrorData.command = objPathDetailsData.command;
            convertErrorData.floor = objPathDetailsData.floor;
            convertErrorData.aisle = objPathDetailsData.aisle;
            convertErrorData.floor_row = objPathDetailsData.row;
            convertErrorData.done = objPathDetailsData.done;
            convertErrorData.queueId = objPathDetailsData.queueId;
            convertErrorData.seqId = objPathDetailsData.sequencePkId;

            return convertErrorData;
        }
        public bool UpdateMachineBlockStatus(string unblockMachine)
        {
            bool isUnblocked = false;
            if (string.IsNullOrEmpty(unblockMachine))
            {
                return isUnblocked;
            }
            
            if (unblockMachine.Contains("CM"))
            {
                CMControllerService service = new CMControllerImp();
                isUnblocked=service.UpdateMachineBlockStatus(unblockMachine, false);
            }
            else if (unblockMachine.Contains("VLC"))
            {
                VLCControllerService service = new VLCControllerImp();
                isUnblocked = service.UpdateMachineBlockStatus(unblockMachine, false);
            }
            else if (unblockMachine.Contains("EES"))
            {
                EESControllerService service = new EESControllerImp();
                isUnblocked = service.UpdateMachineBlockStatus(unblockMachine, false);
            }
            else if (unblockMachine.Contains("PVL"))
            {
                PVLControllerService service = new PVLControllerImp();
                isUnblocked = service.UpdateMachineBlockStatus(unblockMachine, false);
            }
            else if (unblockMachine.Contains("PST"))
            {
                PSTControllerService service = new PSTControllerImp();
                isUnblocked = service.UpdateMachineBlockStatus(unblockMachine, false);
            }
            return isUnblocked;
        }





        public List<PathDetailsData> GetPathDetails(int queueId, int pathId = 0)
        {
            if (objParkingDaoService==null) objParkingDaoService = new ParkingDaoImp();
            return objParkingDaoService.GetPathDetails(queueId, pathId);
        }
        public bool UpdateLiveCommandOfCM(ErrorData objErrorData){//not using now
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            if (objCMControllerService == null) objCMControllerService = new CMControllerImp();
            bool isValidate=false;
            bool isUpdated = false;
            string clearingMachine=null;
            int clearFlag=0;
            lock (triggerUpdateLock)
            {

                if (!objErrorControllerService.UpdateLiveCommandOfCM(objErrorData))
                {
                    objCMControllerService.GetCMClearRequest(objErrorData.machine, out clearingMachine, out clearFlag);
                    if (clearFlag == 1)
                    {
                        objCMControllerService.UpdateCMClearPermission(objErrorData.machine);
                    }
                }
            }
            return isUpdated;
        }
        public void HomePositionMoveTrigger()
        {
            //get all cms
            if (objCMControllerService == null) 
                objCMControllerService = new CMControllerImp();
            List<CMData> cmList = objCMControllerService.GetCMList();
            bool isCMHealthy = false;
            bool isCMIdle = false;
            if (objErrorControllerService == null) objErrorControllerService=new ErrorControllerImp();
            Dictionary<string,int> machineIdleCount=new Dictionary<string,int>();
            foreach (CMData objCMData in cmList)
            {
                machineIdleCount.Add(objCMData.machineCode,0);
            }

            while (!_shouldStop)
            {

                foreach (CMData objCMData in cmList)
                {
                    try
                    {
                        //check cm healthy
                        isCMHealthy = objCMControllerService.CheckCMHealthy(objCMData);
                        if (!isCMHealthy) continue;

                        int cmAisle = objCMControllerService.GetCMAisle(objCMData);
                        if (objCMData.homeAisle == cmAisle)
                        {
                            Thread.Sleep(200);
                            continue;
                        }

                        //check cm idle for 10 seconds
                        isCMIdle = objCMControllerService.IsCMInIdeal(objCMData);

                        if (!isCMIdle)
                        {
                            machineIdleCount[objCMData.machineCode] = 0;
                            continue;
                        }
                        else
                        {
                            machineIdleCount[objCMData.machineCode]++;
                        }
                        if (machineIdleCount[objCMData.machineCode] > 3)
                        {
                            machineIdleCount[objCMData.machineCode] = 0;


                            //move machine
                            CMData refCMData = new CMData();
                            refCMData = BasicConfig.Clone<CMData>(objCMData);


                            refCMData.destFloor = objCMData.floor;
                            refCMData.command = OpcTags.CM_L2_Move_Cmd;
                            refCMData.destAisle = objCMData.homeAisle;
                            refCMData.destRow = 2;
                            refCMData.isHomeMove = true;

                            bool hasMoveSuccess = false;

                           
                            
                            Task.Factory.StartNew(() => objCMControllerService.CMHomeMove(refCMData));
                            

                           
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                Thread.Sleep(100);
            }

        }
        
        public void CheckCommandDoneInParallel(PathDetailsData objPathDetailsData, CMData objCMData)
        {
            objCMControllerService.CheckCMCommandDone(objCMData);
            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objCMData.machineCode, true);
            UpdateMachineBlockStatus(objPathDetailsData.unblockMachine);

        }
        public bool FindOptimizedPath(int queueId)
        {
            if (objParkingDaoService == null) objParkingDaoService = new ParkingDaoImp();
            return objParkingDaoService.FindOptimizedPath(queueId);
        }
        public bool GetIterationStatus(int queueId)
        {
            if (objParkingDaoService == null) objParkingDaoService = new ParkingDaoImp();
            return objParkingDaoService.GetIterationStatus(queueId);
        }
     
        public void CallResumeProcedure()
        {
            if (objParkingDaoService == null) objParkingDaoService = new ParkingDaoImp();
            objParkingDaoService.ResumeProcedureCall();
        }
        public void VLCHomePositionMoveTrigger()
        {
            //get all vlcs
            if (objVLCControllerService == null)
                objVLCControllerService = new VLCControllerImp();
            if (objVLCDaoService == null)
                objVLCDaoService = new VLCDaoImp();
            List<VLCData> vlcList = objVLCDaoService.GetVLCList();
            bool isVLCHealthy = false;
            bool isVLCIdle = false;
            int lcmFloor = 1;
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
           

            while (!_shouldStop)
            {

                foreach (VLCData objVLCData in vlcList)
                {
                    try
                    {
                        //check cm healthy
                        isVLCHealthy = objVLCControllerService.CheckVLCHealthy(objVLCData);
                        if (!isVLCHealthy)
                            continue;

                        int vlcAisle = objVLCControllerService.VLCAtFloor(objVLCData);
                        if (vlcAisle == lcmFloor)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        if (objVLCDaoService.GetVLCMode(objVLCData.machineCode) == GlobalValues.vlcMode.ENTRY)
                        {
                            //move machine
                            VLCData refVLCData = new VLCData();
                            refVLCData = BasicConfig.Clone<VLCData>(objVLCData);


                            refVLCData.destFloor = lcmFloor;
                            refVLCData.command = OpcTags.VLC_CP_Start;

                            Task.Factory.StartNew(() => objVLCControllerService.VLCHomeMove(refVLCData));
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                Thread.Sleep(2000);
            }
            

        }
        //29OCt18
        public bool PreCheckQueueStatusBeforeFindingPath(QueueData objQueueData)
        {
            if (objQueueDaoService.GetQueueAvailableStatus(objQueueData.queuePkId) == 0)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "PreCheckQueueStatusBeforeFindingPath(): Queue Id:" + objQueueData.queuePkId + ":--Not available");
                return false;
            }
            if (objQueueDaoService.GetQueueStatus(objQueueData.queuePkId) == 1)   //not consider finished transactions
            {
                objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, false);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "PreCheckQueueStatusBeforeFindingPath(): Queue Id:" + objQueueData.queuePkId + ":--Finished");
                return false;

            }

            if (objQueueDaoService.GetCancelledStatus(objQueueData.queuePkId) != 0) //remove deleted transactions
            {
                objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, false);
                objQueueDaoService.UpdateAbortedStatus(objQueueData.queuePkId);
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "PreCheckQueueStatusBeforeFindingPath(): Queue Id:" + objQueueData.queuePkId + ":--Cancelled");
                return false;
            }



            //hold & resume logic block
            if (objQueueDaoService.GetHoldRequestFlagStatus(objQueueData.queuePkId))
            {
                objQueueDaoService.SetHoldFlagStatus(objQueueData.queuePkId, true);
                RequestQueue.Enqueue(objQueueData);                 //insert into queue again if transaction paused
                return false;
            }
            else if (objQueueDaoService.GetHoldFlagStatus(objQueueData.queuePkId))
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "PreCheckQueueStatusBeforeFindingPath(): Queue Id:" + objQueueData.queuePkId + ":--transaction Resumed");
                objQueueDaoService.SetHoldFlagStatus(objQueueData.queuePkId, false);
            }
            return true;
        }
        public void ProcessQueueDefault()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueueDefault()");

            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (DefaultSeachQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!DefaultSeachQueue.TryDequeue(out objQueueData))
                            continue;
                        FindPath(objQueueData);
                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueueDefault':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);

        }
        public void ProcessQueueForEES()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueueForEES()");

            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (EESSeachQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!EESSeachQueue.TryDequeue(out objQueueData))
                            continue;
                        FindPath(objQueueData);
                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueueForEES':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);

        }
        public void ProcessQueueForLCM()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueueWrtLCM()");

            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (LCMSeachQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!LCMSeachQueue.TryDequeue(out objQueueData))
                            continue;
                        FindPath(objQueueData);
                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueueWrtLCM':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);

        }
        public void ProcessQueueForUCM()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueueWrtUCM()");

            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (UCMSeachQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!UCMSeachQueue.TryDequeue(out objQueueData))
                            continue;
                        FindPath(objQueueData);
                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueueWrtUCM':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);

        }

        public void ProcessQueueForVLC()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entered ProcessQueueWrtVLC()");

            do
            {
                Thread.Sleep(1000);
                try
                {
                    while (VLCSeachQueue.Count != 0)
                    {
                        Thread.Sleep(100);
                        QueueData objQueueData = null;
                        if (!VLCSeachQueue.TryDequeue(out objQueueData))
                            continue;
                        FindPath(objQueueData);
                    }

                }
                catch (Exception outerEx)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Outer Exception 'ProcessQueueWrtVLC':: " + outerEx.Message + ":: " + outerEx.StackTrace);
                }
            } while (!_shouldStop);

        }
        public void FindPath(QueueData objQueueData)
        {
            if (objQueueDaoService == null)
                objQueueDaoService = new QueueDaoImp();
            if (objParkingDaoService == null)
                objParkingDaoService = new DB.ParkingDaoImp();

            try
            {


                objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, true);    //updating path search status true; using for priorty in DB

                List<Model.PathDetailsData> lstPathDetails = null;
                lstPathDetails = objParkingDaoService.FindAndGetAllocationPath(objQueueData.queuePkId);

                if (lstPathDetails != null)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "FindPath(): Queue Id:" + objQueueData.queuePkId + ":--Get Path");
                    objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, false);
                    CancellationTokenSource tokenSource = new CancellationTokenSource();

                    if (GlobalValues.threadsDictionary.ContainsKey(objQueueData.queuePkId)) //remove thread in thread dictionary if it contains already
                        GlobalValues.threadsDictionary.Remove(objQueueData.queuePkId);

                    GlobalValues.threadsDictionary.Add(objQueueData.queuePkId, tokenSource);
                    Task.Factory.StartNew(() => ExcecuteCommands(objQueueData), tokenSource.Token);
                }
                else
                {
                    RequestQueue.Enqueue(objQueueData);                 //insert into queue again if path is not available
                }

            }

            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objQueueData.queuePkId + ":--Exception 'FindPath':: " + ex.Message + ":: " + ex.StackTrace);
                objParkingDaoService.UpdateSearchingPathStatus(objQueueData.queuePkId, false);
            }
            finally
            {

            }



        }
    }
     
}
