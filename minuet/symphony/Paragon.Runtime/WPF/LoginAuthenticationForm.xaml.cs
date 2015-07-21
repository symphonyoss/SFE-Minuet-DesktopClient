using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paragon.Runtime.WPF
{
    /// <summary>
    /// Interaction logic for LoginAuthenticationForm.xaml
    /// </summary>
    public partial class LoginAuthenticationForm : Window
    {
        public LoginAuthenticationForm(String host)
        {
            InitializeComponent();
            lblHostName.Content = host;
        }

        internal string UserName
        {
               get { return txtboxUserName.Text; }
        }

        internal string Password
        {
            get { return txtboxPassword.Password; }
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


    }
}
