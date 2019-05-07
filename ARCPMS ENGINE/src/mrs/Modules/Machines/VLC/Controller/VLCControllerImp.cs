using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPC;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.DB;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;
using System.Threading.Tasks;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller
{
    class VLCControllerImp : CommonServicesForMachines,VLCControllerService
    {
        OPCDA.NET.RefreshGroup uGrp;
        int DAUpdateRate = 1;
        Thread updateDataFromOpcListener = null;
        VLCDaoService objVLCDaoService = null;
        ErrorDaoService objErrorDaoService = null;
        QueueControllerService objQueueControllerService = null;
        ErrorControllerService objErrorControllerService = null;
        public bool UpdateMachineValues()
        {
            objVLCDaoService = new VLCDaoImp();
            List<VLCData> vlcList;
            bool result;
            int dataValue;
            bool dataValueInBool;



            try
            {
                vlcList = objVLCDaoService.GetVLCList();
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    foreach (VLCData objVLCData in vlcList)
                    {

                        if (opcd.IsMachineHealthy(objVLCData.machineChannel + "." + objVLCData.machineCode + "." + OpcTags.VLC_Auto_Ready) == true)
                        {
                            dataValueInBool = false;
                            dataValueInBool = opcd.ReadTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_Auto_Ready);
                            UpdateVLCBoolData(objVLCData.machineCode, OpcTags.VLC_Auto_Ready, dataValueInBool);

                            
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


        public bool AsynchReadSettings()
        {
            // add a periodic data callback group and add one item to the group
            OPCDA.NET.RefreshEventHandler dch = new OPCDA.NET.RefreshEventHandler(AsynchReadListenerForVLC);
            uGrp = new OPCDA.NET.RefreshGroup(OpcConnection.GetOPCServerConnection(), DAUpdateRate, dch);

            int rtc = 0;

            VLCDaoService objVLCDaoService = new VLCDaoImp();
            List<VLCData> vlcList = objVLCDaoService.GetVLCList();


            try
            {

                foreach (VLCData objVLCData in vlcList)
                {
                    rtc = uGrp.Add(objVLCData.machineChannel + "." + objVLCData.machineCode + "." + OpcTags.VLC_Auto_Ready);
                  

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


        public bool UpdateMachineTagValueToDBFromListener(string machineCode, string machineTag, object dataValue)
        {
            string field = "";
            bool boolDataValue;
            Int16 intDataValue;
            string stringDataValue;
            objVLCDaoService = new VLCDaoImp();
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;

            GetDataTypeAndFieldOfTag(machineTag, out dataType, out field, out isRem);

            if (dataType == 1)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objVLCDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
            }
            else if (dataType == 2)
            {
                intDataValue = Convert.ToInt16(dataValue);
                objVLCDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
            }
            else if (dataType == 3)
            {
                stringDataValue = Convert.ToString(dataValue);
                objVLCDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
            }

            return true;
        }

        public void GetDataTypeAndFieldOfTag(string opcTag, out int dataType, out string tableField, out bool isRem)
        {
            isRem = false;
            tableField = "";
            dataType = 0;//1:bool;2:number;3:string


            if (opcTag.Equals(OpcTags.VLC_Auto_Mode) || opcTag.Equals(OpcTags.VLC_Auto_Ready))
            {
                tableField = "IS_AUTOMODE";
                dataType = 1;

            }
            
        }
        
        public bool VLCMove(Model.VLCData objVLCData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Entering 'VLCMove' ");
            bool isCMHealthy = false;
            bool success = false;
            if (objQueueControllerService == null) 
                objQueueControllerService = new QueueControllerImp();
            if (objErrorDaoService == null)
                objErrorDaoService = new ErrorDaoImp();
            
            do
            {
                /**checking transaction deleted or not****/
                objQueueControllerService.CancelIfRequested(objVLCData.queueId);
                /******/
                try
                {
                    TriggerData objTriggerData = NeedToShowTrigger(objVLCData);
                    if (objTriggerData.TriggerEnabled)
                    {

                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
                        break;
                    }
                    isCMHealthy = CheckVLCHealthy(objVLCData);

                    if (!isCMHealthy)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {
                        objVLCData.floor = opcd.ReadTag<Int32>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_At_Floor);
                        if (objVLCData.floor != objVLCData.destFloor)
                        {

                            opcd.WriteTag<int>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_DestFloor, objVLCData.destFloor);
                            success = opcd.WriteTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, objVLCData.command, true);
                            
                        }
                        else
                        {
                            success = opcd.WriteTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, objVLCData.command, true);
                            success = true;
                        }


                    }

                }
                catch (OperationCanceledException errMsg)
                {
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": CM=" + objVLCData.machineCode + " --TaskCanceledException 'VLCMove':: " + errMsg.Message);
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exception 'VLCMove':: " + ex.Message);
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(objVLCData.queueId);
                    /******/
                    if (ex is TaskCanceledException)
                        throw new Exception();
                    success = false;
                }
                

            } while (!success);
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exitting 'VLCMove' ");
            return success;

        }

        public bool CheckVLCCommandDone(Model.VLCData objVLCData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Entering 'CheckVLCCommandDone' ");
            bool result = false;

            bool isWaitingForCmdDoneOn = false;
            int counter = 1;
            OpcOperationsService opcd = null;
            if (objQueueControllerService == null)
                objQueueControllerService = new QueueControllerImp();
          
            if (objErrorDaoService == null)
                objErrorDaoService = new ErrorDaoImp();
            bool triggerAlreadyEnabled = false;
            TriggerData objTriggerData = null;

            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                Thread.Sleep(3000);
                result = false;

                do
                {
                    triggerAlreadyEnabled = objErrorDaoService.GetTriggerActiveStatus(objVLCData.machineCode);
                    if (!triggerAlreadyEnabled)
                        objTriggerData = NeedToShowTrigger(objVLCData);
                    if (triggerAlreadyEnabled || objTriggerData.TriggerEnabled)
                    {

                        if (!triggerAlreadyEnabled)
                            objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);


                        while (objErrorDaoService.GetTriggerActiveStatus(objVLCData.machineCode))
                        {
                            /**checking transaction deleted or not****/
                            objQueueControllerService.CancelIfRequested(objVLCData.queueId);
                            /******/
                            Thread.Sleep(1000);
                        }

                        if (objErrorDaoService.GetTriggerAction(objVLCData.machineCode) == 1)
                        {
                            VLCMove(objVLCData);

                            Thread.Sleep(2000);
                        }
                    }


                    result = opcd.ReadTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_CP_Done);

                    if (counter > 3)
                    {
                        counter = 1;
                        /**checking transaction deleted or not****/
                        objQueueControllerService.CancelIfRequested(objVLCData.queueId);
                        /******/
                        Thread.Sleep(700);
                    }
                    counter += 1;
                } while (!result);
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": CM=" + objVLCData.machineCode + " --TaskCanceledException 'CheckVLCCommandDone':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception errMsg)
            {
                
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exception 'CheckVLCCommandDone':: " + errMsg.Message);
                if (errMsg is TaskCanceledException)
                    throw new Exception();
            }
            finally
            {


                if (opcd != null) opcd.Dispose();
            }
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exitting 'CheckVLCCommandDone' ");
            return result;
        }

        public int CheckError(Model.VLCData objVLCData)
        {
            throw new NotImplementedException();
        }

        public bool CheckVLCHealthy(Model.VLCData objVLCData)
        {
            bool isHealthy = false;
            objVLCDaoService = new VLCDaoImp();

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objVLCData.machineChannel + "." + objVLCData.machineCode + "." + OpcTags.VLC_Auto_Ready))
                {
                    isHealthy = opcd.ReadTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_Auto_Ready);
                    isHealthy = isHealthy && !objVLCDaoService.IsVLCDisabled(objVLCData.machineCode);
                    isHealthy = isHealthy && !objVLCDaoService.IsVLCSwitchOff(objVLCData.machineCode);
                    isHealthy = isHealthy && opcd.ReadTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_CP_Done);

                }

            }
            return isHealthy;
        }
        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            if (objVLCDaoService == null)
                objVLCDaoService = new VLCDaoImp();
            return objVLCDaoService.UpdateMachineBlockStatus(machine_code, blockStatus);
           
        }

        public void AsynchReadListenerForVLC(object sender, OPCDA.NET.RefreshEventArguments arg)
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


        public bool UpdateVLCIntData(string machineCode, string opcTag, int dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;


            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);

            if (dataType == 2)
            {
                intDataValue = Convert.ToInt16(dataValue);
                objVLCDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
            }
            else if (dataType == 1)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objVLCDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                
            }
            else if (dataType == 3)
            {
                stringDataValue = Convert.ToString(dataValue);
                objVLCDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
            }

            return true;
        }

        public bool UpdateVLCBoolData(string machineCode, string opcTag, bool dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;


            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);

            if (dataType == 1)
            {
                boolDataValue = Convert.ToBoolean(dataValue);
                objVLCDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);

            }
            else if (dataType == 2)
            {
                intDataValue = Convert.ToInt16(dataValue);
                objVLCDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
            }
            
            else if (dataType == 3)
            {
                stringDataValue = Convert.ToString(dataValue);
                objVLCDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
            }

            return true;
        }





        public bool ConfirmReachedAtFloor(VLCData objVLCData) 
        {
           
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Entering 'ConfirmReachedAtFloor' ");
            if (objVLCDaoService == null) objVLCDaoService = new VLCDaoImp();
            VLCData confirmVLCData = new VLCData();
            bool vlcConfirmed = false;
            if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();

            try
            {
                confirmVLCData.machineCode = objVLCData.machineCode;

                confirmVLCData = objVLCDaoService.GetVLCDetails(objVLCData);

                confirmVLCData.destFloor = objVLCData.destFloor;
                confirmVLCData.queueId = objVLCData.queueId;
                while(VLCAtFloor(confirmVLCData) != confirmVLCData.destFloor)
                {
                    VLCMove(confirmVLCData);
                    /**checking transaction deleted or not****/
                    objQueueControllerService.CancelIfRequested(confirmVLCData.queueId);
                    /******/
                    Thread.Sleep(500);
                }
                vlcConfirmed = true;
            }
            catch (OperationCanceledException errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": VLC=" + objVLCData.machineCode + " --TaskCanceledException 'ConfirmReachedAtFloor':: " + errMsg.Message);
                throw new OperationCanceledException();
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": VLC=" + objVLCData.machineCode + " --Exception 'ConfirmReachedAtFloor':: " + errMsg.Message);
                if (errMsg is TaskCanceledException)
                    throw new Exception();
            }
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exitting 'ConfirmReachedAtFloor' ");
            return vlcConfirmed;
        }
        public int VLCAtFloor(VLCData objVLCData)
        {
            int vlcFloor=0;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                vlcFloor = opcd.ReadTag<Int32>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_At_Floor);
            }
            return vlcFloor;
        }
        public VLCData GetVLCDetails(VLCData objVLCData)
        {
            if (objVLCDaoService == null) objVLCDaoService = new VLCDaoImp();
            return objVLCDaoService.GetVLCDetails(objVLCData);
        }
        public bool IsVLCDisabled(string machineName)
        {
             if (objVLCDaoService == null) objVLCDaoService = new VLCDaoImp();
             return objVLCDaoService.IsVLCDisabled(machineName);
        }


        public TriggerData NeedToShowTrigger(VLCData objVLCData)
        {

            bool needToShow = false;
            if (objVLCDaoService == null) objVLCDaoService = new VLCDaoImp();

            int checkCount = 0;
            int errorCode = 0;
            TriggerData objTriggerData = new TriggerData();
            if (objErrorControllerService == null)
                objErrorControllerService = new ErrorControllerImp();
            int max_check_count = 10;


            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                do
                {
                    
                        errorCode = objErrorControllerService.GetErrorCode(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_L2_ErrCode);
                        if (errorCode > 0)
                        {
                            objTriggerData.category = TriggerData.triggerCategory.ERROR;
                            objTriggerData.ErrorCode = errorCode;
                            needToShow = true;
                            break;
                        }

                       
                       
                        needToShow = !opcd.ReadTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_Auto_Ready);

                        if (needToShow)
                        {
                            if (checkCount >= max_check_count)
                            {
                                objTriggerData.category = TriggerData.triggerCategory.MANUAL;
                                break;
                            }
                            checkCount++;
                            Thread.Sleep(1000);
                            continue;
                        }



                        needToShow =  objVLCDaoService.IsVLCDisabled(objVLCData.machineCode);
                        if (needToShow )
                        {
                            objTriggerData.category = TriggerData.triggerCategory.DISABLE;
                            break;
                        }

                   
                    checkCount++;
                    Thread.Sleep(200);
                } while (needToShow );
            }
            if (needToShow)
            {
                objTriggerData.MachineCode = objVLCData.machineCode;
                objTriggerData.TriggerEnabled = true;
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": VLC = " + objVLCData.machineCode + "--'NeedToShowTrigger For VLC' = " + needToShow);
            }
            return objTriggerData;
        }


       
       public Model.VLCData GetVLCDetails(Int64 queueId)
        {
            if (objVLCDaoService == null) objVLCDaoService = new VLCDaoImp();
            return objVLCDaoService.GetVLCDetails(queueId);
        }
       public bool VLCHomeMove(Model.VLCData objVLCData)
       {
           Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Entering 'VLCHomeMove' ");
           bool isCMHealthy = false;
           bool success = false;
           bool isBlocked = false;
           if (objQueueControllerService == null)
               objQueueControllerService = new QueueControllerImp();
           if (objErrorControllerService == null)
               objErrorControllerService = new ErrorControllerImp();
           if (objVLCDaoService == null)
               objVLCDaoService = new VLCDaoImp();


           /**checking transaction deleted or not****/
           objQueueControllerService.CancelIfRequested(objVLCData.queueId);
           /******/
           try
           {
               isBlocked=objVLCDaoService.IsVLCBlockedInDB(objVLCData.machineCode);
               if (isBlocked)
                   return false;
               TriggerData objTriggerData = NeedToShowTrigger(objVLCData);
               if (objTriggerData.TriggerEnabled)
                   return false;

               isCMHealthy = CheckVLCHealthy(objVLCData);
               if (!isCMHealthy)
                   return false;

               using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
               {
                   objVLCData.floor = opcd.ReadTag<Int32>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_At_Floor);
                   if (objVLCData.floor != objVLCData.destFloor)
                   {

                       opcd.WriteTag<int>(objVLCData.machineChannel, objVLCData.machineCode, OpcTags.VLC_DestFloor, objVLCData.destFloor);
                       success = opcd.WriteTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, objVLCData.command, true);

                   }
                   else
                   {
                       success = opcd.WriteTag<bool>(objVLCData.machineChannel, objVLCData.machineCode, objVLCData.command, true);
                       success = true;
                   }


               }

           }
           catch (OperationCanceledException errMsg)
           {
               Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ": CM=" + objVLCData.machineCode + " --TaskCanceledException 'VLCMove':: " + errMsg.Message);
               throw new OperationCanceledException();
           }
           catch (Exception ex)
           {

               Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exception 'VLCHomeMove':: " + ex.Message);
               /**checking transaction deleted or not****/
               objQueueControllerService.CancelIfRequested(objVLCData.queueId);
               /******/
               if (ex is TaskCanceledException)
                   throw new Exception();
               success = false;
           }



           Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objVLCData.queueId + ":--Exitting 'VLCHomeMove' ");
           return success;

       }
    }
}
