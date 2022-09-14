using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Markup;
using System.Management;
using MySql.Data.MySqlClient;

namespace ProjektManager
{
    public partial class ChangePassword : Window
    {
        private string help_link;
        private string cUsername;
        private string cPasswordHash;
        private MySqlConnection cnn = new MySqlConnection("server=iphere;database=databasehere;uid=userid;pwd=passwordhere=C@lTs;");

        public ChangePassword(string username, string dPasswordHash)
        {
            InitializeComponent();
            cUsername = username;
            cPasswordHash = dPasswordHash;
            cnn.Open();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void ButtonMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }
        private void ButtonClose(object sender, RoutedEventArgs e)
        {
            Close();
            cnn.Close();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT value FROM settings WHERE name = 'help_link'";
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    help_link = reader.GetString("value");
                }
                reader.Close();
                System.Diagnostics.Process.Start(help_link);
            }
            catch (Exception ex)
            {
                MessageBox.Show("HELP | Please contact support!");
                Console.Write(ex);
            }
        }
        private void ChangePasswordNow(object sender, RoutedEventArgs e)
        {
            ManagePaswordChange();
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (txtPassword1.Password != null && txtPassword2.Password != null)
                {
                    ManagePaswordChange();
                }
            }
        }
        private void ManagePaswordChange()
        {
            try
            {
                if(txtPassword1.Password == txtPassword2.Password)
                {
                    if(txtPassword1.Password.Length > 6)
                    {
                        string hPassword = BCrypt.Net.BCrypt.HashPassword(txtPassword1.Password);
                        if (hPassword != cPasswordHash)
                        {
                            MySqlCommand cmd = cnn.CreateCommand();
                            cmd.CommandText = "UPDATE userdata SET password = @newpw WHERE username = @uname";
                            cmd.Parameters.AddWithValue("@uname", cUsername);

                            cmd.Parameters.AddWithValue("@newpw", hPassword);
                            cmd.ExecuteNonQuery();
                            MainWindow dashboard = new MainWindow(cUsername);
                            dashboard.Show();
                            Close();
                            hPassword = null;
                            cPasswordHash = null;
                            cnn.Close();
                        }else
                        {
                            MessageBox.Show("Please dont use your old password!");
                        }
                    }else
                    {
                        MessageBox.Show("Password must be longer than 6 characters!");
                    }
                }else
                {
                    MessageBox.Show("Passwords must match!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LOGIN | Please contact support!");
                Console.Write(ex);
            }
        }
    }
}
