using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Utilities;

namespace ProjektManager
{
    public partial class MainWindow : Window
    {
        // THIS VERSION ID ------------
        private readonly int thisVersionID = 1;
        // THIS VERSION ID ------------

        private string cUsername;
        private string prod_id;
        private string help_link;
        private long expirytimestamp;
        private bool adminUserEditMode;
        private bool AdminSettingsLoaded;
        private bool tempcheck;
        private string tempusername;
        private string defaultPW_Hash;
        private string defaultPWPlain;

        private string help_link_settings;
        private string version_settings;
        private string defaultPWPlain_Admin;

        private Random random = new Random();

        private MySqlConnection cnn = new MySqlConnection("server=iphere;database=databasehere;uid=userid;pwd=passwordhere=C@lTs;");

        public MainWindow(string currentUsername)
        {
            InitializeComponent();
            cUsername = currentUsername;
            cnn.Open();
            SetFirstJoin();
            UpdateProductsName();
            UpdateProductKeyTime();
            StartAutoLogOut();
            CheckUserState();
            CheckVersion();
            ReturnDefaultPassword();
            AdminSettingsLoaded = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void ButtonClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ButtonMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }
        private void LogOut(object sender, RoutedEventArgs e)
        {
            cUsername = "undefined";
            LoginScreen loginwindow = new LoginScreen();
            loginwindow.Show();
            Close();
        }
        private void SetFirstJoin()
        {
            txt_license.CharacterCasing = CharacterCasing.Upper;
            welcome_message.Inlines.Clear();
            welcome_message.Inlines.Add("Welcome ");
            welcome_message.Inlines.Add(new Run(cUsername) { Foreground = Brushes.DeepSkyBlue });
            welcome_message.Inlines.Add("!");
        }
        private void SubmitLicenseKey(object sender, RoutedEventArgs e)
        {
            if (txt_license.Text.Length != 16)
            {
                MessageBox.Show("license key invalid");
                return;
            }
            try
            {
                tempusername = null;
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM licensekeys WHERE licensekey = @key";
                cmd.Parameters.AddWithValue("@key", txt_license.Text);

                MySqlDataReader reader = cmd.ExecuteReader();

                Boolean invalid = true;
                while (reader.Read())
                {
                    prod_id = reader.GetString("product_id");
                    tempusername = reader.GetString("username");
                    string timestamp = reader.GetString("timestamp");
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                    long add_timestamp = long.Parse(timestamp) * 30 * 24 * 60 * 60 * 1000;
                    expirytimestamp = unixTimeMilliseconds + add_timestamp;
                }
                reader.Close();

                if (tempusername != "0")
                {
                    MessageBox.Show("license key invalid");
                    return;
                }

                if (prod_id == "1" || prod_id == "2" || prod_id == "3")
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    UpdateProfile(prod_id, expirytimestamp);
                    KeyUsageSave(txt_license.Text, now.ToUnixTimeMilliseconds(), expirytimestamp);
                    MessageBox.Show("You have successfully redeemed a license key!");
                    invalid = false;
                }

                if (invalid)
                {
                    MessageBox.Show("license key invalid");
                }
                else
                {
                    UpdateProductKeyTime();
                    prod_id = "empty";
                    expirytimestamp = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("2 | Database Error!");
                Console.Write(ex);
            }
        }
        private void KeyUsageSave(string key, long c_timestamp, long ex_timestamp)
        {
            MySqlCommand command = cnn.CreateCommand();
            command.CommandText = "UPDATE licensekeys SET username = @user, usage_timestamp = @usage, expiry_timestamp = @expire WHERE licensekey = @licenskey";
            command.Parameters.AddWithValue("@user", cUsername);
            command.Parameters.AddWithValue("@licenskey", key);
            command.Parameters.AddWithValue("@usage", c_timestamp);
            command.Parameters.AddWithValue("@expire", ex_timestamp);
            command.ExecuteNonQuery();
        }
        private void UpdateProfile(string Productid, long expiry_timestamp)
        {
            long expiry_timestamp_1hour = expiry_timestamp + (1000 * 60 * 60);
            try
            {
                switch (Productid)
                {

                    case "1":
                        {
                            MySqlCommand command = cnn.CreateCommand();
                            command.CommandText = "UPDATE userdata SET product_1 = @time WHERE username = @cusername";
                            command.Parameters.AddWithValue("@time", expiry_timestamp_1hour);
                            command.Parameters.AddWithValue("@cusername", cUsername);
                            command.ExecuteNonQuery();
                            break;
                        }
                    case "2":
                        {
                            MySqlCommand command = cnn.CreateCommand();
                            command.CommandText = "UPDATE userdata SET product_2 = @time WHERE username = @cusername";
                            command.Parameters.AddWithValue("@time", expiry_timestamp_1hour);
                            command.Parameters.AddWithValue("@cusername", cUsername);
                            command.ExecuteNonQuery();
                            break;
                        }
                    case "3":
                        {
                            MySqlCommand command = cnn.CreateCommand();
                            command.CommandText = "UPDATE userdata SET product_3 = @time WHERE username = @cusername";
                            command.Parameters.AddWithValue("@time", expiry_timestamp_1hour);
                            command.Parameters.AddWithValue("@cusername", cUsername);
                            command.ExecuteNonQuery();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("98 | Database Error!");
                Console.Write(ex);
            }
        }
        
        private void UpdateProductsName()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM products";

                MySqlDataReader reader = cmd.ExecuteReader();

                ArrayList Al = new ArrayList();
                while (reader.Read())
                {
                    Al.Add(reader.GetValue(1));
                }
                reader.Close();

                int i = 0;
                foreach (String ProductName in Al)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                product_1_name.Text = ProductName + ":";
                                Product1_Tab.Header = "  " + ProductName + "  ";
                                i++;
                                break;
                            }
                        case 1:
                            {
                                product_2_name.Text = ProductName + ":";
                                Product2_Tab.Header = "  " + ProductName + "  ";
                                i++;
                                break;
                            }
                        case 2:
                            {
                                product_3_name.Text = ProductName + ":";
                                Product3_Tab.Header = "  " + ProductName + "  ";
                                i++;
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("3 | Database Error!");
                Console.Write(ex);
            }
        }
        private void UpdateProductKeyTime()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userdata WHERE username = @uname";
                cmd.Parameters.AddWithValue("@uname", cUsername);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string product_1_timestamp = reader.GetString("product_1");
                    string product_2_timestamp = reader.GetString("product_2");
                    string product_3_timestamp = reader.GetString("product_3");

                    if (product_1_timestamp == "0")
                    {
                        product_1_txt.Text = "Never purchased";
                        product_1_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product1_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product1_Tab.IsEnabled = false;
                    }
                    else
                    {
                        long timestamp1 = long.Parse(product_1_timestamp);
                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                        long timestamp_difference = timestamp1 - unixTimeMilliseconds;
                        long timestamp_hours = timestamp_difference / 1000 / 60 / 60;
                        long timestamp_days = timestamp_difference / 1000 / 60 / 60 / 24;
                        if (timestamp_hours > 1)
                        {
                            Product1_Tab.IsEnabled = true;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                product_1_txt.Text = timestamp_days + " Days remaining.";
                                product_1_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product1_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                            else
                            {
                                product_1_txt.Text = "Lifetime";
                                product_1_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product1_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                        }
                        else
                        {
                            product_1_txt.Text = "Expired";
                            product_1_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product1_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product1_Tab.IsEnabled = false;
                        }
                    }

                    if (product_2_timestamp == "0")
                    {
                        product_2_txt.Text = "Never purchased";
                        product_2_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product2_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product2_Tab.IsEnabled = false;
                    }
                    else
                    {
                        long timestamp2 = long.Parse(product_2_timestamp);
                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                        long timestamp_difference = timestamp2 - unixTimeMilliseconds;
                        long timestamp_hours = timestamp_difference / 1000 / 60 / 60;
                        long timestamp_days = timestamp_difference / 1000 / 60 / 60 / 24;
                        if (timestamp_hours > 1)
                        {
                            Product2_Tab.IsEnabled = true;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                product_2_txt.Text = timestamp_days + " Days remaining.";
                                product_2_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product2_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                            else
                            {
                                product_2_txt.Text = "Lifetime";
                                product_2_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product2_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                        }
                        else
                        {
                            product_2_txt.Text = "Expired";
                            product_2_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product2_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product2_Tab.IsEnabled = false;
                        }
                    }

                    if (product_3_timestamp == "0")
                    {
                        product_3_txt.Text = "Never purchased";
                        product_3_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product3_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                        Product3_Tab.IsEnabled = false;
                    }
                    else
                    {
                        long timestamp3 = long.Parse(product_3_timestamp);
                        DateTimeOffset now = DateTimeOffset.UtcNow;
                        long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                        long timestamp_difference = timestamp3 - unixTimeMilliseconds;
                        long timestamp_hours = timestamp_difference / 1000 / 60 / 60;
                        long timestamp_days = timestamp_difference / 1000 / 60 / 60 / 24;
                        if (timestamp_hours > 1)
                        {
                            Product3_Tab.IsEnabled = true;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                product_3_txt.Text = timestamp_days + " Days remaining.";
                                product_3_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product3_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                            else
                            {
                                product_3_txt.Text = "Lifetime";
                                product_3_txt.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                                Product3_Tab.Foreground = new SolidColorBrush(Colors.LightSeaGreen);
                            }
                        }
                        else
                        {
                            product_3_txt.Text = "Expired";
                            product_3_txt.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product3_Tab.Foreground = new SolidColorBrush(Colors.IndianRed);
                            Product3_Tab.IsEnabled = false;
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("1 | Database Error!");
                Console.Write(ex);
            }
        }
        private void StartAutoLogOut()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Timer_LogOut);
            dispatcherTimer.Interval = new TimeSpan(12, 0, 0);
            dispatcherTimer.Start();
        }
        private void Timer_LogOut(object sender, EventArgs e)
        {
            LoginScreen loginwindow = new LoginScreen();
            loginwindow.Show();
            Close();
            MessageBox.Show("Your session has expired. Please log in again");
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
        private void CheckVersion()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT value FROM settings WHERE name = 'version'";

                MySqlDataReader reader = cmd.ExecuteReader();
                int cVersion;

                while (reader.Read())
                {
                    cVersion = Int32.Parse(reader.GetString("value"));
                    if (cVersion > thisVersionID)
                    {
                        MessageBox.Show("A new Update is available!");
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("VERSION | Please contact support!");
                Console.Write(ex);
            }
        }
        private void CheckUserState()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userdata WHERE username = @uname";
                cmd.Parameters.AddWithValue("@uname", cUsername);

                MySqlDataReader reader = cmd.ExecuteReader();

                bool state;
                while (reader.Read())
                {
                    state = reader.GetBoolean("admin");
                    if (state)
                    {
                        admin_panel.Visibility = Visibility.Visible;
                        admin_panel.IsEnabled = true;
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("STATE | Please contact support!");
                Console.Write(ex);
            }
        }
        private void SaveBt_Admin_User(object sender, RoutedEventArgs e)
        {
            if (adminUserEditMode)
            {


                admin_inputSearchUsername.IsReadOnly = false;
                admin_inputSearchUsername.Text = null;
                admin_UserHWID.Text = null;
                admin_UserPW.Text = null;
                admin_Product1.Text = null;
                admin_Product2.Text = null;
                admin_Product3.Text = null;
                adminUserEditMode = false;
            }
            else
            {
                MessageBox.Show("You cant save nothing");
            }
        }
        private void DeleteBt_Admin_User(object sender, RoutedEventArgs e)
        {
            if (adminUserEditMode)
            {
                MySqlCommand command = cnn.CreateCommand();
                command.CommandText = "DELETE FROM userdata WHERE username = @cusername";
                command.Parameters.AddWithValue("@cusername", admin_inputSearchUsername.Text);
                command.ExecuteNonQuery();

                MessageBox.Show("User '" + admin_inputSearchUsername.Text + "' got deleted");

                admin_inputSearchUsername.IsReadOnly = false;
                admin_inputSearchUsername.Text = null;
                admin_UserHWID.Text = null;
                admin_UserPW.Text = null;
                admin_Product1.Text = null;
                admin_Product2.Text = null;
                admin_Product3.Text = null;
                adminUserEditMode = false;
            }
            else
            {
                MessageBox.Show("You cant delete nothing");
            }
        }
        private void ManageUserSearch(object sender, RoutedEventArgs e)
        {
            if (adminUserEditMode)
            {
                MessageBox.Show("Please exit the current User with the 'save' button");
                return;
            }
            if (!string.IsNullOrEmpty(admin_inputSearchUsername.Text))
            {
                try
                {
                    tempcheck = true;

                    MySqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM userdata WHERE username = @uname";
                    cmd.Parameters.AddWithValue("@uname", admin_inputSearchUsername.Text);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    string hwid;
                    string timestamp1;
                    string timestamp2;
                    string timestamp3;
                    string passwordHashed;
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                    while (reader.Read())
                    {
                        hwid = reader.GetString("sysid");
                        timestamp1 = reader.GetString("product_1");
                        timestamp2 = reader.GetString("product_2");
                        timestamp3 = reader.GetString("product_3");
                        passwordHashed = reader.GetString("password");

                        adminUserEditMode = true;
                        admin_inputSearchUsername.IsReadOnly = true;

                        if (hwid == "notset")
                        {
                            admin_UserHWID.Text = "not set";
                        }
                        else
                        {
                            admin_UserHWID.Text = "exists";
                        }

                        if(passwordHashed == defaultPW_Hash)
                        {
                            admin_UserPW.Text = "default";
                        }else
                        {
                            admin_UserPW.Text = "not default";
                        }

                        long timestamp_difference1 = long.Parse(timestamp1) - unixTimeMilliseconds;
                        long timestamp_difference2 = long.Parse(timestamp2) - unixTimeMilliseconds;
                        long timestamp_difference3 = long.Parse(timestamp3) - unixTimeMilliseconds;

                        if (timestamp_difference1 <= 0)
                        {
                            admin_Product1.Text = "expired or never bought";
                        }
                        else
                        {
                            long timestamp_hours = timestamp_difference1 / 1000 / 60 / 60;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                long timestamp_days = timestamp_difference1 / 1000 / 60 / 60 / 24;
                                admin_Product1.Text = timestamp_days + " days left";
                            }
                            else
                            {
                                admin_Product1.Text = "lifetime";
                            }
                        }

                        if (timestamp_difference2 <= 0)
                        {
                            admin_Product2.Text = "expired or never bought";
                        }
                        else
                        {
                            long timestamp_hours = timestamp_difference2 / 1000 / 60 / 60;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                long timestamp_days = timestamp_difference2 / 1000 / 60 / 60 / 24;
                                admin_Product2.Text = timestamp_days + " days left";
                            }
                            else
                            {
                                admin_Product2.Text = "lifetime";
                            }
                        }

                        if (timestamp_difference3 <= 0)
                        {
                            admin_Product3.Text = "expired or never bought";
                        }
                        else
                        {
                            long timestamp_hours = timestamp_difference3 / 1000 / 60 / 60;
                            if (timestamp_hours < (100 * 365 * 24))
                            {
                                long timestamp_days = timestamp_difference3 / 1000 / 60 / 60 / 24;
                                admin_Product3.Text = timestamp_days + " days left";
                            }
                            else
                            {
                                admin_Product3.Text = "lifetime";
                            }
                        }
                        tempcheck = false;
                    }
                    reader.Close();

                    if (tempcheck)
                    {
                        MessageBox.Show("User does not exist");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ADMIN SEARCH | Please contact support!");
                    Console.Write(ex);
                }
            }
            else
            {
                MessageBox.Show("Please don't leave the username blank.");
            }
        }
        private void SearchLicense(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(license_txt_adminsearch.Text))
            {
                try
                {
                    tempcheck = true;

                    MySqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM licensekeys WHERE licensekey = @key";
                    cmd.Parameters.AddWithValue("@key", license_txt_adminsearch.Text);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                    while (reader.Read())
                    {
                        string p_id = reader.GetString("product_id");
                        string user = reader.GetString("username");
                        string timestamp = reader.GetString("timestamp");
                        string usage_time = reader.GetString("usage_timestamp");
                        string expiry_timestamp = reader.GetString("expiry_timestamp");

                        if(int.Parse(timestamp) > 25)
                        {
                            duration_txt_adminsearch.Text = "Lifetime";
                            expires_txt_adminsearch.Text = "Never";
                        }
                        else
                        {
                            DateTimeOffset expires = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expiry_timestamp));
                            DateTimeOffset expires_localtime = expires.LocalDateTime;
                            string expires_localtime_string = expires_localtime.ToString().Substring(0, 19);
                            expires_txt_adminsearch.Text = expires_localtime_string;
                            duration_txt_adminsearch.Text = timestamp + " months";
                        }

                        if(user != "0")
                        {
                            username_txt_adminsearch.Text = user;
                        }else
                        {
                            expires_txt_adminsearch.Text = "license unused";
                            username_txt_adminsearch.Text = "license unused";
                        }

                        proid_txt_adminsearch.Text = p_id;

                        tempcheck = false;
                    }
                    reader.Close();

                    if (tempcheck)
                    {
                        MessageBox.Show("License does not exist");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("LICENSE SEARCH | Please contact support!");
                    Console.Write(ex);
                }
            }
            else
            {
                MessageBox.Show("Please don't leave the license blank.");
            }
        }
        private void HWIDReset(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adminUserEditMode)
                {
                    if (admin_UserHWID.Text != "not set")
                    {
                        MySqlCommand cmd = cnn.CreateCommand();
                        cmd.CommandText = "UPDATE userdata SET sysid=default where username = @cuser;";
                        cmd.Parameters.AddWithValue("@cuser", admin_inputSearchUsername.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("HWID got reset");
                        admin_UserHWID.Text = "not set";
                    }
                    else
                    {
                        MessageBox.Show("The HWID is already undefined");
                    }
                }
                else
                {
                    MessageBox.Show("Please search a user first");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("242 | Please contact support!");
                Console.Write(ex);
            }
        }
        private void CreateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                if(admin_user_create_txt.Text.Length <= 6)
                {
                    MessageBox.Show("Please use a username longer than 6 characters");
                    return;
                }
                if(string.IsNullOrEmpty(admin_user_create_txt.Text))
                {
                    MessageBox.Show("Please choose a username");
                    return;
                }

                tempcheck = true;

                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM userdata WHERE username = @uname";
                cmd.Parameters.AddWithValue("@uname", admin_user_create_txt.Text);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tempcheck = false;
                }
                reader.Close();


                if (tempcheck)
                {
                    MySqlCommand command = cnn.CreateCommand();
                    command.CommandText = "INSERT INTO userdata SET username = @usern";
                    command.Parameters.AddWithValue("@usern", admin_user_create_txt.Text);
                    command.ExecuteNonQuery();
                    admin_user_output.Text = admin_user_create_txt.Text + ":" + defaultPWPlain;
                }
                else
                {
                    MessageBox.Show("This username is already taken");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CREATE USER | Please contact support!");
                Console.Write(ex);
            }
        }
        private void PWReset(object sender, RoutedEventArgs e)
        {
            try
            {
                if (adminUserEditMode)
                {
                    if (admin_UserHWID.Text != "not default")
                    {
                        MySqlCommand cmd = cnn.CreateCommand();
                        cmd.CommandText = "UPDATE userdata SET password=default where username = @cuser;";
                        cmd.Parameters.AddWithValue("@cuser", admin_inputSearchUsername.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Password got reset");
                        admin_UserPW.Text = "default";
                    }
                    else
                    {
                        MessageBox.Show("The Password is already default");
                    }
                }
                else
                {
                    MessageBox.Show("Please search a user first");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("243 | Please contact support!");
                Console.Write(ex);
            }
        }
        private void ReturnDefaultPassword()
        {
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT value FROM settings WHERE name = @settingsname";
                cmd.Parameters.AddWithValue("@settingsname", "default_pw");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    defaultPW_Hash = reader.GetString("value");
                }
                reader.Close();

                MySqlCommand cmd1 = cnn.CreateCommand();
                cmd1.CommandText = "SELECT value FROM settings WHERE name = @settingsname1";
                cmd1.Parameters.AddWithValue("@settingsname1", "defaultPWPlain");

                MySqlDataReader reader1 = cmd1.ExecuteReader();

                while (reader1.Read())
                {
                    defaultPWPlain = reader1.GetString("value");
                }
                reader1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("676 | Please contact support!");
                Console.Write(ex);
            }
        }
        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            AdminSettingsLoaded = true;
            try
            {
                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT value FROM settings  WHERE name = @settingsname";
                cmd.Parameters.AddWithValue("@settingsname", "help_link");

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    help_link_settings = reader.GetString("value");
                }
                reader.Close();

                MySqlCommand cmd1 = cnn.CreateCommand();
                cmd1.CommandText = "SELECT value FROM settings WHERE name = @settingsname1";
                cmd1.Parameters.AddWithValue("@settingsname1", "version");

                MySqlDataReader reader1 = cmd1.ExecuteReader();

                while (reader1.Read())
                {
                    version_settings = reader1.GetString("value");
                }
                reader1.Close();

                MySqlCommand cmd3 = cnn.CreateCommand();
                cmd3.CommandText = "SELECT value FROM settings  WHERE name = @settingsname";
                cmd3.Parameters.AddWithValue("@settingsname", "defaultPWPlain");

                MySqlDataReader reader3 = cmd3.ExecuteReader();

                while (reader3.Read())
                {
                    defaultPWPlain_Admin = reader3.GetString("value");
                }
                reader3.Close();

                help_link_setting_txt.Text = help_link_settings;
                version_txt.Text = version_settings;
                password_txt_admin.Text = defaultPWPlain_Admin;
            }
            catch (Exception ex)
            {
                MessageBox.Show("6876 | Please contact support!");
                Console.Write(ex);
            }
        }
        private void ChangeVersionDefault(object sender, RoutedEventArgs e)
        {
            if(AdminSettingsLoaded)
            {
                if (String.IsNullOrEmpty(version_txt.Text))
                {
                    MessageBox.Show("Please use more than 6 characters");
                    return;
                }
                if (version_txt.Text != version_settings)
                {
                    MySqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "UPDATE settings SET value=@newvalue where name = @settingname;";
                    cmd.Parameters.AddWithValue("@newvalue", version_txt.Text);
                    cmd.Parameters.AddWithValue("@settingname", "version");

                    cmd.ExecuteNonQuery();
                    version_settings = version_txt.Text;
                    MessageBox.Show("Version updated.");
                }else
                {
                    MessageBox.Show("Nothing would be changed.");
                }
            }
            else
            {
                MessageBox.Show("Please reload the settings first!");
            }
        }
        private void ChangeHelpLinkDefault(object sender, RoutedEventArgs e)
        {
            if (AdminSettingsLoaded)
            {
                if(String.IsNullOrEmpty(help_link_setting_txt.Text) || help_link_setting_txt.Text.Length <= 6)
                {
                    MessageBox.Show("Please use more than 6 characters");
                    return;
                }
                if (help_link_setting_txt.Text != help_link_settings)
                {
                    MySqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "UPDATE settings SET value=@newvalue where name = @settingname;";
                    cmd.Parameters.AddWithValue("@newvalue", help_link_setting_txt.Text);
                    cmd.Parameters.AddWithValue("@settingname", "help_link");

                    cmd.ExecuteNonQuery();
                    help_link_settings = help_link_setting_txt.Text;
                    MessageBox.Show("Help-Link updated.");
                }
                else
                {
                    MessageBox.Show("Nothing would be changed.");
                }
            }
            else
            {
                MessageBox.Show("Please reload the settings first!");
            }
        }
        private void ChangePasswortDefault(object sender, RoutedEventArgs e)
        {
            if (AdminSettingsLoaded)
            {
                if (String.IsNullOrEmpty(password_txt_admin.Text) || password_txt_admin.Text.Length <= 6)
                {
                    MessageBox.Show("Please use more than 6 characters");
                    return;
                }
                if (password_txt_admin.Text != defaultPWPlain_Admin)
                {
                    string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(password_txt_admin.Text);

                    MySqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "UPDATE settings SET value=@newvalue where name = @settingname;";
                    cmd.Parameters.AddWithValue("@newvalue", password_txt_admin.Text);
                    cmd.Parameters.AddWithValue("@settingname", "defaultPWPlain");
                    cmd.ExecuteNonQuery();
                    MySqlCommand cmd1 = cnn.CreateCommand();
                    cmd1.CommandText = "UPDATE settings SET value=@newvalue where name = @settingname;";
                    cmd1.Parameters.AddWithValue("@newvalue", newHashedPassword);
                    cmd1.Parameters.AddWithValue("@settingname", "default_pw");
                    cmd1.ExecuteNonQuery();
                    MySqlCommand cmd2 = cnn.CreateCommand();
                    cmd2.CommandText = "ALTER TABLE userdata ALTER password SET DEFAULT @newvalue;";
                    cmd2.Parameters.AddWithValue("@newvalue", newHashedPassword);
                    cmd2.ExecuteNonQuery();

                    defaultPWPlain_Admin = password_txt_admin.Text;
                    defaultPWPlain = password_txt_admin.Text;
                    defaultPW_Hash = newHashedPassword;
                    MessageBox.Show("DefaultPassword updated!");
                }
                else
                {
                    MessageBox.Show("Nothing would be changed.");
                }
            }
            else
            {
                MessageBox.Show("Please reload the settings first!");
            }
        }
        private void StartLicenseCreation(object sender, RoutedEventArgs e)
        {
            int licenseToCreate = 0;
            if (IsParsable(license_creation_amount.Text))
            {
                licenseToCreate = int.Parse(license_creation_amount.Text);
            }else
            {
                MessageBox.Show("Please enter a number");
                return;
            }
            if(licenseToCreate <= 0)
            {
                MessageBox.Show("Cant generate less then 1 license");
                return;
            }
            if (licenseToCreate > 100)
            {
                MessageBox.Show("Cant generate more then 100 license");
                return;
            }

            ComboBoxItem product = (ComboBoxItem)product_id_create_licenses.SelectedItem;
            string product_string = product.Content.ToString();
            string product_id = "0";

            switch (product_string)
            {
                case "Product 1":
                    {
                        product_id = "1";
                        break;
                    }
                case "Product 2":
                    {
                        product_id = "2";
                        break;
                    }
                case "Product 3":
                    {
                        product_id = "3";
                        break;
                    }
            }

            ComboBoxItem time = (ComboBoxItem)license_time_create_licenses.SelectedItem;
            string time_string = time.Content.ToString();
            string real_time_license = "0";

            switch(time_string)
            {
                case "1 Month":
                    {
                        real_time_license = "1";
                        break;
                    }
                case "3 Months":
                    {
                        real_time_license = "3";
                        break;
                    }
                case "12 Months":
                    {
                        real_time_license = "12";
                        break;
                    }
                case "Lifetime":
                    {
                        real_time_license = "3000";
                        break;
                    }
            }

            try
            {
                string txt_string = "INSERT INTO licensekeys(licensekey, product_id, timestamp) VALUES ";

                LicenseKeys_Output.Text = null;

                for (int i = 0; i < licenseToCreate; i++)
                {
                    if (i == licenseToCreate - 1)
                    {
                        string key = GenerateLicenseKey();
                        txt_string += "('" + key + "','" + product_id + "','" + real_time_license + "');";
                        LicenseKeys_Output.AppendText(key);
                        LicenseKeys_Output.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        string key = GenerateLicenseKey();
                        txt_string += "('" + key + "','" + product_id + "','" + real_time_license + "'),";
                        LicenseKeys_Output.AppendText(key);
                        LicenseKeys_Output.AppendText(Environment.NewLine);
                    }
                }

                MySqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = txt_string;
                cmd.ExecuteNonQuery();
            }
            catch(MySqlException ex)
            {
                MessageBox.Show(ex.Message);

                if(ex.ErrorCode == 1062)
                {
                    MessageBox.Show("Error Code 1482 | Please try again.");
                }
                else
                {
                    MessageBox.Show("81651 | Please contact support");

                }
            }
        }
        
        public string GenerateLicenseKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public Boolean IsParsable(String input)
        {
            try
            {
                int.Parse(input);
                return true;
            }
            catch (Exception ex) {
                return false;
            }
            }
        }
}
