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
    public partial class LoginScreen : Window
    {
        private string dbPassword;
        private string defaultPW;
        private string sysGuid;
        private string help_link;
        private MySqlConnection cnn = new MySqlConnection("server=iphere;database=databasehere;uid=userid;pwd=passwordhere=C@lTs;");

        public LoginScreen()
        {
            InitializeComponent();
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
            catch(Exception ex)
            {
                MessageBox.Show("HELP | Please contact support!");
                Console.Write(ex);
            }
        }
        private void Login(object sender, RoutedEventArgs e)
        {
            ManageLogin();
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if(txtUsername.Text != null && txtPassword.Password != null)
                {
                    ManageLogin();
                }
            }
        }
        private void ManageLogin()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT username, password, sysid FROM userdata WHERE username = @uname";
                cmd.Parameters.AddWithValue("@uname", txtUsername.Text);

                MySqlDataReader reader = cmd.ExecuteReader();
                
                while(reader.Read())
                {
                    dbPassword = reader.GetString("password");
                    sysGuid = reader.GetString("sysid");
                }
                reader.Close();

                if(dbPassword == null)
                {
                    MessageBox.Show("Username or password incorrect!");
                    return;
                }

                if (IsDefaultPassword())
                {
                    ChangePassword cpw = new ChangePassword(txtUsername.Text, defaultPW);
                    cpw.Show();
                    Close();
                    return;
                }

                string pw = txtPassword.Password;
                bool isValidPassword = BCrypt.Net.BCrypt.Verify(pw, dbPassword);
                if (isValidPassword)
                {
                    if (sysGuid == "notset")
                    {
                        var systemGuid = new SystemGuid();
                        MySqlCommand guidcmd = cnn.CreateCommand();
                        cmd.CommandText = "UPDATE userdata SET sysid = @sysguid WHERE username = @uname";
                        cmd.Parameters.AddWithValue("@sysguid", systemGuid.Value());

                        cmd.ExecuteNonQuery();
                        MainWindow dashboard = new MainWindow(txtUsername.Text);
                        dashboard.Show();
                        Close();
                        dbPassword = null;
                        cnn.Close();
                    }
                    else
                    {
                        var systemGuid = new SystemGuid();
                        if (sysGuid == systemGuid.Value())
                        {
                            MainWindow dashboard = new MainWindow(txtUsername.Text);
                            dashboard.Show();
                            Close();
                            dbPassword = null;
                            cnn.Close();
                        }
                        else
                        {
                            MessageBox.Show("HWID | Please contact support!");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Username or password incorrect!");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("LOGIN | Please contact support!");
                Console.Write(ex);
            }
        }
        private Boolean IsDefaultPassword()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT value FROM settings WHERE name = @settingsname";
                cmd.Parameters.AddWithValue("@settingsname", "default_pw");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    defaultPW = reader.GetString("value");
                }
                reader.Close();

                bool isDefaultPassword = BCrypt.Net.BCrypt.Verify(txtPassword.Password, defaultPW);

                if (isDefaultPassword && defaultPW == dbPassword)
                {
                    return true;
                }
                else
                { 
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("67 | Please contact support!");
                Console.Write(ex);
                return false;
            }
        }
    }
}
