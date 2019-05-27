using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.DB;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using ARCPMS_ENGINE.src.mrs.Global;
using OPC;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Controller
{
    class PSControllerImp : CommonServicesForMachines,PSControllerService
    {
        PSDaoService objPSDaoService = null;
        PSTControllerService objPSTControllerService = null;
        OPCDA.NET.RefreshGroup uGrp;
        int DAUpdateRate = 1;
        Thread updateDataFromOpcListener = null;
        ErrorControllerService objErrorControllerService = null;
        ErrorDaoService objErrorDaoService = null;

        public List<Model.PSData> GetPSList()
        {
            if (objPSDaoService==null) objPSDaoService = new PSDaoImp();
            return objPSDaoService.GetPSList();
        }

        public Model.PSData GetPSDetailsIncludeAisle(int aisle)
        {
            if (objPSDaoService == null) objPSDaoService = new PSDaoImp();
            return objPSDaoService.GetPSDetailsIncludeAisle(aisle);
        }

        public bool UpdateMachineValues()
        {
            objPSDaoService = new PSDaoImp();
            List<Model.PSData> psList;
            bool result;
            int dataValue;
            bool dataValueInBool;



            try
            {
                psList = objPSDaoService.GetPSList();
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    foreach (Model.PSData objPSData in psList)
                    {

                        if (opcd.IsMachineHealthy(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_Auto_Mode) == true)
                        {

                            dataValue = opcd.ReadTag<Int16>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                            if (dataValue > 0) UpdatePSIntData(objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2, dataValue);
                            dataValue = opcd.ReadTag<Int16>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Max_Window_Limit);
                            if (dataValue > 0) UpdatePSIntData(objPSData.machineCode, OpcTags.PS_L2_Max_Window_Limit, dataValue);
                            dataValue = opcd.ReadTag<Int16>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Min_Window_Limit);
                            if (dataValue > 0) UpdatePSIntData(objPSData.machineCode, OpcTags.PS_L2_Min_Window_Limit, dataValue);



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
            OPCDA.NET.RefreshEventHandler dch = new OPCDA.NET.RefreshEventHandler(AsynchReadListenerForPS);
            uGrp = new OPCDA.NET.RefreshGroup(OpcConnection.GetOPCServerConnection(), DAUpdateRate, dch);

            int rtc = 0;

            PSDaoService objPSDaoService = new PSDaoImp();
            List<Model.PSData> psList = objPSDaoService.GetPSList();


            try
            {

                foreach (Model.PSData objPSData in psList)
                {
                    rtc = uGrp.Add(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    rtc = uGrp.Add(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_L2_Max_Window_Limit);
                    rtc = uGrp.Add(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_L2_Min_Window_Limit);
                   


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
            objPSDaoService = new PSDaoImp();
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
          
            GetDataTypeAndFieldOfTag(machineTag, out dataType, out field, out isRem);
            
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objPSDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    objPSDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objPSDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            
            return true;
        }


        public void GetDataTypeAndFieldOfTag(string opcTag, out int dataType, out string tableField, out bool isRem)
        {
            isRem = false;
            tableField = "";
            dataType = 0;//1:bool;2:number;3:string
            if (opcTag.Equals(OpcTags.PS_Shuttle_Aisle_Position_for_L2))
            {
                tableField = "DYNAMIC_HOME";
                dataType = 2;
            }
            else if (opcTag.Equals(OpcTags.PS_L2_Max_Window_Limit))
            {
                tableField = "DYNAMIC_MAX";
                dataType = 2;
            }
            else if (opcTag.Equals(OpcTags.PS_L2_Min_Window_Limit))
            {
                tableField = "DYNAMIC_MIN";
                dataType = 2;
            }
        }


        public bool PSMove(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering PSMove: " + objPSData.machineCode+" >> dest_aisle: "+objPSData.destAisle);
            bool isPSHealthy = false;
            bool success = false;
            bool isPathClear = false;


            //do
            //{
            try
            {

                isPSHealthy = CheckPSHealthy(objPSData);

                if (!isPSHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objPSData.dynamicHome = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    if (objPSData.dynamicHome != objPSData.destAisle)
                    {
                        //isPathClear = ClearPathForPS(objPSData);
                        //if (isPathClear)
                        //{
                            opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle, objPSData.destAisle);

                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, objPSData.command, true);
                        //}
                    }
                    else
                    {
                        success = true;
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in PSMove: " + objPSData.machineCode
                + " >> dest_aisle: " + objPSData.destAisle + "; error: " + ex.Message);
                success = false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PSMove: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            }


            //} while (!success);
                
            return success;
        }

        public bool PSGetFromEES(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering PSGetFromEES: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            bool isPSHealthy = false;
            bool success = false;
            bool isPathClear = false;
            int checkCount = 0;
            int setAisle = 0;


          //  do
          //  {
            try
            {

                isPSHealthy = CheckPSHealthy(objPSData);

                if (!isPSHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objPSData.dynamicHome = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    if (objPSData.dynamicHome != objPSData.destAisle)
                    {
                        isPathClear = true;// ClearPathForPS(objPSData);

                    }
                    else
                        isPathClear = true;

                    if (isPathClear)
                    {
                        do
                        {
                            opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle, objPSData.destAisle);
                            setAisle = opcd.ReadTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle);
                            checkCount++;
                        } while (objPSData.destAisle != setAisle && checkCount < 5);
                        if (objPSData.destAisle == setAisle)
                        {
                            opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_MoveCmd, true);
                            opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, objPSData.command, true);
                            success = true;
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in PSGetFromEES: " + objPSData.machineCode
                + " >> dest_aisle: " + objPSData.destAisle + "; error: " + ex.Message);
                success = false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PSGetFromEES: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            }


            //} while (!success);
            return success;
        }

        public bool PSGetFromPST(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering PSGetFromPST: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            bool isPSHealthy = false;
            bool success = false;
            bool isPathClear = false;
            int checkCount = 0;
            int setAisle = 0;

            //do
            //{
            try
            {

                isPSHealthy = CheckPSHealthy(objPSData);

                if (!isPSHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objPSData.dynamicHome = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    if (objPSData.dynamicHome != objPSData.destAisle)
                    {
                        isPathClear = true;// ClearPathForPS(objPSData);

                    }
                    else
                        isPathClear = true;

                    if (isPathClear)
                    {
                        do
                        {
                            opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle, objPSData.destAisle);
                            setAisle = opcd.ReadTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle);
                            checkCount++;
                        } while (objPSData.destAisle != setAisle && checkCount < 5);

                        if (objPSData.destAisle == setAisle)
                        {
                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_MoveCmd, true);
                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, objPSData.command, true);
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in PSGetFromPST: " + objPSData.machineCode
                 + " >> dest_aisle: " + objPSData.destAisle + "; error: " + ex.Message);
                success = false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PSGetFromPST: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            }


            //} while (!success);
            return success;
        }

        public bool PSPutToEES(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering PSPutToEES: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            bool isPSHealthy = false;
            bool success = false;
            bool isPathClear = false;
            int checkCount = 0;
            int setAisle = 0;

            //do
            //{
            try
            {

                isPSHealthy = CheckPSHealthy(objPSData);

                if (!isPSHealthy) return false;



                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objPSData.dynamicHome = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    if (objPSData.dynamicHome != objPSData.destAisle)
                    {
                        isPathClear = ClearPathForPS(objPSData);

                    }
                    else
                        isPathClear = true;

                    if (isPathClear)
                    {
                        do
                        {
                            opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle, objPSData.destAisle);
                            setAisle = opcd.ReadTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle);
                            checkCount++;
                        } while (objPSData.destAisle != setAisle && checkCount < 5);

                        if (objPSData.destAisle == setAisle)
                        {
                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_MoveCmd, true);
                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, objPSData.command, true);
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in PSPutToEES: " + objPSData.machineCode
                  + " >> dest_aisle: " + objPSData.destAisle + "; error: " + ex.Message);
                success = false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PSPutToEES: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            }


            //} while (!success);
            return success;
        }

        public bool PSPutToPST(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering PSPutToPST: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            bool isPSHealthy = false;
            bool success = false;
            bool isPathClear = false;
            int checkCount = 0;
            int setAisle = 0;

            //do
            //{
            try
            {

                isPSHealthy = CheckPSHealthy(objPSData);

                if (!isPSHealthy) return false;

                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {
                    objPSData.dynamicHome = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    if (objPSData.dynamicHome != objPSData.destAisle)
                    {
                        isPathClear = true;// ClearPathForPS(objPSData);

                    }
                    else
                        isPathClear = true;

                    if (isPathClear)
                    {

                        do
                        {
                            opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle, objPSData.destAisle);
                            setAisle = opcd.ReadTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Destination_Aisle);
                            checkCount++;
                        } while (objPSData.destAisle != setAisle && checkCount < 5);

                        if (objPSData.destAisle == setAisle)
                            success = opcd.WriteTag<bool>(objPSData.machineChannel, objPSData.machineCode, objPSData.command, true);
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in PSPutToPST: " + objPSData.machineCode
                   + " >> dest_aisle: " + objPSData.destAisle + "; error: " + ex.Message);
                success = false;
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PSPutToPST: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            }

            //} while (!success);
            return success;
        }

      

        public bool CheckPSCommandDone(Model.PSData objPSData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entering CheckPSCommandDone: " + objPSData.machineCode + " >> dest_aisle: " + objPSData.destAisle);
            bool result = false;

            int counter = 1;
            OpcOperationsService opcd = null;
        
            int commandType = 0;
            string doneCheckTag = null;
            int error = 0;
            if (objErrorControllerService == null)
                objErrorControllerService = new ErrorControllerImp();
            if (objErrorDaoService == null)
                objErrorDaoService = new ErrorDaoImp();

            try
            {
                opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                Thread.Sleep(3000);
                result = false;
                FindCommandTypeAndDoneTag(objPSData,out commandType,out doneCheckTag);
               
                
                do
                {
                    error = objErrorControllerService.GetErrorCode(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Error_Data_Register);
                    if (error > 0)
                    {

                        TriggerData objTriggerData = new TriggerData();
                        objTriggerData.MachineCode = objPSData.machineCode;
                        objTriggerData.category = TriggerData.triggerCategory.ERROR;
                        objTriggerData.ErrorCode = error;
                        objTriggerData.TriggerEnabled = true;
                        objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);


                        while (objErrorControllerService.GetTriggerActiveStatus(objPSData.machineCode))
                        {



                            Thread.Sleep(1000);
                        }
                        if (objErrorControllerService.GetTriggerAction(objPSData.machineCode) == 1)
                        {
                            DoTriggerAction(objPSData,commandType);
                            
                            Thread.Sleep(2000);
                        }

                    }

                    result = opcd.ReadTag<bool>(objPSData.machineChannel, objPSData.machineCode, doneCheckTag);

                    if (counter > 3) Thread.Sleep(700);
                    counter += 1;
                } while (!result);
            }
            catch (Exception errMsg)
            {
               
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Error in CheckPSCommandDone: " + objPSData.machineCode
                    + " >> dest_aisle: " + objPSData.destAisle + "; error: " + errMsg.Message);
            }
            finally
            {

                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting CheckPSCommandDone: " + objPSData.machineCode + " >> dest_aisle: "
                    + objPSData.destAisle + ", result =" + result);
                if (opcd != null) opcd.Dispose();
            }
            return result;
        }

        public bool ClearPathForPS(Model.PSData objPSData)
        {
            bool pathClear = true;
            if(objPSTControllerService==null)
                objPSTControllerService=new PSTControllerImp();

            PSTData pst=objPSTControllerService.GetPSTDetailsInRange(objPSData.dynamicHome, objPSData.destAisle);
            if (pst != null && pst.machineCode!=null)
                pathClear=!objPSTControllerService.IsPSTBlockedInDB(pst.machineCode);
            //TODO: implement the logic
            return pathClear;
        }

        public bool ClearNearestPS(Model.PSData objPSData)
        {
            throw new NotImplementedException();
        }

        public bool CheckPSHealthy(Model.PSData objPSData)
        {
            bool isHealthy = false;
            objPSDaoService = new PSDaoImp();

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_Shuttle_Aisle_Position_for_L2))
                {
                    isHealthy = opcd.ReadTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Auto_Ready_Bit);
                    isHealthy = isHealthy && !objPSDaoService.IsPSDisabled(objPSData.machineCode);
                    isHealthy = isHealthy && !objPSDaoService.IsPSSwitchOff(objPSData.machineCode);
                  
                }

            }
            return isHealthy;
        }

        public void AsynchReadListenerForPS(object sender, OPCDA.NET.RefreshEventArguments arg)
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


        public bool UpdatePSIntData(string machineCode, string opcTag, int dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;
            objPSDaoService = new PSDaoImp();

            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);
           
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objPSDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    objPSDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objPSDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            
            return true;
        }

        public bool UpdatePSBoolData(string machineCode, string opcTag, bool dataValue)
        {
            int dataType = 0; //1:bool;2:number;3:string
            bool isRem = false;
            string field = "";
            bool boolDataValue;
            int intDataValue;
            string stringDataValue;

            objPSDaoService = new PSDaoImp();
            GetDataTypeAndFieldOfTag(opcTag, out dataType, out field, out isRem);
           
                if (dataType == 1)
                {
                    boolDataValue = Convert.ToBoolean(dataValue);
                    objPSDaoService.UpdateBoolValueUsingMachineCode(machineCode, field, boolDataValue);
                }
                else if (dataType == 2)
                {
                    intDataValue = Convert.ToInt16(dataValue);
                    objPSDaoService.UpdateIntValueUsingMachineCode(machineCode, field, intDataValue);
                }
                else if (dataType == 3)
                {
                    stringDataValue = Convert.ToString(dataValue);
                    objPSDaoService.UpdateStringValueUsingMachineCode(machineCode, field, stringDataValue);
                }
            
            return true;
        }



        public void FindCommandTypeAndDoneTag(Model.PSData objPSData, out int commandType, out string doneTag)
        {
            commandType = 0;
            doneTag = null;
            if (objPSData.command.Equals(OpcTags.PS_L2_MoveCmd))
            {
                commandType = 1;
                doneTag = OpcTags.PS_L2_CMD_POSITION_DONE;
            }
            else if (objPSData.command.Equals(OpcTags.PS_L2_EES_GetCmd))
            {
                commandType = 2;
                doneTag = OpcTags.PS_L2_EES_GET_DONE;
            }
            else if (objPSData.command.Equals(OpcTags.PS_L2_EES_PutCmd))
            {
                commandType = 3;
                doneTag = OpcTags.PS_L2_EES_PUT_DONE;
            }
            else if (objPSData.command.Equals(OpcTags.PS_L2_PST_GetCmd))
            {
                commandType = 4;
                doneTag = OpcTags.PS_L2_PST_GET_DONE;
            }
            else if (objPSData.command.Equals(OpcTags.PS_L2_PST_PutCmd))
            {
                commandType = 5;
                doneTag = OpcTags.PS_L2_PST_PUT_DONE;
            }
        }

        public bool DoTriggerAction(Model.PSData objPSData, int commandType)
        {
            bool success = false;
            if (commandType == 1)
            {
                success=PSMove(objPSData);
            }
            else if (commandType == 2)
            {
                success=PSGetFromEES(objPSData);
            }
            else if (commandType == 3)
            {
                success=PSPutToEES(objPSData);
            }
            else if (commandType == 4)
            {
                success=PSGetFromPST(objPSData);
            }
            else if (commandType == 5)
            {
                success=PSPutToPST(objPSData);
            }
            return success;

        }


        public bool IsPSBlockedInDB(string machineName)
        {
            objPSDaoService = new PSDaoImp();
            return objPSDaoService.IsPSBlockedInDB(machineName);
        }

        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            objPSDaoService = new PSDaoImp();
            return objPSDaoService.UpdateMachineBlockStatus(machine_code, blockStatus);
        }

        public bool IsPSDisabled(string machineName)
        {
            objPSDaoService = new PSDaoImp();
            return objPSDaoService.IsPSDisabled(machineName);
        }
        public bool IsPSSwitchOff(string machineName)
        {
            objPSDaoService = new PSDaoImp();
            return objPSDaoService.IsPSSwitchOff(machineName);
        }


        

        public bool IsPalletPresentOnPS(Model.PSData objPSData,out bool hasPSCommunication)
        {
            bool isPresent = false;
            int healthCheckCount = 0;
            hasPSCommunication=false;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                while (healthCheckCount < 3)
                {
                    hasPSCommunication=opcd.IsMachineHealthy(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_East_Pallet_Present_Prox);
                    if (hasPSCommunication)
                    {
                        isPresent = opcd.ReadTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_East_Pallet_Present_Prox);
                        isPresent = isPresent || opcd.ReadTag<bool>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_West_Pallet_Present_Prox);
                        break;

                    }
                    healthCheckCount++;
                }

            }
            return isPresent;
        }

        public int GetAisleOfPS(Model.PSData objPSData)
        {
            int aisle = 0;

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objPSData.machineChannel + "." + objPSData.machineCode + "." + OpcTags.PS_Shuttle_Aisle_Position_for_L2))
                {
                    aisle = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_Shuttle_Aisle_Position_for_L2);
                    
                }

            }
            return aisle;
        }
        

      
    }
}
