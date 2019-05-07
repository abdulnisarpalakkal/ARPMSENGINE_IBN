using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB;
//using OPC;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller
{
    class ErrorControllerImp:ErrorControllerService
    {
        ErrorDaoService objErrorDaoService = null;
        object triggerUpdateLock = new object();
        public int GetErrorCode(string channel, string machine, string errorRegister)
        {
            int errorCode = 0;


            try
            {
                using (OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection()))
                {

                    errorCode = opcd.ReadTag<Int32>(channel, machine, errorRegister);
                }
            }
            catch (Exception errmsg)
            {
                Console.WriteLine(errmsg);
            }
            finally
            {

            }
            return errorCode;
        }
        public bool UpdateLiveCommandOfMachine(Model.ErrorData objErrorData)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.UpdateLiveCommandOfMachine(objErrorData);
        }
        public bool UpdateLiveCommandOfCM(Model.ErrorData objErrorData)
        {
            
            bool isValidate = false;
            bool isUpdated = false;
           
            lock (triggerUpdateLock)
            {

                isValidate = validate_live_command_update(objErrorData);
                if (isValidate)
                {
                    isUpdated = UpdateLiveCommandOfMachine(objErrorData);
                }
            }
            return isUpdated;
        }

        public bool UpdateLiveCommandStatusOfMachine(string machine, bool isDone)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.UpdateLiveCommandStatusOfMachine(machine,isDone); 
        }
        public Model.ErrorData GetLiveCommandOfMachine(string machine)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.GetLiveCommandOfMachine(machine);

        }

        public bool UpdateTriggerActiveStatus(TriggerData objTriggerData)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.UpdateTriggerActiveStatus(objTriggerData);
        }

        public bool GetTriggerActiveStatus(string machine)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.GetTriggerActiveStatus(machine);
        }

        public int GetTriggerAction(string machine)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.GetTriggerAction(machine);
        }

        public Model.ErrorData GetCommandOfActiveTrigger(string machine)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.GetCommandOfActiveTrigger(machine);
        }


        public bool DeleteTransaction(int queueId)
        {
            throw new NotImplementedException();
        }

        public bool CompleteTransaction(int queueId)
        {
            throw new NotImplementedException();
        }
        public bool validate_live_command_update(Model.ErrorData objErrorData)
        {
            if (objErrorDaoService == null) objErrorDaoService = new ErrorDaoImp();
            return objErrorDaoService.validate_live_command_update(objErrorData);
        }

       
    }
}
