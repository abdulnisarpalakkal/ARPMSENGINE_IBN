using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ARCPMS_ENGINE.src.mrs.user;
using System.Security.Principal;
using System.Security.Cryptography;



namespace ARCPMS_ENGINE
{
    public partial class AuthenticationForm : Form
    {
        bool isValid = false;
        public AuthenticationForm()
        {
            InitializeComponent();
        }

        private void password_TextChanged(object sender, EventArgs e)
        {

        }

        private void loginButton_Click(object sender, EventArgs e)
        {

            UserAuthentication objUserAuthentication = new UserAuthentication();
            Index.OnEngineClose += new EventHandler(Index_OnEngineClose);
            if (objUserAuthentication.ValidateUser(user.Text, password.Text))
            {
                //if (checkLicenseIsValid())
                //{
                    isValid = true;
                    this.Close();

                //}
                //else
                //    MessageBox.Show("License is not valid.", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Question);
                   
                //Application.Run(new Index());
                //new Index().ShowDialog();

            }
            else
            {
                if (MessageBox.Show("Wrong user/password.", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Question)
                    == System.Windows.Forms.DialogResult.OK)
                {

                }
            }
        }
        static void Index_OnEngineClose(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AuthenticationForm_Load(object sender, EventArgs e)
        {
           // this.Parent.FindForm().Close();
        }
        private void AuthenticationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
           // e.Cancel = true;
            //ToAndFromTray();
            if (!isValid) Environment.Exit(1);
        }
        public string GetRegistry()
        {
            string regValue = null;
            Microsoft.Win32.RegistryKey objRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("OSIdentity");
            regValue = (String)objRegistryKey.GetValue("Value");
            MessageBox.Show(regValue);
            objRegistryKey.Close();
            return regValue;

        }
        public bool checkLicenseIsValid()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            byte[] tmpSource;
            byte[] tmpHash;
            String OSHashValue = null;
            String regHashValue = null;

            if (currentIdentity != null)
            {
                SecurityIdentifier userSid = currentIdentity.User.AccountDomainSid;

                //Create a byte array from source data.
                tmpSource = ASCIIEncoding.ASCII.GetBytes(userSid.Value);
                //Compute hash based on source data.
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                OSHashValue = ByteArrayToString(tmpHash);
            }
            regHashValue = GetRegistry();
            return OSHashValue == regHashValue;

        }
         public string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

       
    }
}
