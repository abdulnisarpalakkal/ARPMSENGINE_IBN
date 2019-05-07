using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;
using Oracle.DataAccess.Client;
using ARCPMS_ENGINE.src.mrs.DBCon;
using System.Data;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.DB
{
    class EESDaoImp : EESDaoService
    {
        public List<Model.EESData> GetEESList()
        {
            List<EESData> lstEESData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) 
                {
                    
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT EES_ID, EES_NAME,AISLE,F_ROW,START_AISLE,END_AISLE,MACHINE_CODE,COLLAPSE_START"
                                     + " ,COLLAPSE_END,IS_BLOCKED,MORNING_MODE,NORMAL_MODE,EVENING_MODE,STATUS,NORMAL_MIX_EES,IS_AUTOMODE,MACHINE_CHANNEL"
                                     + " FROM L2_EES_MASTER";
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstEESData = new List<EESData>();

                                while (reader.Read())
                                {

                                    EESData objEESData = new EESData();

                                    objEESData.eesPkId = Int32.Parse(reader["EES_ID"].ToString());
                                    objEESData.eesName = reader["EES_NAME"].ToString();
                                    objEESData.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    objEESData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objEESData.startAisle = Int32.Parse(reader["START_AISLE"].ToString());
                                    objEESData.endAisle = Int32.Parse(reader["END_AISLE"].ToString());
                                    objEESData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objEESData.collapseStart = Int32.Parse(reader["COLLAPSE_START"].ToString());

                                    objEESData.collapseEnd = Int32.Parse(reader["COLLAPSE_END"].ToString());
                                    objEESData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());
                                    objEESData.morningMode = Int32.Parse(reader["MORNING_MODE"].ToString());
                                    objEESData.normalMode = Int32.Parse(reader["NORMAL_MODE"].ToString());

                                    objEESData.eveningMode = Int32.Parse(reader["EVENING_MODE"].ToString());
                                    objEESData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objEESData.normalMixEES = Int32.Parse(reader["NORMAL_MIX_EES"].ToString());
                                    objEESData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objEESData.machineChannel = reader["MACHINE_CHANNEL"].ToString();


                                    lstEESData.Add(objEESData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return lstEESData;
        }

        public List<Model.EESData> GetEESListInRange(int minRange,int maxRange)
        {
            List<EESData> lstEESData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT EES_ID, EES_NAME,AISLE,F_ROW,MACHINE_CODE"
                                     + " ,IS_BLOCKED,STATUS,IS_AUTOMODE,MACHINE_CHANNEL"
                                     + " FROM L2_EES_MASTER"
                                     +" where aisle between " + minRange + " and "+maxRange;
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstEESData = new List<EESData>();

                                while (reader.Read())
                                {

                                    EESData objEESData = new EESData();

                                    objEESData.eesPkId = Int32.Parse(reader["EES_ID"].ToString());
                                    objEESData.eesName = reader["EES_NAME"].ToString();
                                    objEESData.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    objEESData.row = Int32.Parse(reader["F_ROW"].ToString());

                                   
                                    objEESData.machineCode = reader["MACHINE_CODE"].ToString();
                                   
                                    objEESData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());
                                   
                                    objEESData.status = Int32.Parse(reader["STATUS"].ToString());
                                    
                                    objEESData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objEESData.machineChannel = reader["MACHINE_CHANNEL"].ToString();


                                    lstEESData.Add(objEESData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return lstEESData;
        }

        public EESData GetEESDetails(EESData objEESData)
        {
            

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT EES_ID, EES_NAME,AISLE,F_ROW,START_AISLE,END_AISLE,MACHINE_CODE,COLLAPSE_START"
                                     + " ,COLLAPSE_END,IS_BLOCKED,MORNING_MODE,NORMAL_MODE,EVENING_MODE,STATUS,NORMAL_MIX_EES,IS_AUTOMODE,MACHINE_CHANNEL"
                                     + " FROM L2_EES_MASTER"
                                     + " WHERE ";
                        if(!string.IsNullOrEmpty(objEESData.machineCode) )
                                sql += " MACHINE_CODE='" + objEESData.machineCode+"'";
                        if (objEESData.eesPkId!=0)
                                sql += " EES_ID=" + objEESData.eesPkId;

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                              
                                while (reader.Read())
                                {

                                    objEESData.eesPkId = Int32.Parse(reader["EES_ID"].ToString());
                                    objEESData.eesName = reader["EES_NAME"].ToString();
                                    objEESData.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    objEESData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objEESData.startAisle = Int32.Parse(reader["START_AISLE"].ToString());
                                    objEESData.endAisle = Int32.Parse(reader["END_AISLE"].ToString());
                                    objEESData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objEESData.collapseStart = Int32.Parse(reader["COLLAPSE_START"].ToString());

                                    objEESData.collapseEnd = Int32.Parse(reader["COLLAPSE_END"].ToString());
                                    objEESData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());
                                    objEESData.morningMode = Int32.Parse(reader["MORNING_MODE"].ToString());
                                    objEESData.normalMode = Int32.Parse(reader["NORMAL_MODE"].ToString());

                                    objEESData.eveningMode = Int32.Parse(reader["EVENING_MODE"].ToString());
                                    objEESData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objEESData.normalMixEES = Int32.Parse(reader["NORMAL_MIX_EES"].ToString());
                                    objEESData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objEESData.machineChannel = reader["MACHINE_CHANNEL"].ToString();


                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return objEESData;
        }

        public bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue)
        {
            string qry = "";
            int intDataValue = 0;
            intDataValue = dataValue == true ? 1 : 0;
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_EES_MASTER SET " + tableField + " = " + intDataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + intDataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue)
        {
            string qry = "";
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_EES_MASTER SET " + tableField + " = " + dataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + dataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue)
        {
            string qry = "";
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_EES_MASTER SET " + tableField + " = " + dataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + dataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            bool bOk = false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_EES_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "', block_q_id=0"
                                  + "  where MACHINE_CODE = '" + machine_code + "' and IS_BLOCKED !='" + (blockStatus ? 1 : 0) + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    bOk = true;
                }

            }
            finally
            {

            }
            return bOk;
        }

        public bool UpdateMachineBlockStatusForPMS(string machine_code, bool blockStatus)
        {
            bool bOk = false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_EES_MASTER set IS_BLOCKED_PALLET ='" + (blockStatus ? 1 : 0) + "' where MACHINE_CODE = '" + machine_code + "'"
                                 + " and IS_BLOCKED_PALLET!= " + (blockStatus ? 1 : 0);
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bOk = command.ExecuteNonQuery() > 0;

                }

            }
            finally
            {

            }
            return bOk;
        }

        public bool IsEESDisabled(string machineName)
        {
            bool isDisable = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT status FROM L2_EES_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isDisable = bResult != 2;
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isDisable;
        }

        public bool IsEESSwitchOff(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_EES_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isSwitchOff = bResult ==1 ;
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isSwitchOff;
        }


        public bool IsEESEntryInCurrentModeInDB(string machineCode)
        {
            bool isEntry = false;
            string sql=null;
            try
            {
                int bResult = 0;
               
                sql = "SELECT config_package.isEESEntryInCurrentMode('" + machineCode + "') is_entry FROM dual";
             

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    
                        bResult = Convert.ToInt32(command.ExecuteScalar());
                        isEntry = bResult == 1;
                   
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isEntry;
        }
        public bool IsPSNotGettingFromEES(string machineCode)
        {
            bool isNotGetting = false;
            string sql = null;
            try
            {
                int bResult = 0;

                sql = "SELECT config_package.is_ees_ready_for_cm('" + machineCode + "') is_entry FROM dual";


                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isNotGetting = bResult == 1;

                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isNotGetting;
        }
        public bool IsEESBlockedInDBForParking(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_EES_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    bOk = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
                bOk = true;
            }
            finally
            {
            }

            return bOk;
        }
        public bool IsEESBlockedInDBForPMS(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED_PALLET  FROM L2_EES_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    bOk = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
                bOk = true;
            }
            finally
            {
            }

            return bOk;
        }
        public void UpdatePhotoPathToCustomerTable(EESData objEESData, string imageSouth, string imageNorth)
        {

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "";

                    if (objEESData.isEntry)
                        sql = "UPDATE L2_CUSTOMERS SET ENTRY_NORTH_IMG = '" + imageNorth + "'," +
                             " ENTRY_SOUTH_IMG ='" + imageSouth + "' WHERE customer_id = '" + objEESData.customerPkId + "' and ENTRY_QUEUE_ID =" + objEESData.queueId;
                    else
                        sql = "UPDATE L2_CUSTOMERS SET EXIT_NORTH_IMG = '" + imageNorth + "'," +
                             " EXIT_SOUTH_IMG ='" + imageSouth + "' WHERE customer_id = '" + objEESData.customerPkId + "' and EXIT_QUEUE_ID =" + objEESData.queueId;

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
            }
            catch (Exception errMsg)
            {

            }
            finally
            {

            }
        }

        //public void SetCarOutsideTime(string machineCode,DateTime outsideDate)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleConnection con = new DBConnection().getDBConnection())
        //        {
                    
        //            if (con.State == ConnectionState.Closed) con.Open();
        //            using (OracleCommand command = con.CreateCommand())
        //            {
        //                string sql = "update WAIT_CAR_FOR_EES_SNAP set CAR_OUTSIDE_TIME=:outSideDate"
        //                                 + "  where EES_ID=:eesId ";
        //                command.BindByName = true;
        //                command.Parameters.Add("outSideDate", outsideDate.ToString("dd-MMM-yy hh:mm:ss tt"));
        //                command.Parameters.Add("eesId", machineCode[machineCode.Length - 1]);
                       
        //                command.CommandText = sql;
        //                command.ExecuteNonQuery();
        //                result = true;
        //            }

        //        }
        //    }
        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //}
        //public void SetEESReadyTime(string machineCode, DateTime outsideDate)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleConnection con = new DBConnection().getDBConnection())
        //        {

        //            if (con.State == ConnectionState.Closed) con.Open();
        //            using (OracleCommand command = con.CreateCommand())
        //            {
        //                string sql = "update WAIT_CAR_FOR_EES_SNAP set EES_READY_TIME=:outSideDate"
        //                                 + "  where EES_ID=:eesId ";
        //                command.BindByName = true;
        //                command.Parameters.Add("outSideDate", outsideDate.ToString("dd-MMM-yy hh:mm:ss tt"));
        //                command.Parameters.Add("eesId", machineCode[machineCode.Length - 1]);

        //                command.CommandText = sql;
        //                command.ExecuteNonQuery();
        //                result = true;
        //            }

        //        }
        //    }
        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //}
        //public void SetCarAlignedTime(string machineCode, DateTime outsideDate)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleConnection con = new DBConnection().getDBConnection())
        //        {

        //            if (con.State == ConnectionState.Closed) con.Open();
        //            using (OracleCommand command = con.CreateCommand())
        //            {
        //                string sql = "update WAIT_CAR_FOR_EES_SNAP set CAR_ALIGNED_TIME=:outSideDate"
        //                                 + "  where EES_ID=:eesId ";
        //                command.BindByName = true;
        //                command.Parameters.Add("outSideDate", outsideDate.ToString("dd-MMM-yy hh:mm:ss tt"));
        //                command.Parameters.Add("eesId", machineCode[machineCode.Length - 1]);

        //                command.CommandText = sql;
        //                command.ExecuteNonQuery();
        //                result = true;
        //            }

        //        }
        //    }
        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //}
        //public void SetReadyToGetTime(string machineCode, DateTime outsideDate)
        //{
        //    bool result = false;
        //    try
        //    {
        //        using (OracleConnection con = new DBConnection().getDBConnection())
        //        {

        //            if (con.State == ConnectionState.Closed) con.Open();
        //            using (OracleCommand command = con.CreateCommand())
        //            {
        //                string sql = "update WAIT_CAR_FOR_EES_SNAP set READY_TO_GET_TIME=:outSideDate"
        //                                 + "  where EES_ID=:eesId ";
        //                command.BindByName = true;
        //                command.Parameters.Add("outSideDate", outsideDate.ToString("dd-MMM-yy hh:mm:ss tt"));
        //                command.Parameters.Add("eesId", machineCode[machineCode.Length - 1]);

        //                command.CommandText = sql;
        //                command.ExecuteNonQuery();
        //                result = true;
        //            }

        //        }
        //    }
        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally
        //    {

        //    }
        //}

        public EESData GetBlockedEESDetails(Int64 queueId)
        {

            EESData objEESData = null;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT EES_ID, EES_NAME,AISLE,F_ROW,START_AISLE,END_AISLE,MACHINE_CODE,COLLAPSE_START"
                                     + " ,COLLAPSE_END,IS_BLOCKED,MORNING_MODE,NORMAL_MODE,EVENING_MODE,STATUS,NORMAL_MIX_EES,IS_AUTOMODE,MACHINE_CHANNEL"
                                     + " FROM L2_EES_MASTER"
                                     + " WHERE  BLOCK_Q_ID=" + queueId+ " and rownum=1";
                       

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                              
                                if (reader.Read())
                                {
                                    objEESData = new EESData();
                                    objEESData.eesPkId = Int32.Parse(reader["EES_ID"].ToString());
                                    objEESData.eesName = reader["EES_NAME"].ToString();
                                    objEESData.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    objEESData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objEESData.startAisle = Int32.Parse(reader["START_AISLE"].ToString());
                                    objEESData.endAisle = Int32.Parse(reader["END_AISLE"].ToString());
                                    objEESData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objEESData.collapseStart = Int32.Parse(reader["COLLAPSE_START"].ToString());

                                    objEESData.collapseEnd = Int32.Parse(reader["COLLAPSE_END"].ToString());
                                    objEESData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());
                                    objEESData.morningMode = Int32.Parse(reader["MORNING_MODE"].ToString());
                                    objEESData.normalMode = Int32.Parse(reader["NORMAL_MODE"].ToString());

                                    objEESData.eveningMode = Int32.Parse(reader["EVENING_MODE"].ToString());
                                    objEESData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objEESData.normalMixEES = Int32.Parse(reader["NORMAL_MIX_EES"].ToString());
                                    objEESData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objEESData.machineChannel = reader["MACHINE_CHANNEL"].ToString();


                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return objEESData;
        }

    }
}
