namespace ARCPMS_ENGINE
{
    partial class OperationConfirm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.restartBut = new System.Windows.Forms.Button();
            this.resumeBut = new System.Windows.Forms.Button();
            this.parkCheck = new System.Windows.Forms.CheckBox();
            this.pmsCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // restartBut
            // 
            this.restartBut.Location = new System.Drawing.Point(29, 40);
            this.restartBut.Name = "restartBut";
            this.restartBut.Size = new System.Drawing.Size(131, 56);
            this.restartBut.TabIndex = 0;
            this.restartBut.Text = "RESTART";
            this.restartBut.UseVisualStyleBackColor = true;
            // 
            // resumeBut
            // 
            this.resumeBut.Location = new System.Drawing.Point(224, 40);
            this.resumeBut.Name = "resumeBut";
            this.resumeBut.Size = new System.Drawing.Size(131, 56);
            this.resumeBut.TabIndex = 0;
            this.resumeBut.Text = "RESUME";
            this.resumeBut.UseVisualStyleBackColor = true;
            // 
            // parkCheck
            // 
            this.parkCheck.AutoSize = true;
            this.parkCheck.Location = new System.Drawing.Point(15, 11);
            this.parkCheck.Name = "parkCheck";
            this.parkCheck.Size = new System.Drawing.Size(62, 17);
            this.parkCheck.TabIndex = 1;
            this.parkCheck.Text = "Parking";
            this.parkCheck.UseVisualStyleBackColor = true;
            // 
            // pmsCheck
            // 
            this.pmsCheck.AutoSize = true;
            this.pmsCheck.Location = new System.Drawing.Point(101, 11);
            this.pmsCheck.Name = "pmsCheck";
            this.pmsCheck.Size = new System.Drawing.Size(49, 17);
            this.pmsCheck.TabIndex = 1;
            this.pmsCheck.Text = "PMS";
            this.pmsCheck.UseVisualStyleBackColor = true;
            // 
            // OperationConfirm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 138);
            this.Controls.Add(this.pmsCheck);
            this.Controls.Add(this.parkCheck);
            this.Controls.Add(this.resumeBut);
            this.Controls.Add(this.restartBut);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OperationConfirm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OperationConfirm";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button restartBut;
        public System.Windows.Forms.Button resumeBut;
        public System.Windows.Forms.CheckBox parkCheck;
        public System.Windows.Forms.CheckBox pmsCheck;
    }
}