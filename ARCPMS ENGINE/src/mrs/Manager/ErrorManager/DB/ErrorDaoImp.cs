using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.DBCon;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;


namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.DB
{
    class ErrorDaoImp:ErrorDaoService
    {

        public bool UpdateLiveCommandOfMachine(Model.ErrorData objErrorData)
        {
            bool bOk = false;
            try
            {
               
                    using (OracleConnection con = new DBConnection().getDBConnection())
                    {

                        OracleCommand command = con.CreateCommand();
                        string sql = "update L2_TRIGGER_COMMANDS set COMMAND ='" + objErrorData.command + "'"
                           + " ,FLOOR =" + objErrorData.floor + " ,AISLE =" + objErrorData.aisle +
                           " ,FLOOR_ROW =" + objErrorData.floor_row + " ,DONE =" + (objErrorData.done ? 1 : 0)
                           + " ,Q_ID =" + objErrorData.queueId
                           + " ,SEQ_ID =" + objErrorData.seqId
                           + " where MACHINE = '" + objErrorData.machine + "'";
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
        public bool UpdateLiveCommandStatusOfMachine(string machine,bool isDone)
        {
            bool bOk = false;
            string sql =null;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    if (isDone)
                    {
                        sql = "update L2_TRIGGER_COMMANDS set Q_ID=0, DONE ='" + (isDone ? 1 : 0) + "'"
                        + " where MACHINE = '" + machine + "' and DONE !='" + (isDone ? 1 : 0) + "'";
                    }
                    else
                    {
                        sql = "update L2_TRIGGER_COMMANDS set DONE ='" + (isDone ? 1 : 0) + "'"
                    + " where MACHINE = '" + machine + "' and DONE !='" + (isDone ? 1 : 0) + "'";
                    }

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                   
                    bOk = true;
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + machine + ":--Exception 'UpdateLiveCommandStatusOfMachine':: " + ex.Message);
            }
            finally
            {

            }
            return bOk;
        }
        public Model.ErrorData GetLiveCommandOfMachine(string machine)
        {
            string query = "select TRIGGER_ID,MACHINE,COMMAND,FLOOR,AISLE,FLOOR_ROW,IS_TRIGGER,Q_ID from l2_trigger_commands where machine= '" + machine + "'"
                                                    + "  and done = 0";
            
            Model.ErrorData objErrorData = null;
            objErrorData = new Model.ErrorData();
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {


                    using (OracleCommand command = new OracleCommand(query))
                    {
                        command.CommandText = query;
                        command.Connection = con;

                        //int.TryParse(command.ExecuteScalar().ToString(), out val);
                        using (OracleDataReader dreader = command.ExecuteReader())
                        {
                            if (dreader.HasRows == true)
                            {
                                
                                objErrorData.machine = Convert.ToString(dreader["MACHINE"]);
                                objErrorData.command = Convert.ToString(dreader["COMMAND"]);
                                objErrorData.floor = int.Parse(Convert.ToString(dreader["FLOOR"]));
                                objErrorData.aisle = int.Parse(Convert.ToString(dreader["AISLE"]));
                                objErrorData.floor_row = int.Parse(Convert.ToString(dreader["FLOOR_ROW"]));
                                objErrorData.queueId = int.Parse(Convert.ToString(dreader["Q_ID"]));

                            }
                        }

                    }
                }


            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);

            }
            return objErrorData;
        }

        //18Oct18
        public bool UpdateTriggerActiveStatus(TriggerData objTriggerData)
        {
            bool bOk = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_TRIGGER_COMMANDS set is_trigger ='" + (objTriggerData.TriggerEnabled ? 1 : 0)
                        + "',TRIGGER_TYPE ='" + objTriggerData.category.ToString()
                        + "',N_VALUE ='" + objTriggerData.ErrorCode
                        + "' where MACHINE = '" + objTriggerData.MachineCode
                        + "' and is_trigger!='" + (objTriggerData.TriggerEnabled ? 1 : 0) + "'";
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

        public bool GetTriggerActiveStatus(string machine)
        {
            bool status = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select is_trigger from L2_TRIGGER_COMMANDS  where MACHINE = '" + machine + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    status = Convert.ToInt16(command.ExecuteScalar()) == 1 ? true : false;
                }
            }
            finally
            {

            }
            return status;
        }

        public Model.ErrorData GetCommandOfActiveTrigger(string machine)
        {
            string query = "select TRIGGER_ID,MACHINE,COMMAND,FLOOR,AISLE,FLOOR_ROW,IS_TRIGGER,Q_ID from l2_trigger_commands where machine= '" + machine + "'"
                                                    + "  and done = 0 and is_trigger=1";
            Int16 val = 0;
            Model.ErrorData objErrorData = null;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {


                    using (OracleCommand command = new OracleCommand(query))
                    {
                        command.CommandText = query;
                        command.Connection = con;

                        //int.TryParse(command.ExecuteScalar().ToString(), out val);
                        using (OracleDataReader dreader = command.ExecuteReader())
                        {
                            if (dreader.HasRows == true)
                            {
                                objErrorData = new Model.ErrorData();
                                objErrorData.machine = Convert.ToString(dreader["MACHINE"]);
                                objErrorData.command = Convert.ToString(dreader["COMMAND"]);
                                objErrorData.floor = int.Parse(Convert.ToString(dreader["FLOOR"]));
                                objErrorData.aisle = int.Parse(Convert.ToString(dreader["AISLE"]));
                                objErrorData.floor_row = int.Parse(Convert.ToString(dreader["FLOOR_ROW"]));
                                objErrorData.queueId = int.Parse(Convert.ToString(dreader["Q_ID"]));

                            }
                        }

                    }
                }


            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);

            }
            return objErrorData;
        }


        public int GetTriggerAction(string machine)
        {
            int triggerAction = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select trigger_action from L2_TRIGGER_COMMANDS  where MACHINE = '" + machine + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    triggerAction = Convert.ToInt16(command.ExecuteScalar()) ;
                }
            }
            finally
            {

            }
            return triggerAction;
        }
        public bool validate_live_command_update(Model.ErrorData objErrorData)
        {
            int validateStatus = 0;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CONFIG_PACKAGE.validate_live_command_update";

                        command.Parameters.Add("cm", OracleDbType.Varchar2, 50, objErrorData.machine, ParameterDirection.Input);
                        command.Parameters.Add("cm_command", OracleDbType.Varchar2, 50, objErrorData.command, ParameterDirection.Input);
                        command.Parameters.Add("dest_floor", OracleDbType.Int64, objErrorData.floor, ParameterDirection.Input);
                        command.Parameters.Add("dest_aisle", OracleDbType.Int64, objErrorData.aisle, ParameterDirection.Input);
                        command.Parameters.Add("dest_row", OracleDbType.Int64, objErrorData.floor_row, ParameterDirection.Input);

                        command.Parameters.Add("validate_status", OracleDbType.Int64, validateStatus, ParameterDirection.Output);

                        command.ExecuteNonQuery();
                        Int32.TryParse(Convert.ToString(command.Parameters["validate_status"].Value), out validateStatus);
                       
                    }
                }
            }

            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);

            }
            return validateStatus==1;
        }

        

    }
}
