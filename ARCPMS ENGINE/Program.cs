using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;
using System.Diagnostics;

namespace ARCPMS_ENGINE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            bool ok;
            System.Threading.Mutex m = new System.Threading.Mutex(true, "ARCPMS ENGINE", out ok);

            if (!ok)
            {
                MessageBox.Show("Another instance is already running.");
                return;
            }
            if(GetNumberOfInstanceAlreadyRunning("ARCPMS ENGINE")>1)
            {
                MessageBox.Show("Another instance is already running.");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new AuthenticationForm());


            Index.OnEngineClose += new EventHandler(Index_OnEngineClose);

            Application.Run(new Index());


           //TestFunction(2);
           

        }
        static public int GetNumberOfInstanceAlreadyRunning(string applicationName)
        {
            int runCount = 0;
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            if (IsProcessOpen(applicationName) == false)
            {
                //System.Windows.MessageBox.Show("Application not open!");
                //System.Windows.Application.Current.Shutdown();
            }
            else
            {
                runCount = Process.GetProcessesByName(thisProc.ProcessName).Length;


            }
            return runCount;
        }
        static public bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
        static void TestFunction(int index)
        {
            //OpcConnection.GetOPCServerConnection();
            switch(index)
            {
                case 1:
                                try
                                {
                                    //BasicConfig.GetXmlTextOfTag("connectionString");
                                    DataTable dt = new DataTable();
                                    //dt.TableName = "SNAPSHOT";
                                    using (OracleConnection conn = new DBConnection().getDBConnection())
                                    {
                                        OracleCommand cmd = new OracleCommand();
                                        cmd.Connection = conn;
                                        cmd.CommandText = "select *  from l2_ees_master";
                                        cmd.CommandType = CommandType.Text;
                                        //OracleDataReader dr = cmd.ExecuteReader();
                                        //dr.Read();
                                        OracleDataAdapter dadapter = new OracleDataAdapter(cmd);
                                        dadapter.Fill(dt);
                                    }
                                }
                                catch (Exception ex) 
                                {
                                    Console.WriteLine(ex);
                                }
                                break;
                case 2:
                                GlobalValues.LOG_DIR = BasicConfig.GetXmlTextOfTag("LogPath");
                    
                               // for (int i = 0; i < 100;i++ )
                                    //Logger.WriteLogger(GlobalValues.LOG_DIR, GlobalValues.PARKING_LOG, "for testing" + i);
                                break;
                case 3:
                                GlobalValues.LOG_DIR = BasicConfig.GetXmlTextOfTag("LogPath");

                                // for (int i = 0; i < 100;i++ )
                                //Logger.WriteLogger(GlobalValues.LOG_DIR, GlobalValues.PARKING_LOG, "for testing" + i);
                                break;
                default:
                                break;
             }
        }

        static void Index_OnEngineClose(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
