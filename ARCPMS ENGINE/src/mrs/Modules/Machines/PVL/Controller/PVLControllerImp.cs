using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.DB;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using ARCPMS_ENGINE.src.mrs.Global;
using OPC;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller
{
    
    class PVLControllerImp : CommonServicesForMachines,PVLControllerService
    {
        PVLDaoService objPVLDaoService = null;
        ErrorControllerService objErrorControllerService = null;
        ErrorDaoService objErrorDaoService = null;

        public List<Model.PVLData> GetPVLList()
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.GetPVLList();
        }
        public Model.PVLData GetPVLDetails(Model.PVLData objPVLData)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.GetPVLDetails(objPVLData);
        }
        public bool UpdateMachineValues()
        {
            throw new NotImplementedException();
        }


        public bool AsynchReadSettings()
        {
            throw new NotImplementedException();
        }

        public bool PVLMove(Model.PVLData objPVLData)
        {
            bool isPVLHealthy = false;
            bool success = false;

            //do
            //{
                try
                {

                    isPVLHealthy = CheckPVLHealthy(objPVLData);

                    if (!isPVLHealthy) return false;

                    using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                    {
                        objPVLData.floor = opcd.ReadTag<Int32>(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_Current_Floor);
                        if (objPVLData.floor != objPVLData.destFloor)
                        {

                            opcd.WriteTag<int>(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_Request_Floor, objPVLData.destFloor);
                            success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, true);
                            //Thread.Sleep(500);
                            //success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, false);
                        }
                        else
                        {
                            success = true;
                        }


                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    success = false;
                }


           // } while (!success);
            return success;
        }

        public bool PVLPut(Model.PVLData objPVLData)
        {
            bool isPVLHealthy = false;
            bool success = false;

            //do
            //{
            try
            {

                isPVLHealthy = CheckPVLHealthy(objPVLData);

                if (!isPVLHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {

                    success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, true);
                    //Thread.Sleep(500);
                    //success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, false);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }


            // } while (!success);
            return success;
        }

        public bool PVLGet(Model.PVLData objPVLData)
        {
            bool isPVLHealthy = false;
            bool success = false;

            //do
            //{
            try
            {

                isPVLHealthy = CheckPVLHealthy(objPVLData);

                if (!isPVLHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {

                    success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, true);
                    //Thread.Sleep(500);
                    //success = opcd.WriteTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, objPVLData.command, false);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }


            // } while (!success);
            return success;
        }

        public bool IsPalletOnPVL(Model.PVLData objPVLData, out bool isHealthy)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered IsPalletOnPVL : PVL=" + objPVLData.machineCode);
            bool isPalletPresent = false;
            isHealthy = false;
            int checkCount = 0;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                do
                {
                    //string pvlDeck = objPVLData.machineCode.Replace("Drive", "Deck");
                    isHealthy = opcd.IsMachineHealthy(objPVLData.machineChannel + "." + objPVLData.machineCode + "." + OpcTags.PVL_Auto_Ready);
                    if (isHealthy)
                        isPalletPresent = opcd.ReadTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_Deck_Pallet_Present);
                    checkCount++;
                    Thread.Sleep(100);
                } while (!isPalletPresent && checkCount < 5);
            }
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting IsPalletOnPVL : PVL=" + objPVLData.machineCode
                + ", isPalletPresent= " + isPalletPresent + ", isHealthy= " + isHealthy);
            return isPalletPresent;
        }

        public bool CheckPVLCommandDone(Model.PVLData objPVLData)
        {
            bool result = false;

            int counter = 1;
            OpcOperationsService opcd = null;

            int commandType = 0;
            string doneCheckTag = null;
            int error = 0;
            if(objErrorControllerService==null) 
                objErrorControllerService= new ErrorControllerImp();
            if (objErrorDaoService == null)
                objErrorDaoService = new ErrorDaoImp();

            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                objErrorControllerService = new ErrorControllerImp();
                Thread.Sleep(2000);
                result = false;
                FindCommandTypeAndDoneTag(objPVLData, out commandType, out doneCheckTag);


                do
                {
                    error=objErrorControllerService.GetErrorCode(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_L2_ErrCode) ;
                   if (error > 0)
                    {

                        TriggerData objTriggerData = new TriggerData();
                        objTriggerData.MachineCode = objPVLData.machineCode;
                        objTriggerData.category = TriggerData.triggerCategory.ERROR;
                        objTriggerData.ErrorCode = error;
                        objTriggerData.TriggerEnabled = true;
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);


                        while (objErrorControllerService.GetTriggerActiveStatus(objPVLData.machineCode))
                        {



                            Thread.Sleep(1000);
                        }
                        if (objErrorControllerService.GetTriggerAction(objPVLData.machineCode) == 1)
                        {
                            DoTriggerAction(objPVLData, commandType);

                            Thread.Sleep(2000);
                        }

                    }

                    result = opcd.ReadTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, doneCheckTag);

                    if (counter > 3) Thread.Sleep(700);
                    counter += 1;
                } while (!result);
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {


                if (opcd != null) opcd.Dispose();
            }
            return result;
        }
        public void FindCommandTypeAndDoneTag(Model.PVLData objPVLData, out int commandType, out string doneTag)
        {
            commandType = 0;
            doneTag = null;
            if (objPVLData.command.Equals(OpcTags.PVL_CP_Start))
            {
                commandType = 1;
                doneTag = OpcTags.PVL_CP_Done;
            }
            else if (objPVLData.command.Equals(OpcTags.PVL_Get_PB))
            {
                commandType = 2;
                doneTag = OpcTags.PVL_Get_PB_Done;
            }
            else if (objPVLData.command.Equals(OpcTags.PVL_Put_PB))
            {
                commandType = 3;
                doneTag = OpcTags.PVL_Put_PB_Done;
            }
           
        }
        public bool DoTriggerAction(Model.PVLData objPVLData, int commandType)
        {
            bool success = false;
            if (commandType == 1)
            {
                success = PVLMove(objPVLData);
            }
            else if (commandType == 2)
            {
                success = PVLGet(objPVLData);
            }
            else if (commandType == 3)
            {
                success = PVLPut(objPVLData);
            }
           
            return success;

        }
        public bool CheckPVLHealthy(Model.PVLData objPVLData)
        {
            bool isHealthy = false;
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objPVLData.machineChannel + "." + objPVLData.machineCode + "." + OpcTags.PVL_Auto_Ready))
                {
                    isHealthy = opcd.ReadTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_Auto_Ready);
                    isHealthy = isHealthy && !objPVLDaoService.IsPVLDisabled(objPVLData.machineCode);
                    isHealthy = isHealthy && !objPVLDaoService.IsPVLSwitchOff(objPVLData.machineCode);

                }

            }
            return isHealthy;
        }

        public bool FindSlotAndPathForPVL(Model.PVLData objPVLData)
        {
            throw new NotImplementedException();
        }

        public bool TaskAfterPvlProcess(Model.PVLData objPVLData)
        {
            throw new NotImplementedException();
        }

        public void AsynchReadListenerForPVL(object sender, OPCDA.NET.RefreshEventArguments arg)
        {
            throw new NotImplementedException();
        }


        public bool UpdateMachineTagValueToDBFromListener(string machineCode, string machineTag, object dataValue)
        {
            throw new NotImplementedException();
        }

        public void GetDataTypeAndFieldOfTag(string opcTag, out int dataType, out string tableField, out bool isRem)
        {
            throw new NotImplementedException();
        }


        public bool IsPVLBlockedInDB(string machineName)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.IsPVLBlockedInDB(machineName);
        }

        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.UpdateMachineBlockStatus(machine_code, blockStatus);
        }

        public bool IsPVLDisabled(string machineName)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.IsPVLDisabled(machineName);
        }

        public bool IsPVLSwitchOff(string machineName)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.IsPVLSwitchOff(machineName);
        }



        public int FindPalletGettingSlotAndPath(Model.PVLData objPVLData)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.FindPalletGettingSlotAndPath(objPVLData);
        }

        public int FindPalletStoringSlotAndPath(Model.PVLData objPVLData)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            return objPVLDaoService.FindPalletStoringSlotAndPath(objPVLData);
        }


        public void UpdateAfterPVLTask(Model.PVLData objPVLData)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            objPVLDaoService.UpdateAfterPVLTask(objPVLData);
        }
        public void TaskAfterGetPalletBundle(int pathID)
        {
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            objPVLDaoService.TaskAfterGetPalletBundle(pathID);
        }
        public bool ConfirmPVLReadyPUTByCM(string machineCode)
        {
            bool isReady = false;
            Model.PVLData objPVLData = new Model.PVLData();
            objPVLData.machineCode = machineCode;
            if (objPVLDaoService == null) objPVLDaoService = new PVLDaoImp();
            objPVLData = objPVLDaoService.GetPVLDetails(objPVLData);
            isReady = IsPVLReadyForPUTByCM(objPVLData);
            return isReady;
        }
        public bool IsPVLReadyForPUTByCM(Model.PVLData objPVLData)
        {
            bool isReady = false;
            //string pvlDeck = GetDeckCodeOfPVL(objPVLData.machineCode);
            
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objPVLData.machineChannel + "." + objPVLData.machineCode + "." + OpcTags.PVL_Auto_Ready))
                {
                    isReady = !opcd.ReadTag<bool>(objPVLData.machineChannel, objPVLData.machineCode, OpcTags.PVL_Deck_Pallet_Present);
                    isReady = isReady && CheckPVLHealthy(objPVLData);
                   
                }

            }
            return isReady;
        }
        public string GetDeckCodeOfPVL(string pvlCode)
        {
            string pvlDeck = pvlCode.Replace("Drive", "Deck");
            return pvlDeck;
        }
    }
}
