using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;
namespace ARCPMS_ENGINE.src.mrs.Manager.PalletManager.DB
{
    interface PalletDaoService
    {
        /// <summary>
        /// Check whether PMS is enabled in L2
        /// </summary>
        /// <returns></returns>
        bool IsPMSInL2();
        /// <summary>
        /// Get Current PMS mode
        /// </summary>
        /// <returns></returns>
        int GetCurrentPMSMode();
        /// <summary>
        /// Get  available nearest PS for EES
        /// </summary>
        /// <param name="objEESData"></param>
        /// <returns></returns>
        PSData FindPSForEES(EESData objEESData);
        /// <summary>
        /// Get available nearest PST for PS
        /// </summary>
        /// <param name="objPSData"></param>
        /// <returns></returns>
        PSTData FindPSTForPS(PSData objPSData);
        /// <summary>
        /// Get PVL for PST
        /// </summary>
        /// <param name="objPSTData"></param>
        /// <returns></returns>
        PVLData FindPVLForPST(PSTData objPSTData);
    }
}
