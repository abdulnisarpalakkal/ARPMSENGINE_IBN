using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.DB;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using ARCPMS_ENGINE.src.mrs.Global;
using OPC;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller
{
    class PSTControllerImp : CommonServicesForMachines,PSTControllerService
    {
        PSTDaoService objPSTDaoService = null;
        ParkingControllerService objParkingControllerService = null;


        public List<Model.PSTData> GetPSTList()
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.GetPSTList();
        }
        public Model.PSTData GetPSTDetails(Model.PSTData objPSTData)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.GetPSTDetails(objPSTData);
        }
        public Model.PSTData GetPSTDetailsInRange(int minAisle, int maxAisle)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.GetPSTDetailsInRange(minAisle,maxAisle);
        }
       

        public bool UpdateMachineValues()
        {
            throw new NotImplementedException();
        }


        public bool AsynchReadSettings()
        {
            throw new NotImplementedException();
        }

        public int GetCountOfPallet(Model.PSTData objPSTData)
        {
            int palletCount = -1;
            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                palletCount = opcd.ReadTag<Int32>(objPSTData.machineChannel, objPSTData.machineCode, OpcTags.PST_Pallet_Count);
            }
            return palletCount;
        }

        
        public bool CheckPSTHealthy(Model.PSTData objPSTData)
        {
            bool isHealthy = false;
            if (objPSTDaoService==null ) objPSTDaoService = new PSTDaoImp();

            using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
            {
                if (opcd.IsMachineHealthy(objPSTData.machineChannel+"."+objPSTData.machineCode+"."+OpcTags.PST_Auto_Ready))
                {
                    isHealthy = opcd.ReadTag<bool>(objPSTData.machineChannel, objPSTData.machineCode, OpcTags.PST_Auto_Ready);
                    isHealthy = isHealthy && !objPSTDaoService.IsPSTDisabled(objPSTData.machineCode);
                    isHealthy = isHealthy && !objPSTDaoService.IsPSTSwitchOff(objPSTData.machineCode);

                }

            }
            return isHealthy;

        }

        public void AsynchReadListenerForPST(object sender, OPCDA.NET.RefreshEventArguments arg)
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


        public bool IsPSTBlockedInDB(string machineName)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.IsPSTBlockedInDB(machineName);

        }

        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.UpdateMachineBlockStatus(machine_code, blockStatus);
        }

        public bool IsPSTDisabled(string machineName)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.IsPSTDisabled(machineName);
        }

        public bool IsPSTSwitchOff(string machineName)
        {
            if (objPSTDaoService == null) objPSTDaoService = new PSTDaoImp();
            return objPSTDaoService.IsPSTSwitchOff(machineName);
        }
       
    }
}
