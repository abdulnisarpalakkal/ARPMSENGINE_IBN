using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Global
{
    public static class OpcTags
    {
        //For CM
        public const string CM_ClearError = "ClearError";
        //public const string CM_AT_NORTH = "";
        //public const string CM_AT_SOUTH = "";
        //public const string CM_Locked = "";

        public const string CM_Position_for_L2 = "Carrier_Aisle_Position_for_L2";
        public const string CM_EastCMOff = "East_CM_Off";
        public const string CM_WestCMOff = "West_CM_Off";


        public const string CM_L2_Block_CM = "L2_Block_CM";
        public const string CM_L2_CMD_DONE = "L2_CM_Idle";
        public const string CM_L2_MOVE_DONE = "L2_Command_Position_Done";
        public const string CM_L2_Destination_Row = "L2_Destination_Row";
        public const string CM_L2_Error_Data_Register = "L2_Error_Data_Register";

        public const string CM_L2_EStop = "L2_EStop";
        public const string CM_L2_Get_Cmd = "L2_Get_Cmd";
        public const string CM_L2_Max_Window_Limit = "L2_Max_Window_Limit";
        public const string CM_L2_Min_Window_Limit = "L2_Min_Window_Limit";

        public const string CM_L2_Move_Cmd = "L2_Move_Cmd";
        public const string CM_L2_Put_Cmd = "L2_Put_Cmd";
        public const string CM_Manual_Mode = "";
        public const string CM_Moving = "L2_CM_Moving";


        public const string CM_L2_Destination_Aisle = "L2_Destination_Aisle";
        public const string CM_Auto_Mode = "Auto_Mode";
        public const string CM_Pallet_Present_on_REM = "Pallet_Present_on_REM";
        public const string CM_L2_AUTO_READY = "L2_Auto_Ready_Bit";
        public const string CM_L2_CM_Idle = "L2_CM_Idle";
        public const string CM_REM_In_Home_Position = "REM_In_Home_Position";
        public const string CM_REM_North_Position = "North_Position";

        public const string CM_Pallet_Present = "Pallet_Present";
        public const string CM_Pallet_Not_Present = "Pallet_Not_Present";
        public const string CM_Progress_State = "Progress_State";


        //For UCM 
        public const string UCM_L2_AUTO_READY = "L2_Auto_Ready_Bit";
        //For LCM 

        public const string LCM_Clear_Error_CM = "Clear_Error_CM";
        public const string LCM_L2_ROT_FALSE_ALARM = "";
        public const string LCM_CM_At_Aisle = "CM_At_Aisle";


        //public const string LCM_L2_TT_ROT = "";
        public const string LCM_Rotate_to_0_from_L2 = "L2_TT_Rotate_to_0";
        public const string LCM_Rotate_to_180_from_L2 = "L2_TT_Rotate_to_180";
        //  public const string LCM_Rotate_to_Zero_Done                            = "Rotate_to_Zero_Done";

        //  public const string LCM_Rotate_to_180_Done                             = "Rotate_to_180_Done";
        public const string LCM_TT_at_0_Degrees = "TT_at_0";
        public const string LCM_TT_at_180_Degrees = "TT_at_180";



        //public const string LCM_L2_ROTATE_DONE = "";

        public const string LCM_L2_AUTO_READY = "L2_Auto_Ready_Bit";
        public const string LCM_TT_in_Position = "TT_in_Position";



        //VLC   
        public const string VLC_At_Floor = "At_Floor";
        public const string VLC_Auto_Mode = "Auto_Mode";
        public const string VLC_Auto_Ready = "Auto_Ready";
        public const string VLC_Axis_Error_Code = "";

        public const string VLC_CP_Done = "CP_Done";
        //public const string VLC_CP_Get_Start = "";
        //public const string VLC_CP_Put_Start = "";
        public const string VLC_CP_Start = "CP_Start";

        public const string VLC_DestFloor = "DestFloor";
        //public const string VLC_Get_Put_Done = "";
        public const string VLC_L2_ErrCode = "L2_ErrCode";
        //public const string VLC_Landed_Position_OK = "";

        //public const string VLC_Manual_Mode = "";
        //public const string VLC_No_Current_Error = "";

        //EES  

        public const string EES_LOCKED_BY_REM = "Locked_by_REM";//rem locked
        public const string EES_Ready_for_REM_Lock = "Ready_for_REM_Lock";
        public const string EES_LOC6KED_BY_PS = "Locked_by_PS"; //ps locked
        public const string EES_Ready_for_PS_Lock = "Ready_for_PS_Lock";

        public const string EES_Payment_Is_Done = "Amount_paid_from_L2";
        public const string EES_Pallet_Present_Prox_SW = "Pallet_Present_Prox_SW";
        public const string EES_Pallet_Present_Prox_NE = "Pallet_Present_Prox_NE";
        public const string EES_Outer_Door_Open_Con = "Outer_Door_Open_Con";

        public const string EES_Outer_Door_Close_Con = "Outer_Door_Close_Con";
        //public const string EES_Outer_Door_Block_Sensor = "";
        public const string EES_OutDoor_NotOpen_LS = "OutDoor_NotOpen_LS";
        public const string EES_OutDoor_NotClosed_LS = "OutDoor_NotClosed_LS";

        //public const string EES_No_Current_Error = "";
        public const string EES_Manual_Mode = "Manual_Mode";
        //public const string EES_L2_ErrCode = "";
        public const string EES_Inner_Door_Open_Con = "Inner_Door_Open_Con";

        public const string EES_Inner_Door_Close_Con = "Inner_Door_Close_Con";
        public const string EES_InDoor_NotOpen_LS = "InDoor_NotOpen_LS";
        public const string EES_InDoor_NotClosed_LS = "InDoor_NotClosed_LS";
        //public const string EES_Get_Car = "need to update";

        public const string EES_Sensors_Clear = "EES_Sensors_Clear";
        public const string EES_Mode = "EES_Mode";
        //public const string EES_Car_Simulaton = "";
        public const string EES_Car_Ready_To_Get = "L2_Car_OK_to_Store";

        //public const string EES_Car_Ready_At_Exit = "";
        public const string EES_Car_Ready_At_Entry = "L2_OK_to_GET";
        public const string EES_Car_At_EES = "Car_At_EES";
        //public const string EES_CAR_SENSE_ALARM = "";

        //public const string EES_CAR_AT_EES_ALARM = "";
        public const string EES_Auto_Ready = "Auto_Ready";
        public const string EES_Auto_Mode = "Auto_Mode";
        public const string EES_Lower_Height_Sensor_Blocked = "Lower_Height_Sensor_Blocked";

        public const string EES_L2_Error_Data_Register = "L2_Error_Data_Register";
        public const string EES_Auto_ID_Generator = "Auto_ID_Generator";
        public const string EES_L2_Change_Mode_OK = "L2_Change_Mode_OK";
        public const string EES_State = "State";

        //Camera OPC
        public const string EES_Cam_ImgPath = "ImgPath";
        public const string EES_Cam_GetCmd = "GetCmd";



        //PVL   
        //public const string PVL_StatusPvL02 = ""; //rem locked
        //public const string PVL_StatusPvl01 = "";
        public const string PVL_Request_Floor = "DestFloor";
        public const string PVL_Put_PB_Done = "Put_PB_Done";

        public const string PVL_Put_PB = "Put_PB";
        //public const string PVL_Pvl_Manual_Mode = "";
        public const string PVL_L2_ErrCode = "L2_ErrCode";
        //public const string PVL_Get_Put_Done = "";

        public const string PVL_Get_PB_Done = "Get_PB_Done";
        public const string PVL_Get_PB = "Get_PB";
        public const string PVL_Current_Floor = "At_Floor";
        public const string PVL_CP_Start = "CP_Start";

        //public const string PVL_CP_Put_Start = "";
        //public const string PVL_CP_Get_Start = "";
        public const string PVL_CP_Done = "CP_Done";
        public const string PVL_Auto_Ready = "AutoReady";

        public const string PVL_Auto_Mode = "Auto_Mode";
        public const string PVL_Deck_Pallet_Present = "Deck_Pallet_Present";

        //PST   
        public const string PST_South_Pallet_Sensor_5 = "South_Pallet_Sensor_5";
        public const string PST_South_Pallet_Sensor_4 = "South_Pallet_Sensor_4";
        public const string PST_South_Pallet_Sensor_3 = "South_Pallet_Sensor_3";
        public const string PST_South_Pallet_Sensor_2 = "South_Pallet_Sensor_2";
        public const string PST_South_Pallet_Sensor_1 = "South_Pallet_Sensor_1";

        public const string PST_North_Pallet_Sensor_5 = "North_Pallet_Sensor_5";
        public const string PST_North_Pallet_Sensor_4 = "North_Pallet_Sensor_4";
        public const string PST_North_Pallet_Sensor_3 = "North_Pallet_Sensor_3";
        public const string PST_North_Pallet_Sensor_2 = "North_Pallet_Sensor_2";
        public const string PST_North_Pallet_Sensor_1 = "North_Pallet_Sensor_1";

        public const string PST_L2_ErrCode = "L2_ErrCode";
        public const string PST_Auto_Ready = "AutoReady";
        public const string PST_Auto_Mode = "Auto_Mode";
        public const string PST_StackFull = "StackFull";

        public const string PST_StackEmpty = "StackEmpty";
        public const string PST_Pallet_Count = "Pallet_Count";

        //PS    
        public const string PS_West_Pallet_Present_Prox = "West_Pallet_Present_Prox";
        public const string PS_East_Pallet_Present_Prox = "East_Pallet_Present_Prox";
        public const string PS_Shuttle_Aisle_Position_for_L2 = "PS_At_Aisle";
        //public const string PS_Req_Unlock = "";

        public const string PS_Req_Put = "Req_Put";
        //public const string PS_Req_Lockout = "";
        public const string PS_Req_Get = "Req_Get";
        //public const string PS_PalletPresent = "";

        //public const string PS_NextPSOff = "";
        //public const string PS_Manual_Mode = "";
        //public const string PS_LockConfirmed = "";


        public const string PS_L2_PST_PutCmd = "L2_PST_PutCmd";
        public const string PS_L2_PST_GetCmd = "L2_PST_GetCmd";
        public const string PS_L2_MoveCmd = "L2_Move_CMD";
        public const string PS_L2_Min_Window_Limit = "L2_Min_Window_Limit";

        public const string PS_L2_Max_Window_Limit = "L2_Max_Window_Limit";
        //public const string PS_L2_GetCmd = "";
        public const string PS_L2_Error_Data_Register = "L2_Error_Data_Register";
        public const string PS_L2_EStop = "L2_EStop";

        public const string PS_L2_EES_PutCmd = "L2_EES_PutCmd";
        public const string PS_L2_EES_GetCmd = "L2_EES_GetCmd";
        public const string PS_L2_Destination_Aisle = "L2_Destination_Aisle";
        //public const string PS_L2_CMD_DONE_ALM = "";

        //public const string PS_L2_CMD_DONE = "";
        //public const string PS_L2_Block_PS = "";
        public const string PS_L2_Auto_Ready_Bit = "L2_Auto_Ready_Bit";
        public const string PS_Auto_Mode = "Auto_Mode";

        public const string PS_L2_PST_PUT_DONE = "L2_PST_Put_Done";
        public const string PS_L2_PST_GET_DONE = "L2_PST_Get_Done";
        public const string PS_L2_EES_PUT_DONE = "L2_EES_Put_Done";
        public const string PS_L2_EES_GET_DONE = "L2_EES_Get_Done";

        public const string PS_L2_CMD_POSITION_DONE = "L2_Command_Position_Done";


    }
}
