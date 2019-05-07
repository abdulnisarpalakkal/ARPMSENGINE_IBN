using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ARCPMS_ENGINE.src.mrs.Global
{
    public static class GlobalValues
    {
        //CONSTANTS
        public static readonly int EESEntry             = 1;
        public static readonly int EESExit              = 2;

        public static readonly int MORNING_MODE         = 1;
        public static readonly int EVENING_MODE         = 2;
        public static readonly int NORMAL_MODE          = 3;

        public enum CAR_TYPE
        {
            low = 1,
            high = 2,
            medium = 3
        };


        public static readonly string PARKING_LOG       = "parking";

        public static readonly string PMS_LOG           = "pms";
        public static readonly string TRANSFER_LOG      = "transfer";
        public static Dictionary<int, CancellationTokenSource> threadsDictionary = new Dictionary<int, CancellationTokenSource>();



        //GLOBAL VARIABLES FROM CONFIG XML
        public static string GLOBAL_DB_CON_STRING = "";
        public static string GLOBAL_ENTRY_XML_PATH = "";
        public static string GLOBAL_EXIT_XML_PATH = "";
        public static string GLOBAL_BACKUP_XML_PATH = "";

        public static string GLOBAL_DISPLAY_XML_PATH = "";

        public static string REPORT_PATH = "";
        public static string OPC_MACHINE_HOST = "";
        public static string OPC_SERVER_NAME = "";

        public static string CAM_HOST_SERVER = "";
        public static string CAM_OPC_SERVER_NAME = "";
        public static string LOG_DIR = "";
        public static string IMAGE_PATH = "";
        
        public static bool AUTO_REFRESH = false;
        public static bool PARKING_ENABLED= false;
        public static bool PMS_ENABLED = false;

        public enum palletStatus
        {
            present = 1,
            notPresent = 2,
            notValid = 0
        }
        public enum engineStartMode
        {
            restart=1,
            resume=2
        }
        public enum vlcMode
        {
            ENTRY = 1,
            EXIT = 2,
            MIXED = 0
        }

        public enum REQUEST_TYPE
        {
            SlotToPVL = 7,
            PVLToSlot = 8
        }


    }
}
