using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPC;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.DB;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;
using ARCPMS_ENGINE.src.mrs.Config;
using System.Threading.Tasks;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller
{
    class CMControllerImp : CommonServicesForMachines,CMControllerService
    {
        OPCDA.NET.RefreshGroup uGrp;
        int DAUpdateRate = 1;
        Thread updateDataFromOpcListener = null;
        CMDaoService objCMDaoService=null;
        VLCControllerService objVLCControllerService = null;
        EESControllerService objEESControllerService = null;

        ErrorControllerService objErrorControllerService = null;
        QueueControllerService objQueueControllerService = null;
        ErrorDaoService objErrorDaoService = null;

        private volatile bool _shouldStop;
        int autoRefreshLmt = 10;

       


        public List<CMData> GetCMList()
        {
            
            if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
            return objCMDaoService.GetCMList();
        }
        public bool UpdateMachineValues()
        {
            if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
            List<CMData> cmList;
            bool result;
            int dataValue;
            bool dataValueInBool;



            try
            {
                cmList = objCMDaoService.GetCMList();
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    foreach (CMData objCMData in cmList)
                    {

                        if (opcd.IsMachineHealthy(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Min_Window_Limit) == true)
                        {
                            dataValue = opcd.ReadTag<Int16>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit);
                            if (dataValue > 0) UpdateCMIntData(objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit, dataValue);

                            dataValue = opcd.ReadTag<Int16>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit);
                            if (dataValue > 0) UpdateCMIntData(objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit, dataValue);

                            dataValue = opcd.ReadTag<Int16>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);
                            if (dataValue > 0) UpdateCMIntData(objCMData.machineCode, OpcTags.CM_Position_for_L2, dataValue);

                            dataValue = opcd.ReadTag<Int16>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row);
                            if (dataValue > 0) UpdateCMIntData(objCMData.machineCode, OpcTags.CM_L2_Destination_Row, dataValue);

                            dataValueInBool = false;
                            dataValueInBool = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_AUTO_READY);
                            UpdateCMBoolData(objCMData.machineCode, OpcTags.CM_L2_AUTO_READY, dataValueInBool);

                          

                            dataValueInBool = false;
                            dataValueInBool = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.remCode, OpcTags.CM_Pallet_Present_on_REM);
                            UpdateCMBoolData(objCMData.machineCode, OpcTags.CM_Pallet_Present_on_REM, dataValueInBool);


                        }

                    }
                }
                result = true;
            }
            catch (Exception errMsg)
            {
                result = false;
                Console.WriteLine(errMsg.Message);
            }
            return result;
        }


       

        public bool CMMove(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'CMMove' " );
            bool isCMHealthy = false;
            bool success = false;
            bool isPathClear = false;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            bool srcDestSame = false;
            int autoRefreshCnt = 0;
            
            
            do
            {
               
                try
                {
                    
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    TriggerData objTriggerData = NeedToShowTrigger(objCMData);
                    if (objTriggerData.TriggerEnabled)
                    {
                        if (objCMData.isHomeMove)
                            break;
                        else
                            objErrorControllerService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    }
                     
                    isCMHealthy = CheckCMHealthy(objCMData);

                    if (!isCMHealthy)
                    {
                        if (objCMData.isHomeMove)
                            break;
                        Thread.Sleep(200);
                       
                        continue;
                    }

                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {
                        objCMData.positionAisle = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);

                        if (objCMData.positionAisle != objCMData.destAisle)
                        {
                            isPathClear = ClearPath(objCMData);
                            if (isPathClear)
                            {
                                int destAisle = 0;
                                int destRow = 0;
                                do
                                {
                                    opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle, objCMData.destAisle);
                                    destAisle = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle);
                                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destAisle=" + destAisle + "--Inside 'CMMove' ");
                                }
                                while (destAisle != objCMData.destAisle);

                                do
                                {
                                    opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row, objCMData.destRow);
                                    destRow = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row);
                                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destRow=" + destRow + "--Inside 'CMMove' ");

                                }
                                while (destRow != objCMData.destRow);
                                
                                success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                            }
                            else
                            {
                                if (ParkConfig.IsAutoRefreshActive(objCMData.requestType))
                                {
                                    autoRefreshCnt++;
                                    if (autoRefreshCnt > autoRefreshLmt)
                                    {

                                        Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                                + ": CM=" + objCMData.machineCode + " auto refresh in 'CMMove' ");
                                        if (string.IsNullOrEmpty(objCMData.interactMachine))
                                            objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.pivotCMCode, 3);   //cm move before put command
                                        else
                                            objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.interactMachine, 3);  //cm move before get command
                                     

                                        //Thread.Sleep(500);
                                        objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);
                                        throw new OperationCanceledException();
                                    }
                                }
                               
                                
                            }
                        }
                        else
                        {
                            success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                            success = true;
                        }


                    }

                }
                catch (OperationCanceledException errMsg)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CMMove':: " + errMsg.Message);
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {

                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'CMMove':: " + ex.Message);
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    if (ex is TaskCanceledException)
                        throw new Exception();

                    success = false;
                }
              
                
            } while (!success && !objCMData.isHomeMove);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exiting 'CMMove' ");
            return success;
        }
        public bool CMHomeMove(Model.CMData objCMData)
        {
            //Logger.WriteLogger(GlobalValues.PARKING_LOG, "CM:" + objCMData.machineCode + ":--Entered 'CMHomeMove' ");
            bool isCMHealthy = false;
            bool success = false;
            bool isPathClear = false;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            ErrorData objErrorData = new ErrorData();
            bool isLiveCmdUpdated = false;


            try
            {
                
                objErrorData.machine = objCMData.machineCode;
                objErrorData.command = objCMData.command;
                objErrorData.floor = objCMData.destFloor;
                objErrorData.aisle = objCMData.destAisle;
                objErrorData.floor_row = objCMData.destRow;

                objCMData.isBlocked = false;
                objCMData.isBlocked = UpdateMachineBlockStatusForHome(objCMData.machineCode, true);
                if (!objCMData.isBlocked)
                    return false;
               
                isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                if (!isLiveCmdUpdated) return false;

                isCMHealthy = CheckCMHealthy(objCMData);
                if (!isCMHealthy) return false;


                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objCMData.positionAisle = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);
                    if (objCMData.positionAisle != objCMData.destAisle)
                    {
                        isPathClear = ClearPath(objCMData);
                        if (isPathClear)
                        {
                            

                            int destAisle = 0;
                            int destRow = 0;
                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle, objCMData.destAisle);
                                destAisle = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle); ;
                            }
                            while (destAisle != objCMData.destAisle);

                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row, objCMData.destRow);
                                destRow = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row); ;
                            }
                            while (destRow != objCMData.destRow);
                            success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, false);
                            success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                            CheckCMCommandDone(objCMData);
                        }
                    }
                    else
                    {
                        //success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                        success = true;
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "CM:" + objCMData.machineCode + ":--Exception 'CMHomeMove':: " + ex.Message);


                success = false;
            }
            finally
            {
                if(objCMData.isBlocked)
                    UpdateMachineBlockStatusForHome(objCMData.machineCode, false);
                if(isLiveCmdUpdated)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);

                //Logger.WriteLogger(GlobalValues.PARKING_LOG, "CM:" + objCMData.machineCode + ":--Exiting 'CMHomeMove' ");
            }

            return success;
        }
        public bool CMGet(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + "--Entered 'CMGet' ");
            bool isCMHealthy = false;
            bool success = false;
            bool isPathClear = false;
            bool isRemAtHome = false;
            ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus enumPalletStatus = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            int autoRefreshCnt = 0;

            TriggerData objTriggerData = null;
            do
            {

              

                
                isRemAtHome = true;

                try
                {

                   
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    objTriggerData = NeedToShowTrigger(objCMData);
                    if (objTriggerData.TriggerEnabled)
                    {
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    }
                     

                    
                   
                    //isCMHealthy = CheckCMHealthy(objCMData);

                    //if (!isCMHealthy)
                    //{
                    //    Thread.Sleep(200);
                    //    continue;
                    //}


                    enumPalletStatus = GetPalletOnCMStatus(objCMData);
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": pallet status=" + enumPalletStatus + "--Inside 'CMGet' ");

                    if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.present)
                    {
                        objTriggerData = new TriggerData();
                        objTriggerData.TriggerEnabled = true;
                        objTriggerData.MachineCode = objCMData.machineCode;
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    }
                    else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    


                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {
                        objCMData.positionAisle = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);

                        if (objCMData.positionAisle != objCMData.destAisle)
                        {
                            isPathClear = ClearPath(objCMData);
                        }
                        else
                            isPathClear = true;

                        if (isPathClear)
                        {
                            int destAisle = 0;
                            int destRow = 0;
                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle, objCMData.destAisle);
                                destAisle = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle);
                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destAisle=" + destAisle + "--Inside 'CMGet' ");
                            }
                            while (destAisle != objCMData.destAisle);

                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row, objCMData.destRow);
                                destRow = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row);
                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destRow=" + destRow + "--Inside 'CMGet' ");
                            }
                            while (destRow != objCMData.destRow);


                        //    enumPalletStatus = GetPalletOnCMStatus(objCMData);
                           // if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notPresent)
                           // {
                                opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                                success = ConfirmGetPutAccepted(objCMData);
                          //  }
                           // else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.present)
                                //success = true;
                              //  needToShowTrigger = true;
                        }
                        else
                        {
                            if (ParkConfig.IsAutoRefreshActive(objCMData.requestType))
                            {
                                autoRefreshCnt++;
                                if (autoRefreshCnt > autoRefreshLmt)
                                {

                                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                            + ": CM=" + objCMData.machineCode + " auto refresh in 'CMGet' ");
                                    objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.interactMachine, 3);
                                    autoRefreshCnt = 0;
                                    //Thread.Sleep(500);
                                    objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);
                                    throw new OperationCanceledException();
                                }
                            }
                            

                        }
              

                    }

                }
                catch (OperationCanceledException errMsg)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CMGet':: " + errMsg.Message);
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + "--Exception 'CMGet':: " + ex.Message);
                 
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    if (ex is TaskCanceledException)
                        throw new Exception();
                    success = false;
                }
            } while (!success);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ":--Exitting 'CMGet' ");
            return success;
        }

        public bool CMPut(Model.CMData objCMData)
        {

            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":CM=" + objCMData.machineCode + "--Entered 'CMPut' ");
            
            bool isCMHealthy = false;
            bool success = false;
            bool isPathClear = false;
            bool isPalletOnREM = false;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus enumPalletStatus = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid;
            int autoRefreshCnt = 0;

            TriggerData objTriggerData = null;
            do
            {

              
               
                try
                {
                    
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    objTriggerData = NeedToShowTrigger(objCMData);
                    if (objTriggerData.TriggerEnabled)
                    {
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    } 
                    //isCMHealthy = CheckCMHealthy(objCMData);

                    //if (!isCMHealthy)
                    //{
                    //    Thread.Sleep(200);
                    //    continue;
                    //}



                    enumPalletStatus = GetPalletOnCMStatus(objCMData);
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": pallet status=" + enumPalletStatus + "--Inside 'CMPut' ");

                    if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notPresent)
                    {
                        objTriggerData = new TriggerData();
                        objTriggerData.TriggerEnabled = true;
                        objTriggerData.MachineCode = objCMData.machineCode;
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    }
                    else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid)
                    {
                        Thread.Sleep(200);
                        continue;
                    }
                    




                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {
                        objCMData.positionAisle = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);

                        if (objCMData.positionAisle != objCMData.destAisle)
                        {
                            isPathClear = ClearPath(objCMData);
                        }
                        else
                            isPathClear = true;
                        if (isPathClear)
                        {
                            int destAisle = 0;
                            int destRow = 0;
                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle, objCMData.destAisle);
                                destAisle = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Aisle);
                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destAisle=" + destAisle + "--Inside 'CMPut' ");
                            }
                            while (destAisle != objCMData.destAisle);

                            do
                            {
                                opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row, objCMData.destRow);
                                destRow = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Destination_Row);
                                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": destRow=" + destRow + "--Inside 'CMPut' ");
                            }
                            while (destRow != objCMData.destRow);

                           // enumPalletStatus = GetPalletOnCMStatus(objCMData);
                          //  Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": pallet status=" + enumPalletStatus + "--Inside 'CMPut' ");


                           // if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.present)
                           // {
                                opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, objCMData.command, true);
                                success = ConfirmGetPutAccepted(objCMData);
                         //   }
                          //  else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notPresent)
                                //success = true;
                            //    needToShowTrigger = true;

                        }
                        else
                        {
                            if (ParkConfig.IsAutoRefreshActive(objCMData.requestType))
                            {
                                autoRefreshCnt++;
                                if (autoRefreshCnt > autoRefreshLmt)
                                {

                                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId
                                            + ": CM=" + objCMData.machineCode + " auto refresh in 'CMPut' ");
                                    objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.machineCode, 3);
                                    autoRefreshCnt = 0;
                                   // Thread.Sleep(500);
                                    objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);
                                    throw new OperationCanceledException();
                                }
                            }
                            

                        }
                    }

                }
                catch (OperationCanceledException errMsg)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CMPut':: " + errMsg.Message);
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":CM=" + objCMData.machineCode + "--Exception 'CMPut':: " + ex.Message);

                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    if (ex is TaskCanceledException)
                        throw new Exception();
                    success = false;
                }
            } while (!success);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":CM=" + objCMData.machineCode + "--Exitting 'CMPut' ");
            return success;
        }
        public bool ConfirmGetPutAccepted(CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'ConfirmGetPutAccepted' ");
            bool isConfirmed = false;
            Int16 timeCounter = 0;
            int cmState = 0;
            do
            {
                Thread.Sleep(500);
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    cmState = opcd.ReadTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Progress_State);
                    isConfirmed = cmState == 108 ||cmState == 107 || cmState == 110;

                }
                timeCounter++;

            }
            while (!isConfirmed && timeCounter < 5);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ",isConfirmed=" + isConfirmed + ":--Exitting 'ConfirmGetPutAccepted' ");
            return isConfirmed;
        }
        public bool CMRotate(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'CMRotate' ");
            if (objQueueControllerService == null) 
                objQueueControllerService = new QueueControllerImp();
            if (objCMDaoService == null)
                objCMDaoService = new CMDaoImp();
            bool isCMHealthy = false;
            bool success = false;
            bool isPathClear = false;
            int checkCount=0;
            int checkCountLimit=0;
            int autoRefreshCnt = 0;
           
            
            if (objCMData.cmdVal == 0)
                checkCountLimit = 1;
            else if (objCMData.cmdVal == 1)
                checkCountLimit = 4;
            else if (objCMData.cmdVal == 2)
                checkCountLimit = 200;
            do
            {
                /**checking transaction deleted or not****/
                objQueueControllerService.CancelIfRequested(objCMData.queueId);
                /******/
                try
                {
                    if (!objCMDaoService.GetCMRotationEnabledStatus(objCMData.machineCode))
                        break;
                    isCMHealthy = CheckCMHealthy(objCMData) ;
                    isCMHealthy = isCMHealthy && IsCMInIdeal(objCMData);

                    if (!isCMHealthy)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {

                            isPathClear = ClearForRotaion(objCMData);
                            if (isPathClear)
                            {
                                if (opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_at_0_Degrees))
                                    success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_Rotate_to_180_from_L2, true);
                                else if (opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_at_180_Degrees))
                                    success = opcd.WriteTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_Rotate_to_0_from_L2, true);
                                success = ConfirmRotateCmdAccepted(objCMData);
                            }
                            else
                            {
                                autoRefreshCnt++;
                                Thread.Sleep(500);
                                if (autoRefreshCnt > autoRefreshLmt)
                                {
                                    objQueueControllerService.SetReallocateData(objCMData.queueId, objCMData.machineCode, 3);
                                    objQueueControllerService.UpdateAbortedStatus(objCMData.queueId);
                                    throw new OperationCanceledException();
                                }
                                autoRefreshCnt = 0;

                            }
                       
                    }

                }
                catch (OperationCanceledException errMsg)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CMRotate':: " + errMsg.Message);
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'CMRotate':: " + ex.Message);
                    success = false;

                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/
                    if (ex is TaskCanceledException)
                        throw new Exception();
                }
                checkCount++;
                //Thread.Sleep(500);
            } while (!success && checkCount<checkCountLimit);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exitting 'CMRotate' ");
            return success;
        }
        public bool ConfirmRotateCmdAccepted(CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'ConfirmRotateCmdAccepted' ");
            bool isConfirmed = false;
            Int16 timeCounter = 0;
            do
            {
                Thread.Sleep(500);
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    isConfirmed = !opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_at_0_Degrees);
                    isConfirmed = isConfirmed && !opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_at_180_Degrees);

                }
                timeCounter++;

            }
            while (!isConfirmed && timeCounter < 5);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ",isConfirmed=" + isConfirmed + ":--Exitting 'ConfirmRotateCmdAccepted' ");
            return isConfirmed;
        }
        public bool ClearPath(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'ClearPath' ");
            
            OpcOperationsService opcd =null;
            Model.CMData colCMData;
            

            bool hasNoObstacle = false;
           
            bool col_machine_rot_status = true;
            int lcmFloor = 1;
            int rotGap = 1;
           
            Int32 col_ref_dest_aisle = 0;
                    
           // bool isCollapseMachineAvailable = true;
            if (objQueueControllerService == null) 
                objQueueControllerService = new QueueControllerImp();
            if (objCMDaoService == null) 
                objCMDaoService = new CMDaoImp();
            if (objErrorControllerService == null)
                objErrorControllerService = new ErrorControllerImp();
            bool success = false;
           

            try
            {
           
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
              
               
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objCMData.queueId);
                    /******/



                    do
                    {
                        objCMData.positionAisle = GetCMAisle(objCMData);
                        if (objCMData.positionAisle == 0)
                            Thread.Sleep(200);

                    }
                    while (objCMData.positionAisle == 0);

                    objCMDaoService.UpdateIntValueUsingMachineCode(objCMData.machineCode, "POSITION_AISLE", objCMData.positionAisle);

                    //Get collapse machine,channel
                    colCMData = new CMData();
                   


                    colCMData = objCMDaoService.NeedToPushNearestCM(objCMData);   //for getting moving side CM 
                    if (objCMData.isWait)
                        return false;
                    //check west or east machine is switch off
                    if (objCMData.moveSide == -1)
                    {
                        if (opcd.ReadTag<bool>(objCMData.cmChannel,objCMData.machineCode,OpcTags.CM_WestCMOff) )
                            objCMData.CMPresentOnSide = false;
                    }
                    else if (objCMData.moveSide == 1)
                    {
                        if (opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_EastCMOff))
                            objCMData.CMPresentOnSide = false;
                    }


                    //READ DESTINATION AISLE FROM OPC SERVER TO COLMACHINE_DEST_AISLE
                    if (objCMData.CMPresentOnSide)
                    {
                        int checkCount = 0;
                        do
                        {
                            colCMData.positionAisle = GetCMAisle(colCMData); //reading current position of nearest machine

                            if (colCMData.positionAisle == 0)
                            {
                                if (checkCount > 10)
                                    return false;
                                checkCount++;

                            }
                            else
                                break;

                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objCMData.queueId);
                            /******/
                            Thread.Sleep(200);
                        }
                        while (true);
                        objCMDaoService.UpdateIntValueUsingMachineCode(colCMData.machineCode, "POSITION_AISLE", colCMData.positionAisle);
                       


                        //Moving all other CMs which dont have any work from the path
                        if (objCMData.needToPush)
                            hasNoObstacle = ClearNearestCM(objCMData, colCMData);
                        else
                            hasNoObstacle = true;
                       
                     

                      
                        //checking colmachine is rotating: waiting for tags
                        if ((objCMData.destFloor == lcmFloor || objCMData.destFloor == 0) && hasNoObstacle )
                        { 
                               col_machine_rot_status = false;
                               col_machine_rot_status=ConfirmCMRotationStatus(colCMData);

                               if (col_machine_rot_status == true && (Math.Abs(colCMData.positionAisle - objCMData.destAisle) <= rotGap || colCMData.positionAisle == 0))
                                   hasNoObstacle = false;

                               // rotate_check_count++;
                                                    
                           
                        }
                    }//(end) isColMachineAvailable.
                    else
                    {
                        //if cm is switched off and it is inside the path
                        if (objCMData.moveSide==-1)
                        {
                            if (objCMDaoService.GetCMAisleFromDB(colCMData.machineCode) >= colCMData.destAisle)   
                                hasNoObstacle = false;
                        }
                        else if (objCMData.moveSide == 1)
                        {
                            if (objCMDaoService.GetCMAisleFromDB(colCMData.machineCode) <= colCMData.destAisle)
                                hasNoObstacle = false;
                        }
                        //
                        hasNoObstacle = true;
                    }


                    //if this machine is not ideal then return false.
                    if (hasNoObstacle && !IsCMInIdeal(objCMData)) 
                        hasNoObstacle= false;

                    return hasNoObstacle;
                    //objCMData.virtualAisleMin = opcd.ReadTag<Int32>(objCMData.cmChannel,objCMData.machineCode,OpcTags.CM_L2_Min_Window_Limit);
                    //objCMData.virtualAisleMax = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit);
                    //if (objCMData.destAisle >= objCMData.virtualAisleMin && objCMData.destAisle <= objCMData.virtualAisleMax
                    //                                && objCMData.virtualAisleMin != 0 && objCMData.virtualAisleMax != 0 && hasNoObstacle)
                    //{

                    //    return true;
                    //}
                 

                    ////_____________________SET WINDOW________________________________________________________
                    //if (objCMData.moveSide != 0 && hasNoObstacle && objCMData.CMPresentOnSide)
                    //{

                    //    if (objCMData.moveSide == -1)
                    //    {

                    //        colCMData.virtualAisleMin = opcd.ReadTag<Int32>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit);
                    //        objCMData.virtualAisleMax = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit);
                    //        if (colCMData.virtualAisleMin > Convert.ToInt16(objCMData.destAisle - 1)) return false;
                    //        if (objCMData.virtualAisleMax < Convert.ToInt16(objCMData.destAisle)) return false;

                    //        //Confirm opc server has accepted destination aisle value. 
                    //        bool destination = false;

                    //        do
                    //        {
                    //            opcd.WriteTag<int>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit, objCMData.destAisle - 1);

                    //            destination =
                    //                opcd.ReadTag<Int32>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit) == objCMData.destAisle - 1;
                    //        } while (destination == false);
                    //        Thread.Sleep(500);

                    //        opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit, objCMData.destAisle);
                    //    }
                    //    else
                    //    {
                    //        colCMData.virtualAisleMax = opcd.ReadTag<Int32>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit);
                    //        objCMData.virtualAisleMin = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit);
                    //        if (colCMData.virtualAisleMax < Convert.ToInt16(objCMData.destAisle + 1)) return false;
                    //        if (objCMData.virtualAisleMin > Convert.ToInt16(objCMData.destAisle)) return false;


                    //        //Confirm opc server has accepted destination aisle value. 
                    //        bool destination = false;

                    //        do
                    //        {

                    //            opcd.WriteTag<int>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit, objCMData.destAisle + 1);

                    //            destination =
                    //                opcd.ReadTag<Int32>(colCMData.cmChannel, colCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit) == objCMData.destAisle + 1;
                    //        } while (destination == false);
                    //        Thread.Sleep(500);

                    //        opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit, objCMData.destAisle);

                    //    }

                    //}

                    //if (!objCMData.CMPresentOnSide)
                    //{
                    //    if (objCMData.moveSide == -1)
                    //    {
                    //        opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit, objCMData.destAisle);

                    //    }
                    //    else if (objCMData.moveSide == 1)
                    //    {
                    //        opcd.WriteTag<int>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit, objCMData.destAisle);

                    //    }
                    //}
                    ////_________________________________END SET WINDOW________________________________________

                    //objCMData.virtualAisleMin = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Min_Window_Limit);
                    //objCMData.virtualAisleMax = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Max_Window_Limit);
                    //if (objCMData.destAisle >= objCMData.virtualAisleMin && objCMData.destAisle <= objCMData.virtualAisleMax
                    //                                && objCMData.virtualAisleMin != 0 && objCMData.virtualAisleMax != 0 && hasNoObstacle)
                    //{

                    //    return true;
                    //}
               
                 
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'ClearPath':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'ClearPath':: " + ex.Message);
                if (ex is TaskCanceledException)
                    throw new Exception();
                return false;
            }
            finally
            {
                if (opcd != null) opcd.Dispose();
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ", hasNoObstacle="+hasNoObstacle +":--Exitting 'ClearPath' ");
            }
            return success;

        }

        public bool ClearNearestCM(CMData objCMData, CMData colCMData)
        {

            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entering 'ClearNearestCM' ");
            bool colapseMachineMoving = true;
            bool colapseMachineBlcoked = true;
            OpcOperationsService opcd = null;
            if (objErrorControllerService == null) new ErrorControllerImp();
            ErrorData objErrorData = new ErrorData();
            bool isColCMInPath = false;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
            bool success=false;
            int checkCount=0;
            bool hasCommun=false;
            int checkCountLimit = 5;
            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                objCMDaoService = new CMDaoImp();

                checkCount = 0;
                do
                {
                    objCMData.positionAisle = GetCMAisle(objCMData);
                    colCMData.positionAisle = GetCMAisle(colCMData);
                    hasCommun = colCMData.positionAisle != 0 && objCMData.positionAisle != 0;
                    if (!hasCommun)
                    {
                        Thread.Sleep(700);
                        if (checkCount > checkCountLimit)
                            return false;
                    }
                    checkCount++;
                } while (!hasCommun);

                //colapseMachineMoving = !IsCMInIdeal(colCMData);
                colapseMachineBlcoked = objCMDaoService.IsCMBlockedInDB(colCMData.machineCode);//check collapse machine is already selected
               

                if (objCMData.needToPush) //if collapse machine is inside the path
                {

                    colCMData.isClearing = true;
                    if (string.IsNullOrEmpty(objCMData.pivotCMCode))
                    {
                        colCMData.pivotCMCode = objCMData.machineCode;
                    }
                    else
                    {
                        colCMData.pivotCMCode = objCMData.pivotCMCode;
                    }
                    // check collapse cm is blocked or not 
                    if (!colapseMachineBlcoked)
                    {

                        colCMData.queueId = objCMData.queueId;
                        colCMData.isHomeMove = objCMData.isHomeMove; //for normal transaction it is manadatory to push the nearest machine from the path
                                                                     // but for home move or rotation return if it cant able to move
                        colCMData.command = OpcTags.CM_L2_Move_Cmd;
                        
                        colCMData.isBlocked = false;
                        colCMData.isBlocked = objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, true);
                        if (!colCMData.isBlocked) // there is a chance to take the same machine by path selection procedure
                        {
                          
                            return false;
                        }

                        bool hasMoveSuccess = false;
                        objErrorData.machine = colCMData.machineCode;
                        objErrorData.command = colCMData.command;
                        objErrorData.floor = colCMData.destFloor;
                        objErrorData.aisle = colCMData.destAisle;
                        objErrorData.floor_row = colCMData.destRow;
                        bool isLiveCmdUpdated = false;
                        checkCount = 0;
                        do
                        {
                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objCMData.queueId);
                            /******/
                            isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                            if (!isLiveCmdUpdated)
                            {
                                Thread.Sleep(200);
                                checkCount++;
                            }
                        } while (!isLiveCmdUpdated && !objCMData.isHomeMove && checkCount < checkCountLimit);
                        if (isLiveCmdUpdated)
                        {
                            hasMoveSuccess = CMMove(colCMData);

                            if (hasMoveSuccess )
                            {
                                CheckCMCommandDone(colCMData);
                            }
                            
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                        }
                        if (colCMData.isBlocked)
                        {
                            colCMData.isBlocked = false;
                            objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, colCMData.isBlocked);
                        }
                        success = isLiveCmdUpdated && hasMoveSuccess;
                     
                    }
                    else
                    {
                        //if collapse CM is blocked, then it has cross selection
                        colCMData.queueId = objCMData.queueId;
                        colCMData.isHomeMove = objCMData.isHomeMove;
                        colCMData.command = OpcTags.CM_L2_Move_Cmd;
                       
                        bool hasMoveSuccess = false;
                        objErrorData.machine = colCMData.machineCode;
                        objErrorData.command = colCMData.command;
                        objErrorData.floor = colCMData.destFloor;
                        objErrorData.aisle = colCMData.destAisle;
                        objErrorData.floor_row = colCMData.destRow;
                        bool isLiveCmdUpdated = false;
                        checkCount = 0;
                        do
                        {
                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objCMData.queueId);
                            /******/
                            isLiveCmdUpdated = objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                            if (!isLiveCmdUpdated)
                            {
                                Thread.Sleep(200);
                                checkCount++;
                            }
                        } while (!isLiveCmdUpdated && !objCMData.isHomeMove && checkCount < checkCountLimit);

                        if (isLiveCmdUpdated)
                        {
                            hasMoveSuccess = CMMove(colCMData);
                            hasMoveSuccess = hasMoveSuccess && CheckCMCommandDone(colCMData);
                            objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                        }
                        success = isLiveCmdUpdated && hasMoveSuccess;

                    }
                }
               // success=success && colCMData.positionAisle != 0 && objCMData.positionAisle != 0;
                  
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'ClearNearestCM':: " + errMsg.Message);
                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                //colCMData.isDone = 1;
                if (colCMData.isBlocked)
                    objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, false);
                success = false;
                throw new OperationCanceledException();
            }
            catch (Exception err)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'ClearNearestCM':: " + err.Message);
                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                //colCMData.isDone = 1;
                if (colCMData.isBlocked)
                    objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, false);
                if (err is TaskCanceledException)
                    throw new TaskCanceledException();

                success=false;
            }
            finally
            {

                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exitting 'ClearNearestCM' ");

            }
            return success;
        }
        public bool ClearCrossReferenceCM(CMData objCMData, CMData colCMData, int col_ref_dest_aisle)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entering 'ClearCrossReferenceCM' ");
            bool colapseMachineMoving = true;
            OpcOperationsService opcd = null;
            if (objErrorControllerService == null) new ErrorControllerImp();
            ErrorData objErrorData = new ErrorData();
            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                objCMDaoService = new CMDaoImp();

                objCMData.positionAisle = GetCMAisle(objCMData);
                colCMData.positionAisle = GetCMAisle(colCMData);

                colapseMachineMoving = objCMDaoService.IsCMBlockedInDB(colCMData.machineCode);//check collapse machine is already selected
                colapseMachineMoving |= IsCMInIdeal(colCMData) == false; // check the collapse machine is doing any work


                //check collapse machine coming to wards the detination aisle of current machine
                if (((objCMData.positionAisle > objCMData.destAisle && col_ref_dest_aisle >= objCMData.destAisle) ||
                               (objCMData.positionAisle < col_ref_dest_aisle && col_ref_dest_aisle <= objCMData.destAisle)) && colapseMachineMoving)
                    return false;


                if (((colCMData.positionAisle >= objCMData.positionAisle && colCMData.positionAisle <= objCMData.destAisle) ||
                    (colCMData.positionAisle <= objCMData.positionAisle && colCMData.positionAisle >= objCMData.destAisle)) && colCMData.positionAisle != 0 && objCMData.positionAisle != 0) //if collapse machine is inside the path
                {
                    

                    colCMData.destFloor = objCMData.destFloor;
                    int movAisle = 0;
                    int movRow = 0;
                    objCMDaoService.GetValidAisleForMoving(objCMData, out movAisle, out movRow);


                    colCMData.destAisle = movAisle;
                    colCMData.destRow = movRow;
                    if (colCMData.destAisle < 1 || colCMData.destAisle > 32) return false;



                    bool hasMoveSuccess = false;
                    objErrorData.machine = colCMData.machineCode;
                    objErrorData.floor = colCMData.floor;
                    objErrorData.aisle = colCMData.destAisle;
                    objErrorData.floor_row = colCMData.destRow;
                    objErrorControllerService.UpdateLiveCommandOfCM(objErrorData);
                    hasMoveSuccess = CMMove(colCMData);

                    if (hasMoveSuccess == true && CheckCMCommandDone(colCMData))
                    {

                        //colCMData.isDone = 1;
                        colCMData.isBlocked = false;
                        objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, colCMData.isBlocked);


                    }
                    else
                    {
                        //need to check nearest machine is trying to rotate and current machine destination aisle and current postion are same : june 26,2013

                        //dbpm.UpdateCrossReference(false, "", currMachine);

                        //colCMData.isDone = 1;
                        colCMData.isBlocked = false;
                        objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, colCMData.isBlocked);
                        return false;
                    }
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine,true);

                }
                if (colCMData.positionAisle != 0 && objCMData.positionAisle != 0)
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                //colCMData.isDone = 1;
                colCMData.isBlocked = false;
                objCMDaoService.UpdateMachineBlockStatus(colCMData.machineCode, colCMData.isBlocked);
                return false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exitting 'ClearCrossReferenceCM' ");

            }
        }
        public int FindCollapseMachineAisleAndSide(Model.CMData objCMData,Model.CMData colCMData)
        {
           
            
            int colMachineRefDestAisle = 0;
            //OPCServerDirector opcd = new OPCServerDirector();

            try
            {
                //colMachineCurrentAisle = opcd.SafeReadTag<Int16>(colMachineChannel, colMachineName, "CM_Position_for_L2");


                if (colCMData.destAisle != colCMData.positionAisle)
                {
                    if (objCMData.moveSide == -1)
                    {

                        if (colCMData.destAisle > colCMData.positionAisle)
                        {
                            colMachineRefDestAisle = colCMData.destAisle;
                            colCMData.moveSide=1; //need to check this assign is updating the actual argument
                            
                        }
                        else
                        {
                            colMachineRefDestAisle = colCMData.positionAisle;
                            colCMData.moveSide = -1;
                        }

                    }
                    else
                    {

                        if (colCMData.destAisle < colCMData.positionAisle)
                        {
                            colMachineRefDestAisle = colCMData.destAisle;
                            colCMData.moveSide = -1;

                        }
                        else
                        {
                            colMachineRefDestAisle = colCMData.positionAisle;
                            colCMData.moveSide = 1;

                        }

                    }
                }
                else
                {
                    colMachineRefDestAisle = colCMData.destAisle;

                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
                //if (opcd != null) opcd.Dispose();
            }
            return colMachineRefDestAisle;
        }
       
        public bool ClearForRotaion(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entering 'ClearForRotaion' ");
            CMData leftCMData = new CMData();
            CMData rightCMData = new CMData();
            int expectedRightgap = 2;
            int expectedLeftgap = 2;
            int leftMaxAisle = 9;
            int rightMaxAisle = 30;
            int leftAisleGap = 0;
            int rightAisleGap = 0;

            bool isLeftCMRotating = true;
            bool isRightCMRotating = true;
            bool isLeftCMAvailable = true;
            bool isRightCMAvailable = true;
            bool isRotate = false;
            OpcOperationsService opcd = null;
            bool isLeftCMOff = false;
            bool isRightCMOff = false;
            bool IsLeftCMHealthy = false;
            bool IsRightCMHealthy = false;

            try
            {
                if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
                if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                objCMDaoService.GetNearbyLCMlist(objCMData, out leftCMData, out rightCMData);
                isLeftCMAvailable = !string.IsNullOrEmpty(leftCMData.machineCode);
                isRightCMAvailable = !string.IsNullOrEmpty(rightCMData.machineCode);

                if (isLeftCMAvailable)
                {
                    isLeftCMOff = IsLeftCMSwitchedOff(objCMData);
                    if (!isLeftCMOff)
                    {
                        isLeftCMRotating = ConfirmCMRotationStatus(leftCMData);
                        if (isLeftCMRotating)
                            expectedLeftgap = 2;
                        else
                            expectedLeftgap = 1;
                        leftCMData.positionAisle = GetCMAisle(leftCMData);
                        leftCMData.isBlocked = objCMDaoService.IsCMBlockedInDB(leftCMData.machineCode);
                        if (leftCMData.isBlocked)
                        {
                            leftCMData.destAisle = opcd.ReadTag<Int32>(leftCMData.cmChannel, leftCMData.machineCode, OpcTags.CM_L2_Destination_Aisle);
                            int tempAisle = objErrorControllerService.GetLiveCommandOfMachine(leftCMData.machineCode).aisle;
                            if (tempAisle > leftCMData.destAisle && tempAisle > leftCMData.positionAisle && tempAisle != 0)
                                leftCMData.destAisle = tempAisle;
                        }
                        else
                            leftCMData.destAisle = leftCMData.positionAisle;
                    }
                    else
                    {
                        expectedLeftgap = 1;
                        leftCMData.positionAisle = objCMDaoService.GetCMAisleFromDB(leftCMData.machineCode);
                        leftCMData.destAisle = leftCMData.positionAisle;
                    }


                }
                else
                {
                    expectedLeftgap = 1;
                    leftCMData.positionAisle = leftMaxAisle - 1;
                    leftCMData.destAisle = leftCMData.positionAisle;

                }

                if (isRightCMAvailable)
                {
                    isRightCMOff = IsRightCMSwitchedOff(objCMData);
                    if (!isRightCMOff)
                    {
                        isRightCMRotating = ConfirmCMRotationStatus(rightCMData);
                        if (isRightCMRotating)
                            expectedRightgap = 2;

                        else
                            expectedRightgap = 1;
                        rightCMData.positionAisle = GetCMAisle(rightCMData);
                        rightCMData.isBlocked = objCMDaoService.IsCMBlockedInDB(rightCMData.machineCode);
                        if (rightCMData.isBlocked)
                        {
                            rightCMData.destAisle = opcd.ReadTag<Int32>(rightCMData.cmChannel, rightCMData.machineCode, OpcTags.CM_L2_Destination_Aisle);
                            int tempAisle = objErrorControllerService.GetLiveCommandOfMachine(rightCMData.machineCode).aisle;
                            if (tempAisle < rightCMData.destAisle && tempAisle < rightCMData.positionAisle && tempAisle != 0)
                                rightCMData.destAisle = tempAisle;
                        }
                        else
                            rightCMData.destAisle = rightCMData.positionAisle;
                    }
                    else
                    {
                        expectedRightgap = 1;
                        rightCMData.positionAisle = objCMDaoService.GetCMAisleFromDB(rightCMData.machineCode);
                        rightCMData.destAisle = rightCMData.positionAisle;
                    }
                }
                else
                {
                    expectedRightgap = 1;
                    rightCMData.positionAisle = rightMaxAisle + 1;
                    rightCMData.destAisle = rightCMData.positionAisle;

                }
                if (leftCMData.destAisle > leftCMData.positionAisle)
                    leftCMData.positionAisle = leftCMData.destAisle;
                if (rightCMData.destAisle < rightCMData.positionAisle)
                    rightCMData.positionAisle = rightCMData.destAisle;

                objCMData.positionAisle = GetCMAisle(objCMData);
                leftAisleGap = Math.Abs(leftCMData.positionAisle - objCMData.positionAisle) - 1;
                rightAisleGap = Math.Abs(rightCMData.positionAisle - objCMData.positionAisle) - 1;


                IsLeftCMHealthy = (leftCMData.positionAisle != 0 && leftCMData.destAisle != 0) || !isLeftCMAvailable;
                IsRightCMHealthy = rightCMData.positionAisle != 0 && rightCMData.destAisle != 0 || !isRightCMAvailable;

                isRotate = (leftAisleGap >= expectedLeftgap) && (rightAisleGap >= expectedRightgap);
                isRotate = isRotate && IsLeftCMHealthy && IsRightCMHealthy;

                

                //Trying to push left CM
                if (!isRotate && objCMData.cmdVal != 0)
                {
                    CMData pushRefCMData = new CMData();
                   
                    pushRefCMData = BasicConfig.Clone<CMData>(objCMData);
                    if (objCMData.cmdVal != 2)                                         //issue: 08.4.15.1
                        pushRefCMData.isHomeMove = true;
                    pushRefCMData.command = OpcTags.CM_L2_Move_Cmd;

                    bool case1 = objCMData.positionAisle <= 9 && objCMData.positionAisle != 0 && objCMData.machineCode.Equals("LCM_FLR01_01");
                    if (case1)
                    {
                        pushRefCMData.destAisle = 10;
                        pushRefCMData.destRow = 2;
                        if (CMMove(pushRefCMData))
                            CheckCMCommandDone(pushRefCMData);
                    }


                    if (!(leftAisleGap >= expectedLeftgap) && IsLeftCMHealthy && isLeftCMAvailable  && !isLeftCMOff)
                    {
                        pushRefCMData.destAisle = objCMData.positionAisle - expectedLeftgap;
                        pushRefCMData.destRow = 2;
                        if (pushRefCMData.destAisle >= leftMaxAisle)
                            ClearPath(pushRefCMData);
                    }

                    //Trying to push right CM
                    if (!(rightAisleGap >= expectedRightgap) && IsRightCMHealthy && isRightCMAvailable && !isRightCMOff)
                    {
                        pushRefCMData.destAisle = objCMData.positionAisle + expectedRightgap;
                        pushRefCMData.destRow = 2;
                        if (pushRefCMData.destAisle <= rightMaxAisle)
                            ClearPath(pushRefCMData);
                    }
                    

                }
            }
            catch (TaskCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'ClearForRotation':: " + errMsg.Message);
                throw new TaskCanceledException();
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                    throw new Exception();
                isRotate = false;
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'ClearForRotation':: " + ex.Message);
            }
            finally
            {
                if (opcd != null) opcd.Dispose();
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exitting 'ClearForRotaion' ");
            }
            return isRotate;

        }

        public bool CheckCMCommandDone(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + "--Entering 'CheckCMCommandDone' ");
            bool result = false;
         
            bool isWaitingForCmdDoneOn = false;
            int counter = 1;
            OpcOperationsService opcd = null;
            if (objQueueControllerService == null) 
                objQueueControllerService = new QueueControllerImp();
            if (objErrorControllerService == null) 
                objErrorControllerService = new ErrorControllerImp();
            if (objErrorDaoService == null) 
                objErrorDaoService = new ErrorDaoImp();
            bool triggerAlreadyEnabled = false;
            TriggerData objTriggerData = null;
            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                Thread.Sleep(2000);
                result = false;
                //if (objCMData.machineCode.Contains("LCM"))
                //    objCMData.remCode = objCMData.machineCode.Replace("LCM", "REM");
                //else if (objCMData.machineCode.Contains("UCM"))
                //    objCMData.remCode = objCMData.machineCode.Replace("UCM", "REM");
                //TimeSpan startTime = System.DateTime.Now.TimeOfDay;

                do
                {
                    triggerAlreadyEnabled = objErrorControllerService.GetTriggerActiveStatus(objCMData.machineCode);
                    if (!triggerAlreadyEnabled)
                        objTriggerData = NeedToShowTrigger(objCMData);
                    if (triggerAlreadyEnabled || objTriggerData.TriggerEnabled)
                    {
                        if (!triggerAlreadyEnabled)
                            objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                     

                        while (objErrorControllerService.GetTriggerActiveStatus(objCMData.machineCode))
                        {

                            

                            Thread.Sleep(1000);

                            

                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objCMData.queueId);
                            /******/

                        }
                       
                        if (objErrorControllerService.GetTriggerAction(objCMData.machineCode) == 1)
                        {
                            if (objCMData.command.Equals(OpcTags.CM_L2_Move_Cmd))
                            {
                                CMMove(objCMData);
                            }
                            else if (objCMData.command.Equals(OpcTags.CM_L2_Get_Cmd))
                            {
                                CMGet(objCMData);
                            }
                            else if (objCMData.command.Equals(OpcTags.CM_L2_Put_Cmd))
                            {
                                CMPut(objCMData);
                            }
                            Thread.Sleep(2000);
                            continue;
                        }

                    }


                    result = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_CMD_DONE);

                    if (counter > 3)
                    {
                        counter = 1;
                        /**checking transaction deleted or not****/
                        objQueueControllerService.CancelIfRequested(objCMData.queueId);
                        /******/
                        Thread.Sleep(700);

                    }
                    counter += 1;
                    if (result)
                    {
                        if (!ConfirmCommandCompleted(objCMData))
                            continue;
                    }
                } while (!result);
                // dbpm.UpdateMachineCommandStatus(machine, "", 0, 0, 0, 1); // update done=1 in trigger table
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CheckCMCommandDone':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception errMsg)
            {

                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --Exception 'CheckCMCommandDone':: " + errMsg.Message);
                if (errMsg is TaskCanceledException)
                    throw new Exception();
            }
            finally
            {
                

                //----->>>>>> update the table with collpase machine and cross reference tag
                //dbpm.UpdateCrossReference(false, "", machine);
                //<<<<<----- update the table with collpase machine and cross reference tag
                if (opcd != null) opcd.Dispose();
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --Exitting 'CheckCMCommandDone' ");
            }
            return result;
        }
        private bool ConfirmCommandCompleted(CMData objCMData)
        {
            if (objCMData.command.Equals(OpcTags.CM_L2_Get_Cmd))
            {
                return confirmGetCompleted(objCMData);
            }
            else if (objCMData.command.Equals(OpcTags.CM_L2_Put_Cmd))
            {
                return confirmPutCompleted(objCMData);
            }
            return true;
        }
        private bool confirmPutCompleted(CMData objCMData)
        {
            GlobalValues.palletStatus enumPalletStatus = GetPalletOnCMStatus(objCMData);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": pallet status=" + enumPalletStatus + "--After 'CMPut' ");

            if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.present)
            {
                TriggerData objTriggerData = new TriggerData();
                objTriggerData.TriggerEnabled = true;
                objTriggerData.MachineCode = objCMData.machineCode;
                objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                return false;
            }
            else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid)
            {
                Thread.Sleep(200);
                return false;
            }
            return true;
        }
        private bool confirmGetCompleted(CMData objCMData)
        {
            GlobalValues.palletStatus enumPalletStatus = GetPalletOnCMStatus(objCMData);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + ": pallet status=" + enumPalletStatus + "--After 'CMGet' ");

            if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notPresent)
            {
                TriggerData objTriggerData = new TriggerData();
                objTriggerData.TriggerEnabled = true;
                objTriggerData.MachineCode = objCMData.machineCode;
                objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                return false;
            }
            else if (enumPalletStatus == ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid)
            {
                Thread.Sleep(200);
                return false;
            }
            return true;
        }
        public bool CheckCMRotateDone(Model.CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entering 'CheckCMRotateDone' ");
            bool result = false;

            bool isWaitingForCmdDoneOn = false;
            int counter = 1;
            OpcOperationsService opcd = null;
            if (objQueueControllerService == null) 
                objQueueControllerService = new QueueControllerImp();
            if (objErrorControllerService == null) 
                objErrorControllerService = new ErrorControllerImp();
            if (objErrorDaoService == null) 
                objErrorDaoService = new ErrorDaoImp();
            bool triggerAlreadyEnabled = false;
            TriggerData objTriggerData = null;
            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                Thread.Sleep(3000);
                result = false;
                //TimeSpan startTime = System.DateTime.Now.TimeOfDay;

                do
                {
                    triggerAlreadyEnabled = objErrorControllerService.GetTriggerActiveStatus(objCMData.machineCode);
                    if (!triggerAlreadyEnabled)
                        objTriggerData = NeedToShowTrigger(objCMData);
                    if (triggerAlreadyEnabled || objTriggerData.TriggerEnabled)
                    {
                        if (!triggerAlreadyEnabled)
                            objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);

                        while (objErrorControllerService.GetTriggerActiveStatus(objCMData.machineCode))
                        {

                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objCMData.queueId);
                            /******/

                            Thread.Sleep(1000);
                        }
                        if (objErrorControllerService.GetTriggerAction(objCMData.machineCode) == 1)
                        {
                            CMRotate(objCMData);

                            Thread.Sleep(2000);
                        }
                    }


                    result = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_in_Position);

                    if (counter > 3)
                    {
                        counter = 1;
                        /**checking transaction deleted or not****/
                        objQueueControllerService.CancelIfRequested(objCMData.queueId);
                        /******/
                        Thread.Sleep(700);
                    }
                    counter += 1;
                } while (!result);
                // dbpm.UpdateMachineCommandStatus(machine, "", 0, 0, 0, 1); // update done=1 in trigger table
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --TaskCanceledException 'CheckRotateDone':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exception 'CheckRotateDone':: " + errMsg.Message);
                if (errMsg is TaskCanceledException)
                    throw new Exception();
            }
            finally
            {


                //----->>>>>> update the table with collpase machine and cross reference tag
                //dbpm.UpdateCrossReference(false, "", machine);
                //<<<<<----- update the table with collpase machine and cross reference tag
                if (opcd != null) opcd.Dispose();
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Exitting 'CheckCMRotateDone' ");
            }
            return result;
        }
       
        public string getREMCode(string machineCode)
        {
            string remCode = null;
            if (machineCode.Contains("LCM"))
                remCode = machineCode.Replace("LCM", "REM");
            else if (machineCode.Contains("UCM"))
                remCode = machineCode.Replace("UCM", "REM");

           
            return remCode;
        }
        public bool CheckCMHealthy(Model.CMData objCMData)
        {
            bool isHealthy = false;
            objCMDaoService = new CMDaoImp();

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_Position_for_L2))
                {
                    isHealthy = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_AUTO_READY);
                    isHealthy = isHealthy && !objCMDaoService.IsCMDisabled(objCMData.machineCode);
                    isHealthy = isHealthy && !objCMDaoService.IsCMSwitchOff(objCMData.machineCode);

                }

            }
            return isHealthy;

        }
        public TriggerData NeedToShowTrigger(Model.CMData objCMData)
        {
            
            bool needToShow = false;
            if (objCMDaoService==null) 
                objCMDaoService = new CMDaoImp();
            if (objEESControllerService == null) 
                objEESControllerService = new EESControllerImp();
            if (objVLCControllerService == null) 
                objVLCControllerService = new VLCControllerImp();
            if (objErrorDaoService == null)
                objErrorDaoService = new ErrorDaoImp();
            int checkCount = 0;
            int errorCode = 0;
            int check_max_count = 10;
            TriggerData objTriggerData = new TriggerData(); 

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                do
                {
                   

                        errorCode = GetError(objCMData);
                        if (errorCode != 0)
                        {
                            objTriggerData.category = TriggerData.triggerCategory.ERROR;
                            objTriggerData.ErrorCode = errorCode;
                            needToShow = true;
                            break;
                        }


                        needToShow = !opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_AUTO_READY);

                        if (needToShow )
                        {
                            if (checkCount >= check_max_count)
                            {
                                objTriggerData.category = TriggerData.triggerCategory.MANUAL;
                                break;
                            }
                            checkCount++;
                            Thread.Sleep(1000);
                            continue;
                        }


                        needToShow = objCMDaoService.IsCMDisabled(objCMData.machineCode);
                        if (needToShow )
                        {
                            objTriggerData.category = TriggerData.triggerCategory.DISABLE;
                            break;
                        }



                        needToShow =  objCMDaoService.IsCMSwitchOff(objCMData.machineCode);

                        if (objCMData.interactMachine !=null && objCMData.interactMachine.Contains("EES"))
                        {
                            needToShow = needToShow || objEESControllerService.IsEESDisabled(objCMData.interactMachine);
                        }
                        else if (objCMData.interactMachine != null && objCMData.interactMachine.Contains("VLC"))
                        {
                            needToShow = needToShow || objVLCControllerService.IsVLCDisabled(objCMData.interactMachine);
                        }
                        if (needToShow )
                        {
                            objTriggerData.category = TriggerData.triggerCategory.WAITING;
                            break;
                        }



                } while (needToShow);
            }
            if (needToShow)
            {
                objTriggerData.MachineCode = objCMData.machineCode;
                objTriggerData.TriggerEnabled = true;
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM = " + objCMData.machineCode + "--'NeedToShowTrigger' = " + needToShow);
            }
            return objTriggerData;
        }
       
        public int GetCMAisle(CMData objCMData)
        {
            int posAisle=0;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                posAisle = opcd.ReadTag<Int32>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Position_for_L2);
            }
            return posAisle;

        }
        public bool CheckCMIsRotating(CMData objCMData)
        {
            bool isCMRotating = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                isCMRotating = !opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.LCM_TT_in_Position);
            }

            return isCMRotating;
        }
        public bool ConfirmCMRotationStatus(CMData objCMData)
        {
            bool isCMRotating = false;
            ErrorData objTriggerData = new ErrorData();

            isCMRotating = CheckCMIsRotating(objCMData);
            if (!isCMRotating)
            {
                objTriggerData = objErrorControllerService.GetLiveCommandOfMachine(objCMData.machineCode);
                if (!string.IsNullOrEmpty(objTriggerData.command))
                    isCMRotating = objTriggerData.command.Equals(OpcTags.LCM_Rotate_to_0_from_L2);
            }

            return isCMRotating;


            ////read col_machine position col_machine_pos
        }
        public bool IsCMInIdeal(CMData objCMData)
        {
            bool isIdeal = false;
            objErrorControllerService = new ErrorControllerImp();
            try
            {
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    if (opcd.IsMachineHealthy(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Destination_Aisle))
                    {

                        isIdeal=!objErrorControllerService.GetTriggerActiveStatus(objCMData.machineCode);
                        isIdeal = isIdeal && opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_CM_Idle);

                        /*
                        if (isIdeal)
                            isIdeal &= opcd.SafeReadTag<bool>(channel, machine, "L2_Move_Cmd") == false;

                        if (isIdeal)
                            isIdeal &= opcd.SafeReadTag<bool>(channel, machine, "L2_Get_Cmd") == false;

                        if (isIdeal)
                            isIdeal &= opcd.SafeReadTag<bool>(channel, machine, "L2_Put_Cmd") == false;

                        if (isIdeal)
                            isIdeal &= opcd.SafeReadTag<bool>(channel, machine, "CM_Locked") == false;
                        * */

                        if (objCMData.machineCode.Contains("LCM"))
                        {
                            isIdeal = isIdeal && !CheckCMIsRotating(objCMData);
                        }
                         
                    }
                   
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            // Console.WriteLine("CHECKING " + machine + " IS IDEAL HAS ENDED");
            return isIdeal;

        }
        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            objCMDaoService = new CMDaoImp();
            return objCMDaoService.UpdateMachineBlockStatus(machine_code, blockStatus);
        }
        public bool UpdateMachineBlockStatusForHome(string machine_code, bool blockStatus)
        {
            objCMDaoService = new CMDaoImp();
            return objCMDaoService.UpdateMachineBlockStatusForHome(machine_code, blockStatus);
        }
        
        public bool AsynchReadSettings()
        {
            // add a periodic data callback group and add one item to the group
            OPCDA.NET.RefreshEventHandler dch = new OPCDA.NET.RefreshEventHandler(AsynchReadListenerForCM);
            OpcServer opcServer = OpcConnection.GetOPCServerConnection();
            uGrp = new OPCDA.NET.RefreshGroup(opcServer, DAUpdateRate, dch);

            int rtc = 0;

            CMDaoService objCMDaoService = new CMDaoImp();
            List<CMData> cmList = objCMDaoService.GetCMList();


            try
            {

                foreach (CMData objCMData in cmList)
                {
                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Min_Window_Limit);
                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Max_Window_Limit);
                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_Position_for_L2);


                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Destination_Aisle);
                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Destination_Row);

                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_AUTO_READY);
                    
                    //rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Get_Cmd);
                    //rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Put_Cmd);

                    //rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Move_Cmd);

                    rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.remCode + "." + OpcTags.CM_Pallet_Present_on_REM);



                    if (objCMData.machine.Equals("LCM"))
                    {
                       // rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.LCM_L2_AUTO_READY);
                        //rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.LCM_L2_TT_ROT);
                    }
                    else
                    {

                       // rtc = uGrp.Add(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.UCM_L2_AUTO_READY);
                    }


                }
            }
            catch (Exception errMsg)
            {
                rtc = 0;
                Console.WriteLine(errMsg.Message);
            }
            finally
            {

            }

            return rtc == 0 ? false : true;
        }
        public void AsynchReadListenerForCM(object sender, OPCDA.NET.RefreshEventArguments arg)
        {
            OPCDA.NET.OPCItemState res = arg.items[0].OpcIRslt;

            try
            {
                if (arg.Reason == OPCDA.NET.RefreshEventReason.DataChanged)
                {            // data changes
                    if (HRESULTS.Succeeded(res.Error))
                    {
                      
                        OPCDA.NET.ItemDef opcItemDef = (OPCDA.NET.ItemDef)arg.items.GetValue(0);

                        string[] iterateItemName = opcItemDef.OpcIDef.ItemID.Split(new Char[] { '.' });
                        string channel = "";
                        string machineName = "";
                        string tag_Name = "";
                        if (iterateItemName.Length == 3)
                        {
                            channel = iterateItemName[0].ToString();
                            machineName = iterateItemName[1].ToString();
                            tag_Name = iterateItemName[2].ToString();

                            updateDataFromOpcListener = new Thread(delegate()
                            {
                                UpdateMachineTagValueToDBFromListener(machineName, tag_Name, res.DataValue);
                            });
                            updateDataFromOpcListener.IsBackground = true;
                            updateDataFromOpcListener.Start();
                            
                        }
                     
                    }
                }
            }
            catch (Exception errMsg)
            {
            }
        }



        public void GetDataTypeAndFieldOfTag(string opcTag, out int dataType, out string tableField, out bool isRem)
        {
            isRem = false;
            tableField = "";
            dataType = 0;//1:bool;2:number;3:string
            if (opcTag.Equals(OpcTags.CM_L2_Min_Window_Limit))
            {
                tableField = "VIRTUAL_AISLE_MIN";
                dataType = 2;
            }
            else if (opcTag.Equals(OpcTags.CM_L2_Max_Window_Limit))
            {
                tableField = "VIRTUAL_AISLE_MAX";
                dataType = 2;
            }
            else if (opcTag.Equals(OpcTags.CM_Position_for_L2))
            {
                tableField = "POSITION_AISLE";
                dataType = 2;
            }

            //else if (machineTag.Equals(OpcTags.CM_L2_Destination_Aisle))
            //{
            //    field = "VIRTUAL_AISLE_MIN";

            //}
            else if (opcTag.Equals(OpcTags.CM_L2_Destination_Row))
            {
                tableField = "POSITION_ROW";
                dataType = 2;
            }


            else if (opcTag.Equals(OpcTags.CM_L2_AUTO_READY))
            {
                tableField = "AUTO_MODE";
                dataType = 1;

            }
            //else if (opcTag.Equals(OpcTags.CM_L2_Get_Cmd))
            //{
            //    tableField = "VIRTUAL_AISLE_MIN";
            //}
            //else if (opcTag.Equals(OpcTags.CM_L2_Put_Cmd))
            //{
            //    tableField = "VIRTUAL_AISLE_MIN";

            //}
            //else if (opcTag.Equals(OpcTags.CM_L2_Move_Cmd))
            //{
            //    tableField = "VIRTUAL_AISLE_MIN";

            //}
            else if (opcTag.Equals(OpcTags.CM_Pallet_Present_on_REM))
            {
                tableField = "IS_PALLET_PRESENT";
                dataType = 1;
                isRem = true;
                //use machine rem code

            }
            else if (opcTag.Equals(OpcTags.LCM_L2_AUTO_READY))
            {
                tableField = "AUTO_MODE";
                dataType = 1;

            }
            else if (opcTag.Equals(OpcTags.UCM_L2_AUTO_READY))
            {
                tableField = "AUTO_MODE";
                dataType = 1;

            }

        }




        public bool UpdateCMIntData(string machineCode, string opcTag, int dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;
            objCMDaoService = new CMDaoImp();

            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);
            if (isRem)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objCMDaoService.UpdateBoolValueUsingRemCode(machineCode, field, boolDataValue);
            }
            else
            {
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objCMDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    if (intDataValue != 0)
                        objCMDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objCMDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            }
            return true;
        }

        public bool UpdateCMBoolData(string machineCode, string opcTag, bool dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;

            objCMDaoService = new CMDaoImp();
            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);
            if (isRem)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objCMDaoService.UpdateBoolValueUsingRemCode(machineCode, field, boolDataValue);
            }
            else
            {
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objCMDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    objCMDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objCMDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            }
            return true;
        }
        

        public bool UpdateMachineTagValueToDBFromListener(string machineCode, string machineTag, Object dataValue)
        {
            string field = "";
            bool boolDataValue;
            Int16 intDataValue;
            string stringDataValue;
            objCMDaoService = new CMDaoImp();
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            
            GetDataTypeAndFieldOfTag(machineTag, out dataType, out field, out isRem);
            if (isRem)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objCMDaoService.UpdateBoolValueUsingRemCode(machineCode, field, boolDataValue);
            }
            else
            {
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objCMDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    objCMDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objCMDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            }
            return true;
        }
        public bool UpdateCMClearRequest(string machineCode, string clearMachine, int clearFlag)
        {
            if (objCMDaoService==null) objCMDaoService = new CMDaoImp();
            return objCMDaoService.UpdateCMClearRequest(machineCode, clearMachine, clearFlag);
        }
        public bool UpdateCMClearPermission(string machineCode)
        {
            if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
            return objCMDaoService.UpdateCMClearPermission(machineCode);
        }
        public bool GetCMClearRequest(string machineCode, out string clearingMachine, out int clearFlag)
        {
            if (objCMDaoService == null) objCMDaoService = new CMDaoImp();
            return objCMDaoService.GetCMClearRequest(machineCode, out clearingMachine, out clearFlag);
        }




        public bool IsPalletPresentOnREM(CMData objCMData)
        {
            bool isPallet = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objCMData.cmChannel + "." + objCMData.machineCode + "." + OpcTags.CM_L2_Destination_Aisle))
                {
                    isPallet=opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.remCode, OpcTags.CM_Pallet_Present_on_REM);
                }
            }
            return isPallet;
        }
        public bool IsBothRowAreInSameSide(int row1, int row2)
        {
            bool row1IsNorth = false;
            bool row2IsNorth = false;

            row1IsNorth = row1 == 1 || row1 == 2;
            row2IsNorth = row2 == 1 || row2 == 2;
            return row1IsNorth == row2IsNorth;

        }
        public bool IsRowInNorthSide(int row1)
        {
            bool row1IsNorth = false;
           
            row1IsNorth = row1 == 1 || row1 == 2;
            return row1IsNorth;
        }
       


        public bool IsLeftCMSwitchedOff(CMData objCMData)
        {
            bool isSwitchedOff = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {

                isSwitchedOff = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_WestCMOff);
            }
            return isSwitchedOff;
        }
        public bool IsRightCMSwitchedOff(CMData objCMData)
        {
            bool isSwitchedOff = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {

                isSwitchedOff = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_EastCMOff);
            }
            return isSwitchedOff;
        }
        
        //public bool GetPalletPresentStatus(CMData objCMData)
        //{
        //    bool palletPresent = false;
        //    string remCode = getREMCode(objCMData.machineCode);
        //    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
        //    {

        //        palletPresent = opcd.ReadTag<bool>(objCMData.cmChannel, remCode, OpcTags.REM_Pallet_Present);

        //    }
        //    return palletPresent;
        //}

        public bool GetPalletPresentStatus(CMData objCMData)
        {
            bool palletPresent = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {

                palletPresent = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Pallet_Present);

            }
            return palletPresent;
        }

        public bool GetPalletNotPresentStatus(CMData objCMData)
        {
          
            bool palletNotPresent = false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {

                palletNotPresent = opcd.ReadTag<bool>(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_Pallet_Not_Present);
            }
            return palletNotPresent;
        }

        public ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus GetPalletOnCMStatus(CMData objCMData)
        {
            ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus enumPalletStatus = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notValid;
            bool palletPresent = GetPalletPresentStatus(objCMData);
            bool palletNotPresent = GetPalletNotPresentStatus(objCMData);

            if (palletPresent)
                enumPalletStatus = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.present;
            else if (palletNotPresent)
                enumPalletStatus = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus.notPresent;
            return enumPalletStatus;
        }
        public CMData GetBlockedCMDetails(Int64 queueId)
        {
            if (objCMDaoService == null) 
                objCMDaoService = new CMDaoImp();
            return objCMDaoService.GetBlockedCMDetails(queueId);
        }
        
        //18Oct18
        public int GetError(CMData objCMData)
        {
            int error = 0;
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            objCMData.remCode = getREMCode(objCMData.machineCode);

            error = objErrorControllerService.GetErrorCode(objCMData.cmChannel, objCMData.machineCode, OpcTags.CM_L2_Error_Data_Register);
            if (error == 0)
                error = objErrorControllerService.GetErrorCode(objCMData.cmChannel, objCMData.remCode, OpcTags.CM_L2_Error_Data_Register);

            return error;
        }
    }
}
