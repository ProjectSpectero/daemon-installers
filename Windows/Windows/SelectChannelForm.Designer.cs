namespace installer
{
    partial class SelectChannelForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.StableRadio = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.BetaRadio = new System.Windows.Forms.RadioButton();
            this.AlphaRadio = new System.Windows.Forms.RadioButton();
            this.ChannelVersion = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(643, 63);
            this.panel1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(247, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Spectero comes with multiple channels of releases.\r\n";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Release Channel Selection";
            // 
            // StableRadio
            // 
            this.StableRadio.AutoSize = true;
            this.StableRadio.Location = new System.Drawing.Point(89, 163);
            this.StableRadio.Name = "StableRadio";
            this.StableRadio.Size = new System.Drawing.Size(175, 17);
            this.StableRadio.TabIndex = 5;
            this.StableRadio.Text = "Stable (Intended for Production)";
            this.StableRadio.UseVisualStyleBackColor = true;
            this.StableRadio.CheckedChanged += new System.EventHandler(this.StableRadio_CheckChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(56, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(532, 39);
            this.label3.TabIndex = 2;
            this.label3.Text = "Release channels designate different branches in development of how Spectero rele" +
    "ases features to the public.\r\nYou may choose one of the following channels below" +
    ".\r\n\r\n";
            // 
            // BetaRadio
            // 
            this.BetaRadio.AutoSize = true;
            this.BetaRadio.Location = new System.Drawing.Point(89, 186);
            this.BetaRadio.Name = "BetaRadio";
            this.BetaRadio.Size = new System.Drawing.Size(273, 17);
            this.BetaRadio.TabIndex = 6;
            this.BetaRadio.Text = "Beta (Test the newest features, problems may occur)\r\n";
            this.BetaRadio.UseVisualStyleBackColor = true;
            this.BetaRadio.CheckedChanged += new System.EventHandler(this.BetaRadio_CheckChanged);
            // 
            // AlphaRadio
            // 
            this.AlphaRadio.AutoSize = true;
            this.AlphaRadio.Location = new System.Drawing.Point(89, 209);
            this.AlphaRadio.Name = "AlphaRadio";
            this.AlphaRadio.Size = new System.Drawing.Size(482, 17);
            this.AlphaRadio.TabIndex = 7;
            this.AlphaRadio.Text = "Alpha (Test the newest features before the beta channel, but may run into issues " +
    "more frequently)\r\n";
            this.AlphaRadio.UseVisualStyleBackColor = true;
            this.AlphaRadio.CheckedChanged += new System.EventHandler(this.AlphaRadio_CheckChanged);
            // 
            // ChannelVersion
            // 
            this.ChannelVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChannelVersion.FormattingEnabled = true;
            this.ChannelVersion.Location = new System.Drawing.Point(89, 267);
            this.ChannelVersion.Name = "ChannelVersion";
            this.ChannelVersion.Size = new System.Drawing.Size(152, 21);
            this.ChannelVersion.TabIndex = 12;
            this.ChannelVersion.SelectedIndexChanged += new System.EventHandler(this.ChannelVersion_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(86, 251);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Select Version:";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.BackButton);
            this.panel3.Controls.Add(this.NextButton);
            this.panel3.Controls.Add(this.ExitButton);
            this.panel3.Location = new System.Drawing.Point(1, 362);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(643, 40);
            this.panel3.TabIndex = 14;
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
            this.NextButton.Text = "Next >";
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
            // SelectChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 402);
            this.ControlBox = false;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ChannelVersion);
            this.Controls.Add(this.AlphaRadio);
            this.Controls.Add(this.BetaRadio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.StableRadio);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectChannelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spectero Installation Wizard";
            this.Load += new System.EventHandler(this.SelectChannelForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton StableRadio;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton BetaRadio;
        private System.Windows.Forms.RadioButton AlphaRadio;
        private System.Windows.Forms.ComboBox ChannelVersion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button ExitButton;
    }
}