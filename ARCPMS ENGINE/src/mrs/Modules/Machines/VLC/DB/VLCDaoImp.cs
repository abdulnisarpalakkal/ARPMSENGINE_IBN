using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using Oracle.DataAccess.Client;
using ARCPMS_ENGINE.src.mrs.DBCon;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Global;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.DB
{
    class VLCDaoImp:VLCDaoService
    {

        public List<Model.VLCData> GetVLCList()
        {
            List<VLCData> lstVLCData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT VLC_ID, VLC_NAME,F_ROW,F_AISLE,F_FLOOR,MACHINE_CODE,IS_BLOCKED"
                                     + " ,STATUS,IS_AUTOMODE,MACHINE_CHANNEL,VLC_DECK_CODE"
                                     + " FROM L2_VLC_MASTER";
                        


                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstVLCData = new List<VLCData>();

                                while (reader.Read())
                                {

                                    Model.VLCData objVLCData = new Model.VLCData();

                                    objVLCData.vlcPkId = Int32.Parse(reader["VLC_ID"].ToString());
                                    objVLCData.vlcName = reader["VLC_NAME"].ToString();
                                    objVLCData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objVLCData.aisle = Int32.Parse(reader["F_AISLE"].ToString());
                                    objVLCData.floor = Int32.Parse(reader["F_FLOOR"].ToString());
                                    objVLCData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objVLCData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());

                                    objVLCData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objVLCData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString())==1?true:false;
                                    objVLCData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objVLCData.vlcDeckCode = reader["VLC_DECK_CODE"].ToString();


                                    lstVLCData.Add(objVLCData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return lstVLCData;
        }

        public Model.VLCData GetVLCDetails(Model.VLCData objVLCData)
        {
         

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT VLC_ID, VLC_NAME,F_ROW,F_AISLE,F_FLOOR,MACHINE_CODE,IS_BLOCKED"
                                     + " ,STATUS,IS_AUTOMODE,MACHINE_CHANNEL,VLC_DECK_CODE"
                                     + " FROM L2_VLC_MASTER";
                        if(!string.IsNullOrEmpty(objVLCData.machineCode))
                        {
                            sql += " where MACHINE_CODE='" + objVLCData.machineCode+"'";
                        }


                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                               
                                while (reader.Read())
                                {

                                    objVLCData.vlcPkId = Int32.Parse(reader["VLC_ID"].ToString());
                                    objVLCData.vlcName = reader["VLC_NAME"].ToString();
                                    objVLCData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objVLCData.aisle = Int32.Parse(reader["F_AISLE"].ToString());
                                    objVLCData.floor = Int32.Parse(reader["F_FLOOR"].ToString());
                                    objVLCData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objVLCData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());

                                    objVLCData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objVLCData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString()) == 1 ? true : false;
                                    objVLCData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objVLCData.vlcDeckCode = reader["VLC_DECK_CODE"].ToString();

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return objVLCData;
        }

        public bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue)
        {
            string qry = "";
            bool result;
            int intDataValue = dataValue == true ? 1 : 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_VLC_MASTER SET " + tableField + " = " + intDataValue
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
                        qry = "UPDATE L2_VLC_MASTER SET " + tableField + " = " + dataValue
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
                        qry = "UPDATE L2_VLC_MASTER SET " + tableField + " = " + dataValue
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
                    string sql = "update L2_VLC_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "', BLOCK_Q_ID =0"
                                + " where MACHINE_CODE = '" + machine_code + "' and IS_BLOCKED !='" + (blockStatus ? 1 : 0) + "'";
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


        public bool IsVLCDisabled(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT STATUS FROM L2_VLC_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    bOk = bResult != 2;
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

        public bool IsVLCSwitchOff(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_VLC_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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

        public bool IsVLCBlockedInDB(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_VLC_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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
        public Model.VLCData GetVLCDetails(Int64 queueId)
        {

            Model.VLCData objVLCData = null;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT VLC_ID, VLC_NAME,F_ROW,F_AISLE,F_FLOOR,MACHINE_CODE,IS_BLOCKED"
                                     + " ,STATUS,IS_AUTOMODE,MACHINE_CHANNEL,VLC_DECK_CODE"
                                     + " FROM L2_VLC_MASTER where BLOCK_Q_ID=" + queueId;
                     

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                if (reader.Read())
                                {
                                    objVLCData = new VLCData();
                                    objVLCData.vlcPkId = Int32.Parse(reader["VLC_ID"].ToString());
                                    objVLCData.vlcName = reader["VLC_NAME"].ToString();
                                    objVLCData.row = Int32.Parse(reader["F_ROW"].ToString());

                                    objVLCData.aisle = Int32.Parse(reader["F_AISLE"].ToString());
                                    objVLCData.floor = Int32.Parse(reader["F_FLOOR"].ToString());
                                    objVLCData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objVLCData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString());

                                    objVLCData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objVLCData.isAutoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString()) == 1 ? true : false;
                                    objVLCData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objVLCData.vlcDeckCode = reader["VLC_DECK_CODE"].ToString();

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                objVLCData = null;
            }
            return objVLCData;
        }
       
        public GlobalValues.vlcMode GetVLCMode(string machineName)
        {
            GlobalValues.vlcMode vlcMode = GlobalValues.vlcMode.MIXED;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT VLC_MODE FROM L2_VLC_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    if( bResult == 1)
                       vlcMode = GlobalValues.vlcMode.ENTRY;
                    else if( bResult == 1)
                       vlcMode = GlobalValues.vlcMode.EXIT;
                }
            }
            catch (Exception errMsg)
            {
                vlcMode = GlobalValues.vlcMode.MIXED;
            }
            finally
            {
            }

            return vlcMode;
        }
    }
}
