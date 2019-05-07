using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.PalletManager.Controller
{
    interface PalletManagerService
    {
        /// <summary>
        /// change mode acccording to user selection from GUI
        /// </summary>
        void StartModeScanning();
        /// <summary>
        /// Scanning for PMS Job
        /// </summary>
        void StartPMSProcessing();
        /// <summary>
        /// Scanning EES for Feed or remove pallet
        /// </summary>
        /// <param name="eesList"></param>
        void StartEESScanning(List<EESData> eesList);
        /// <summary>
        /// Scanning  PS for  EES or PST processing
        /// </summary>
        void StartPSScanning();
        /// <summary>
        /// Scanning PST for storing or removing Pallet bundle
        /// </summary>
        void StartPSTScanning();
        /// <summary>
        /// scanning PVL for storing or removing pallet Bundle
        /// </summary>
        void StartPVLScanning();
        
        /// <summary>
        /// Job for putting pallet to EES using PS
        /// </summary>
        /// <param name="objPSData"></param>
        /// <param name="objEESData"></param>
        /// <returns></returns>
        bool PutEESByPS(PSData objPSData, EESData objEESData);
        /// <summary>
        /// Job for getting pallet from EES using PS
        /// </summary>
        /// <param name="objPSData"></param>
        /// <param name="objEESData"></param>
        /// <returns></returns>
        bool GetEESByPS(PSData objPSData, EESData objEESData);
        /// <summary>
        /// Job for putting pallet to PST using PS
        /// </summary>
        /// <param name="objPSData"></param>
        /// <param name="objPSTData"></param>
        /// <returns></returns>
        bool PutPSTByPS(PSData objPSData, PSTData objPSTData);
        /// <summary>
        /// Job for getting pallet from PST using PS
        /// </summary>
        /// <param name="objPSData"></param>
        /// <param name="objPSTData"></param>
        /// <returns></returns>
        bool GetPSTByPS(PSData objPSData, PSTData objPSTData);
        /// <summary>
        /// Job for putting pallet bundle to PST using PVL
        /// </summary>
        /// <param name="objPVLData"></param>
        /// <param name="objPSTData"></param>
        /// <param name="objPSData"></param>
        /// <returns></returns>
        bool PutPSTByPVL(PVLData objPVLData, PSTData objPSTData, PSData objPSData);
        /// <summary>
        ///  Job for getting pallet bundle from PST using PVL
        /// </summary>
        /// <param name="objPVLData"></param>
        /// <param name="objPSTData"></param>
        /// <param name="objPSData"></param>
        /// <returns></returns>
        bool GetPSTByPVL(PVLData objPVLData, PSTData objPSTData, PSData objPSData);
        /// <summary>
        ///  Job for putting pallet bundle to PVL using UCM
        /// </summary>
        /// <param name="objPVLData"></param>
        /// <returns></returns>
        bool PutPVLByUCM(PVLData objPVLData);
        /// <summary>
        ///  Job for getting pallet bundle to PVL using UCM
        /// </summary>
        /// <param name="objPVLData"></param>
        /// <returns></returns>
        bool GetPVLByUCM(PVLData objPVLData);
      
        /// <summary>
        /// Move Pallet shuttle from under the PVL
        /// </summary>
        /// <param name="objPSData"></param>
        /// <returns></returns>
        bool ClearPSFromUnderPST( PSData objPSData);

        /// <summary>
        /// Lock PS as it should not lock by any other thread at same time
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        bool CriticalSectionForPSLocking(string machineCode);
        /// <summary>
        /// Lock PST as it should not lock by any other thread at same time
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        bool CriticalSectionForPSTLocking(string machineCode);
        /// <summary>
        /// Lock EES as it should not lock by any other thread at same time
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        bool CriticalSectionForEESLocking(string machineCode);
        /// <summary>
        /// Lock PVL as it should not lock by any other thread at same time
        /// </summary>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        bool CriticalSectionForPVLLocking(string machineCode);
        /// <summary>
        /// Stop all scanning threads
        /// </summary>
        void RequestStop();
        /// <summary>
        /// Get current PMS Mode of operation
        /// </summary>
        /// <returns></returns>
        int GetCurrentPMSMode();
        /// <summary>
        /// Scanning PS for moving home position: not implemented completely
        /// </summary>
        void HomePositionMoveTrigger();

    }
}
