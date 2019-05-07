using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller
{
    interface ErrorControllerService
    {

        /// <summary>
        /// Get error code of machine
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="machine"></param>
        /// <param name="errorRegister"></param>
        /// <returns></returns>
        int GetErrorCode(string channel,string machine,string errorRegister);
        /// <summary>
        /// update command without validation of all machines
        /// </summary>
        /// <param name="objErrorData"></param>
        /// <returns></returns>
        bool UpdateLiveCommandOfMachine(Model.ErrorData objErrorData);
        /// <summary>
        /// update command of CM after validation
        /// </summary>
        /// <param name="objErrorData"></param>
        /// <returns></returns>
        bool UpdateLiveCommandOfCM(Model.ErrorData objErrorData);
        /// <summary>
        /// update executing command status whether it is finished or not
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="isDone"></param>
        /// <returns></returns>
        bool UpdateLiveCommandStatusOfMachine(string machine, bool isDone);
        /// <summary>
        /// get excecuting command of machine
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        Model.ErrorData GetLiveCommandOfMachine(string machine);
        /// <summary>
        /// update trigger status
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        bool UpdateTriggerActiveStatus(TriggerData objTriggerData);
        /// <summary>
        /// get trigger status
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        bool GetTriggerActiveStatus(string machine);
        /// <summary>
        /// get trigger action, trigger or unlock trigger
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        int GetTriggerAction(string machine);
        /// <summary>
        /// get command of active trigger of machine
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        Model.ErrorData GetCommandOfActiveTrigger(string machine);
        /// <summary>
        /// delete transaction
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool DeleteTransaction(int queueId);
        /// <summary>
        /// complete transaction
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        bool CompleteTransaction(int queueId);
        /// <summary>
        /// validate command to be excecuted
        /// </summary>
        /// <param name="objErrorData"></param>
        /// <returns></returns>
        bool validate_live_command_update(Model.ErrorData objErrorData);
    }
}
