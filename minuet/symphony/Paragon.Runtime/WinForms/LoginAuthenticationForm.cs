using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Paragon.Runtime.WinForms
{
    public partial class LoginAuthenticationForm : Form
    {
        public LoginAuthenticationForm(String host)
        {
            InitializeComponent();
            lblInformation.Text += host;
        }

        public String getUserName()
        {
            return txtboxUserName.Text;
        }

        public String getPasswd()
        {
            return txtboxPassword.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void LoginAuthenticationForm_Load(object sender, EventArgs e)
        {

        }
    }
}
