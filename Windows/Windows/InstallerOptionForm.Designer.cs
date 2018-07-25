namespace installer
{
    partial class InstallerOptionForm
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
            this.ExitButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.InstallArea = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.UninstallArea = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.InstallArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.UninstallArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.ExitButton);
            this.panel3.Location = new System.Drawing.Point(1, 362);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(643, 40);
            this.panel3.TabIndex = 25;
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(541, 7);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(91, 23);
            this.ExitButton.TabIndex = 0;
            this.ExitButton.Text = "Exit";
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
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(643, 63);
            this.panel1.TabIndex = 24;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::installer.Properties.Resources.google_32_132;
            this.pictureBox3.Location = new System.Drawing.Point(512, 5);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(120, 50);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 28;
            this.pictureBox3.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(5, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(162, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please choose an installer action";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select an option";
            // 
            // InstallArea
            // 
            this.InstallArea.BackColor = System.Drawing.Color.White;
            this.InstallArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InstallArea.Controls.Add(this.label4);
            this.InstallArea.Controls.Add(this.label3);
            this.InstallArea.Controls.Add(this.pictureBox2);
            this.InstallArea.Cursor = System.Windows.Forms.Cursors.Hand;
            this.InstallArea.Location = new System.Drawing.Point(115, 127);
            this.InstallArea.Name = "InstallArea";
            this.InstallArea.Size = new System.Drawing.Size(414, 71);
            this.InstallArea.TabIndex = 26;
            this.InstallArea.Click += new System.EventHandler(this.InstallArea_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(329, 13);
            this.label4.TabIndex = 31;
            this.label4.Text = "Install the Spectero Daemon and its dependencies on this computer.";
            this.label4.Click += new System.EventHandler(this.InstallArea_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(17, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 30;
            this.label3.Text = "Install";
            this.label3.Click += new System.EventHandler(this.InstallArea_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::installer.Properties.Resources.outline_get_app_black_18dp;
            this.pictureBox2.Location = new System.Drawing.Point(360, 16);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(36, 36);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 29;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.InstallArea_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::installer.Properties.Resources.outline_delete_forever_black_18dp;
            this.pictureBox1.Location = new System.Drawing.Point(360, 16);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(36, 36);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 28;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.UninstallArea_Click);
            // 
            // UninstallArea
            // 
            this.UninstallArea.BackColor = System.Drawing.Color.White;
            this.UninstallArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.UninstallArea.Controls.Add(this.label5);
            this.UninstallArea.Controls.Add(this.pictureBox1);
            this.UninstallArea.Controls.Add(this.label6);
            this.UninstallArea.Cursor = System.Windows.Forms.Cursors.Hand;
            this.UninstallArea.Enabled = false;
            this.UninstallArea.Location = new System.Drawing.Point(115, 204);
            this.UninstallArea.Name = "UninstallArea";
            this.UninstallArea.Size = new System.Drawing.Size(414, 71);
            this.UninstallArea.TabIndex = 27;
            this.UninstallArea.Click += new System.EventHandler(this.UninstallArea_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(246, 13);
            this.label5.TabIndex = 33;
            this.label5.Text = "Remove the Spectero Daemon from this computer.";
            this.label5.Click += new System.EventHandler(this.UninstallArea_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(17, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Uninstall";
            this.label6.Click += new System.EventHandler(this.UninstallArea_Click);
            // 
            // InstallerOptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 402);
            this.ControlBox = false;
            this.Controls.Add(this.UninstallArea);
            this.Controls.Add(this.InstallArea);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InstallerOptionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spectero Installer - Pick Option";
            this.Load += new System.EventHandler(this.InstallerOptionForm_Load);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.InstallArea.ResumeLayout(false);
            this.InstallArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.UninstallArea.ResumeLayout(false);
            this.UninstallArea.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel InstallArea;
        private System.Windows.Forms.Panel UninstallArea;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBox3;
    }
}