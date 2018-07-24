using installer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows;

namespace installer
{
    public partial class FinishForm : Form
    {
        public FinishForm()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Program.HarshExit(false);
        }

        private void FinishForm_Load(object sender, EventArgs e)
        {
            // Fix the icon for the form in the taskbar.
            this.Icon = Resources.DefaultIcon;
        }
    }
}
