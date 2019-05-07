using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Global;
using System.Threading;

namespace ARCPMS_ENGINE.src.mrs.DBCon
{
    public class DBConnection
    {
       // static OracleConnection GlobalConn;
        //public string connectionString = null;
        /// <summary>
        /// get db connection using global variable
        /// </summary>
        /// <returns></returns>
        public  OracleConnection getDBConnection()
        {
            OracleConnection con = new OracleConnection();
             do
            {
                try
                {

                    con.ConnectionString = GlobalValues.GLOBAL_DB_CON_STRING;

                    if (con.State == ConnectionState.Closed) con.Open();


                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                }
            } while (con.State == ConnectionState.Closed);
                return con;
        } 
    }
}
