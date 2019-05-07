using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace ARCPMS_ENGINE.src.mrs.DBCon
{
    class MSSQLCon
    {
        // static OracleConnection GlobalConn;
        //public string connectionString = null;
        public SqlConnection getMSDBConnection()
        {
            SqlConnection myConnection = null;
            try
            {
                myConnection=new SqlConnection("user id=sa;" +
                                       "password=sa;server=rps-asteco_db;" +
                                       "Trusted_Connection=yes;" +
                                       "database=MRS_Asteco; " +
                                       "connection timeout=30");


                if (myConnection.State == ConnectionState.Closed) myConnection.Open();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return myConnection;
        }
        public bool UpdateGateNumber(string cardId, string eesCode )
        {
           
            Int32 veeid = 0;
            int updateCount = 0;
            int eesNumber = 0;
            SqlConnection myConnection = new SqlConnection();
            eesCode = eesCode.Substring(3, 1);
            int.TryParse(eesCode, out eesNumber);
            try
            {
                //SqlDataReader myReader = null;
                myConnection = getMSDBConnection();
               
                SqlCommand myCommand = new SqlCommand("update VehicleEntryExit set EES=" + eesNumber 
                                                    //   + " where   VehicleId=" + cardId +" and status=2  ",
                                                     + " where   VehicleId='" + cardId +"'",
                                                         myConnection);

                updateCount = myCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                //myConnection.Close();
                myConnection.Dispose();
            }
            return updateCount > 0;
        }
        public bool UpdateCarOut(string cardId)
        {

            Int32 veeid = 0;
            int updateCount = 0;
            int eesNumber = 0;
            SqlConnection myConnection = new SqlConnection();          
            try
            {
                //SqlDataReader myReader = null;
                myConnection = getMSDBConnection();
                
                SqlCommand myCommand = new SqlCommand("update VehicleEntryExit set Status=3, CarOut=1 "
                                                      // + " where   VehicleId=" + cardId + " and status=2  ",
                                                      + " where   VehicleId='" + cardId + "'",
                                                         myConnection);

                updateCount = myCommand.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
              //  myConnection.Close();
                myConnection.Dispose();
            }
            return updateCount > 0;
        }
    }
}
