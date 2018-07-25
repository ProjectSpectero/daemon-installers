namespace installer
{
    partial class InstallLocationForm
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
            this.panel3 = new System.Windows.Forms.Panel();
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.InstallLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ChangeDirectory = new System.Windows.Forms.Button();
            this.InstallAsService = new System.Windows.Forms.CheckBox();
            this.addToEnv = new System.Windows.Forms.CheckBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.BackButton);
            this.panel3.Controls.Add(this.NextButton);
            this.panel3.Controls.Add(this.ExitButton);
            this.panel3.Location = new System.Drawing.Point(2, 361);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(643, 40);
            this.panel3.TabIndex = 16;
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(349, 8);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(91, 23);
            this.BackButton.TabIndex = 2;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(446, 8);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(91, 23);
            this.NextButton.TabIndex = 1;
            this.NextButton.Text = "Install >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(543, 8);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(91, 23);
            this.ExitButton.TabIndex = 0;
            this.ExitButton.Text = "Cancel";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(2, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(643, 63);
            this.panel1.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(250, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "The working directory of where Spectero will reside.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Installation Directory";
            // 
            // InstallLocation
            // 
            this.InstallLocation.Location = new System.Drawing.Point(85, 206);
            this.InstallLocation.Name = "InstallLocation";
            this.InstallLocation.Size = new System.Drawing.Size(477, 20);
            this.InstallLocation.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(82, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(292, 65);
            this.label3.TabIndex = 18;
            this.label3.Text = "Please select a location that spectero can reside in.\r\nBy default, a reccomended " +
    "path is determined for you below.\r\n\r\nTo continue, click Next.\r\n\r\n";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(82, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Save files in folder:";
            // 
            // ChangeDirectory
            // 
            this.ChangeDirectory.Location = new System.Drawing.Point(446, 232);
            this.ChangeDirectory.Name = "ChangeDirectory";
            this.ChangeDirectory.Size = new System.Drawing.Size(116, 23);
            this.ChangeDirectory.TabIndex = 3;
            this.ChangeDirectory.Text = "Change Location";
            this.ChangeDirectory.UseVisualStyleBackColor = true;
            this.ChangeDirectory.Click += new System.EventHandler(this.ChangeDirectory_Click);
            // 
            // InstallAsService
            // 
            this.InstallAsService.AutoSize = true;
            this.InstallAsService.Location = new System.Drawing.Point(85, 268);
            this.InstallAsService.Name = "InstallAsService";
            this.InstallAsService.Size = new System.Drawing.Size(194, 17);
            this.InstallAsService.TabIndex = 20;
            this.InstallAsService.Text = "Install Spectero as a system service\r\n";
            this.InstallAsService.UseVisualStyleBackColor = true;
            this.InstallAsService.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // addToEnv
            // 
            this.addToEnv.AutoSize = true;
            this.addToEnv.Location = new System.Drawing.Point(85, 291);
            this.addToEnv.Name = "addToEnv";
            this.addToEnv.Size = new System.Drawing.Size(240, 17);
            this.addToEnv.TabIndex = 21;
            this.addToEnv.Text = "Add to System Environment Variables (PATH)";
            this.addToEnv.UseVisualStyleBackColor = true;
            this.addToEnv.CheckedChanged += new System.EventHandler(this.addToEnv_CheckedChanged);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::installer.Properties.Resources.google_32_132;
            this.pictureBox3.Location = new System.Drawing.Point(514, 5);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(120, 50);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 30;
            this.pictureBox3.TabStop = false;
            // 
            // InstallLocationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 402);
            this.ControlBox = false;
            this.Controls.Add(this.addToEnv);
            this.Controls.Add(this.InstallAsService);
            this.Controls.Add(this.ChangeDirectory);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.InstallLocation);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InstallLocationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Specter Installation Wizard";
            this.Load += new System.EventHandler(this.InstallLocationForm_Load);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InstallLocation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ChangeDirectory;
        private System.Windows.Forms.CheckBox InstallAsService;
        private System.Windows.Forms.CheckBox addToEnv;
        private System.Windows.Forms.PictureBox pictureBox3;
    }
}