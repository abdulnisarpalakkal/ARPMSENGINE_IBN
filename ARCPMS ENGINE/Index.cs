using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using ARCPMS_ENGINE.src.mrs;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;

namespace ARCPMS_ENGINE
{
    public partial class Index : Form
    {

        
         //Off_SicherheitLicense_oK_1958
        public static event EventHandler OnEngineClose;
        OperationConfirm confirmForm = null;
       
        string timeStampForGeneralMessage = "";
        
        
        StringBuilder strOnMachineBlockUnblockLog = new StringBuilder();

        InitializeEngine engine = null;
        public Index()
        {
            InitializeComponent();
            ntfyARCPSEngine.BalloonTipText = "ARCPS Engine 1.0.5.5";
            src.mrs.InitializeEngine.OnToDisplayMessage += new EventHandler(OnToDisplayMessage);
            QueueControllerImp.OnToDisplayMessage += new EventHandler(OnToDisplayMessage);
           // EESManagerThread.OnToDisplayMessage += new EventHandler(OnToDisplayMessage);
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

           
            btnStartEngine.Enabled = true;
            btnStopEngine.Enabled = false;

            if (engine == null)
                engine = new InitializeEngine();

        }

        private void index_Load(object sender, EventArgs e)
        {
            autoRefreshCheck.Checked = GetAutoRefresh();
        }


        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
           
            Environment.Exit(1);
        }

        

        
        void OnToDisplayMessage(object sender, EventArgs e)
        {
            try
            {
                

                rtxtMessage.BeginInvoke(new Action(() =>
                    {
                        if (timeStampForGeneralMessage != System.DateTime.Now.ToString("dd/MMM/yyyy"))
                        {
                            timeStampForGeneralMessage = System.DateTime.Now.ToString("dd/MMM/yyyy");
                            rtxtMessage.Text += " Group :" + timeStampForGeneralMessage + System.Environment.NewLine;
                        }

                        rtxtMessage.Text += Convert.ToString(sender) + System.Environment.NewLine;


                    } ));

            }
            catch (Exception errMsg)
            { }
            finally { }
        }

        private void index_Resize(object sender, EventArgs e)
        {
            ToAndFromTray();
        }

        void ToAndFromTray()
        {
            if (FormWindowState.Minimized == this.WindowState)
            {

                ntfyARCPSEngine.Visible = true;
                ntfyARCPSEngine.ShowBalloonTip(500);
                //this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                //ntfyARCPSEngine.Visible = false;
            }
        }

        private void ntfyARCPSEngine_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //this.Visible = true;
            //this.WindowState = FormWindowState.Normal;
            //ntfyARCPSEngine.Visible = false;

            // Set the WindowState to normal if the form is minimized. 
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form. 
            this.Activate();
            this.Show();
        }

        private void btnStartEngine_Click(object sender, EventArgs e)
        {
           


            btnStartEngine.Enabled = false;
            btnStopEngine.Enabled = true;
            confirmForm = new OperationConfirm();
            confirmForm.restartBut.Click += restartBut_Click;
            confirmForm.resumeBut.Click += resumeBut_Click;
            confirmForm.ShowDialog();
            //if (MessageBox.Show("Do you want to resume  the engine from the last stage ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            //         == System.Windows.Forms.DialogResult.Yes)
            //{
            //    Task.Factory.StartNew(new Action(() => new InitializeEngine().DoInitializeEngine(GlobalValues.engineStartMode.resume)));
            //}
            //else
            //{
            //    Task.Factory.StartNew(new Action(() => new InitializeEngine().DoInitializeEngine(GlobalValues.engineStartMode.restart)));
            //}
            
           
        }

        void resumeBut_Click(object sender, EventArgs e)
        {
            GlobalValues.PARKING_ENABLED = confirmForm.parkCheck.Checked;
            GlobalValues.PMS_ENABLED = confirmForm.pmsCheck.Checked;
            Task.Factory.StartNew(new Action(() => engine.DoInitializeEngine(GlobalValues.engineStartMode.resume)));
            confirmForm.Close();
        }

        void restartBut_Click(object sender, EventArgs e)
        {
            GlobalValues.PARKING_ENABLED = confirmForm.parkCheck.Checked;
            GlobalValues.PMS_ENABLED = confirmForm.pmsCheck.Checked;
            Task.Factory.StartNew(new Action(() => engine.DoInitializeEngine(GlobalValues.engineStartMode.restart)));
            confirmForm.Close();
        }
        private void testMethod()
        {
            try
            {
                while (! GlobalValues.threadsDictionary[123].Token.IsCancellationRequested)
                {
                }
                GlobalValues.threadsDictionary[123].Token.ThrowIfCancellationRequested();
                
            }
            catch (Exception ex)
            {
                //threadsDictionary[123].Dispose();
                Console.WriteLine("thread cancelled");
            }
            
            
        }
      
        private void btnStopEngine_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to stop the engine?.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                     == System.Windows.Forms.DialogResult.Yes)
            {
              
                try
                {
                    btnStopEngine.Enabled = false;
                    engine.Dispose();
                }
                finally
                {
                   
                    Environment.Exit(1);
                }
            }
        }

        public void Message(string message)
        {
            try
            {

            }
            finally
            {
            }
        }

       
        private void index_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            e.Cancel = true;
            this.Hide();
            
            //notifyIcon1.ShowBalloonTip(500);
            ntfyARCPSEngine.ShowBalloonTip(500);
            ntfyARCPSEngine.Visible = true;
            
      
        }

        private void index_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            OnToDisplayMessage("Stop", null);
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            //RPMEEManageEngine.Security.frmAuthentication frmAuth = new RPMEEManageEngine.Security.frmAuthentication();
            //frmAuth.OnLoginReq += (s, evt) =>
            //    {
            //        frmAuth.Hide();
            //        frmConfigcs config = new frmConfigcs();
            //        config.ucConfigWindow1.OnCloseClick += (zendr, evnt) =>
            //        {
            //            this.Close();
            //        };
            //        config.ShowDialog();
            //    };
            //frmAuth.ShowDialog();
          


          
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoRefresh(autoRefreshCheck.Checked);
            autoRefreshCheck.Checked = GetAutoRefresh();
        }
        private void SetAutoRefresh(bool refreshStatus)
        {
            if (refreshStatus)
            {
                BasicConfig.SetXmlTextOfTag("AutoRefresh", "true");
            }
            else
            {
                BasicConfig.SetXmlTextOfTag("AutoRefresh", "false");
            }
        }
        private bool GetAutoRefresh()
        {
            bool refreshStatus = false;
            refreshStatus = BasicConfig.GetXmlTextOfTag("AutoRefresh")=="true";
            return refreshStatus;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GlobalValues.AUTO_REFRESH = GetAutoRefresh();
            MessageBox.Show("Refreshed");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to reinitialize the asynch reading?.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == System.Windows.Forms.DialogResult.Yes)
            {
                engine.ReinitializeOpc();
            }
        }

      
    }
}
