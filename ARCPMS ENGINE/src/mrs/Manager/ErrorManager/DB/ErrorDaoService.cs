using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB
{
    interface ErrorDaoService
    {
        /// <summary>
        /// update excecuting command
        /// </summary>
        /// <param name="objErrorData"></param>
        /// <returns></returns>
        bool UpdateLiveCommandOfMachine(Model.ErrorData objErrorData);
        /// <summary>
        /// update command completed status
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="isDone"></param>
        /// <returns></returns>
        bool UpdateLiveCommandStatusOfMachine(string machine, bool isDone);
        /// <summary>
        /// get excecuting command details
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        Model.ErrorData GetLiveCommandOfMachine(String machine);
        /// <summary>
        /// 18Oct18
        /// </summary>
        /// <param name="objTriggerData"></param>
        /// <returns></returns>
        bool UpdateTriggerActiveStatus(TriggerData objTriggerData);
        /// <summary>
        /// get trigger status
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        bool GetTriggerActiveStatus(String machine);
        /// <summary>
        /// get trigger action
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        int GetTriggerAction(string machine);
        /// <summary>
        /// get command of trigger
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        Model.ErrorData GetCommandOfActiveTrigger(String machine);
        /// <summary>
        /// call procedure for validating command to be excecuted
        /// </summary>
        /// <param name="objErrorData"></param>
        /// <returns></returns>
        bool validate_live_command_update(Model.ErrorData objErrorData);
        
    }
}
