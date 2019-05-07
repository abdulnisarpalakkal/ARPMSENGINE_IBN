using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;



namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller
{
    interface CMControllerService
    {
        List<CMData> GetCMList();
        bool CMMove(CMData objCMData);
        bool CMHomeMove(CMData objCMData);
        bool CMGet(CMData objCMData);
        bool CMPut(CMData objCMData);
        bool ConfirmGetPutAccepted(CMData objCMData);
        bool CMRotate(CMData objCMData);
        bool ConfirmRotateCmdAccepted(CMData objCMData);
        bool ClearPath(CMData objCMData);
        bool ClearNearestCM(CMData objCMData, CMData colCMData);
        bool ClearCrossReferenceCM(CMData objCMData, CMData colCMData, int col_ref_dest_aisle);
        int FindCollapseMachineAisleAndSide(Model.CMData objCMData, Model.CMData colCMData);
        bool ClearForRotaion(CMData objCMData);
        bool CheckCMCommandDone(CMData objCMData);
        bool CheckCMRotateDone(Model.CMData objCMData);
        string getREMCode(string machineCode);
        bool CheckCMHealthy(CMData objCMData);
        TriggerData NeedToShowTrigger(CMData objCMData);
        
        int GetCMAisle(CMData objCMData);
        bool CheckCMIsRotating(CMData objCMData);
        bool ConfirmCMRotationStatus(CMData objCMData);
        bool IsCMInIdeal(CMData objCMData);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool UpdateMachineBlockStatusForHome(string machine_code, bool blockStatus);

        
        bool UpdateCMIntData(string machineCode,string opcTag,int dataValue);
        bool UpdateCMBoolData(string machineCode, string opcTag, bool dataValue);
       
        

        void AsynchReadListenerForCM(object sender, RefreshEventArguments arg);

        bool UpdateCMClearRequest(string machineCode, string clearMachine, int clearFlag);
        bool UpdateCMClearPermission(string machineCode);
        bool GetCMClearRequest(string machineCode, out string clearingMachine, out int clearFlag);
        bool IsPalletPresentOnREM(CMData objCMData);
        bool IsBothRowAreInSameSide(int row1,int row2);
        bool IsRowInNorthSide(int row1);
       

        bool IsLeftCMSwitchedOff(CMData objCMData);
        bool IsRightCMSwitchedOff(CMData objCMData);
        bool GetPalletPresentStatus(CMData objCMData);
        bool GetPalletNotPresentStatus(CMData objCMData);
        ARCPMS_ENGINE.src.mrs.Global.GlobalValues.palletStatus GetPalletOnCMStatus(CMData objCMData);
        /// <summary>
        /// get cm details blocked by queueid
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        CMData GetBlockedCMDetails(Int64 queueId);
    }
}
