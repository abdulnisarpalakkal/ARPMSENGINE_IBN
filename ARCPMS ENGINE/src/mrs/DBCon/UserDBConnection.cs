using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Config;

namespace ARCPMS_ENGINE.src.mrs.DBCon
{
    public class UserDBConnection
    {
       // static OracleConnection GlobalConn;
        //public string connectionString = null;
        public  OracleConnection getDBConnection()
        {
            OracleConnection con = new OracleConnection();
            try
            {

                //con.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=3.0.0.188)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=l2_user_admin;Password=fcluser456;";
                con.ConnectionString = BasicConfig.GetXmlTextOfTag("userConnectionString");
                if (con.State == ConnectionState.Closed) con.Open();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
                return con;
        } 
    }
}
