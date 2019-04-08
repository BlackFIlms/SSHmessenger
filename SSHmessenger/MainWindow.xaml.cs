/*
 * OpenSource SSH messenger
 * Designed by BlackFilms in 2019.
 * For fast send message to SSH protocol.
 * Version: 1.1.1.1
 * 
 * Version guide: a.a.b.c
 * a - version number.
 * b - build number.
 * c - release number(0 - Release version, 1 - Beta version, 2 - Dev version).
 * 
 * It's betta version messenger.
 * Perhaps there are bugs and errors.
 * 
 * Remark: in this version messenger, passwords don't encrypted!
 * Perhaps in the future, this feature will appear.
*/

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;

namespace SSHmessenger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Define SshClient.
        PasswordConnectionInfo connectionInfo;
        SshClient sshClientInit;

        public MainWindow()
        {
            InitializeComponent();

            // Create the UI elements.
            Button ConnectToHost = new Button();

            // Set properties on elements.
            ConnectToHost.Content = "Connect";
            ConnectToHost.Margin = new Thickness(0, 10, 0, 0);
            ConnectToHost.Width = 100;
            ConnectToHost.Height = 20;

            // Attach event handler.
            ConnectToHost.Click += new RoutedEventHandler(ButtonClickConnect);

            // Attach Elements to Grid.
            connectionData.Children.Capacity++;
            connectionData.Children.Insert(connectionData.Children.Count, ConnectToHost);
            
            // Set position for elements.
            Grid.SetColumn(ConnectToHost, 1);
            Grid.SetRow(ConnectToHost, 6);

            /* Set values for connection.
             * If you have saved data for connection in file C:\Users\Public\Documents\SshData.txt
             * Then used this data.
             * Else used default values.
            */
            try
            {
                ReadDataFromFile(out string IP, out string Port, out string User, out string Password);
                IPField.Text = IP;
                PortField.Text = Port;
                UserField.Text = User;
                PassField.Password = Password;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            //Init connection data.
            InitConnectionData();
        }

        ~MainWindow()
        {
            try
            {
                sshClientInit.Disconnect();
                sshClientInit.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
        }

        private void ButtonClickConnect(object sender, RoutedEventArgs e) //Processing button click.
        {
            InitConnectionData();
            ConnectToServer(sshClientInit);
        }
        private void InitConnectionData()
        {
            // Init ssh client object.
            try
            {
                // Set values for connection.
                connectionInfo = new PasswordConnectionInfo(IPField.Text, Convert.ToInt32(PortField.Text), UserField.Text, PassField.Password);
                connectionInfo.Timeout = TimeSpan.FromSeconds(10);

                // Init SSH object.
                sshClientInit = new SshClient(connectionInfo);
                //sshClient.KeepAliveInterval = TimeSpan.FromSeconds(30);

                // CheckConnectOnTime
                System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
                timer.AutoReset = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(CheckConnectOnTime);
                timer.Start();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Заполните все поля!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат входных данных.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
        }
        private void ConnectToServer(SshClient sshClient)
        {
            // Connect.
            //using (sshClient) {ConnectionCode} - If use this, then sshClient will be Dispose, when using finished his job.
            try
            {
                if (sshClient.IsConnected)
                {
                    MessageBox.Show("You connected eraly");
                }
                else if (sshClient.IsConnected == false)
                {
                    sshClient.Connect();
                    Status.Text = "SSH connection active";
                    Status.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    Status.Text = "You are not connected.";
                    Status.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Неверный формат данных!");
            }
            catch (SshAuthenticationException ex)
            {
                MessageBox.Show("Problem with autentication." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
            catch (SshConnectionException ex)
            {
                MessageBox.Show("Problem with connection." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
            catch (SshException ex)
            {
                MessageBox.Show("Some problems with SSH." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
        }
        private bool CheckConnect(SshClient sshClient) //Check connect for ConnectToServer method.
        {
            try
            {
                if (sshClient.IsConnected)
                {
                    Status.Text = "SSH connection active";
                    Status.Foreground = new SolidColorBrush(Colors.Green);
                    return true;
                }
                else
                {
                    Status.Text = "You are not connected.";
                    Status.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBoxResult noConnection = MessageBox.Show("You are not connected." + "\r\n" + "Want to connect?", "No cennection.", MessageBoxButton.YesNo);
                    if (noConnection == MessageBoxResult.Yes)
                    {
                        InitConnectionData();
                        ConnectToServer(sshClient);
                        CheckConnect(sshClient);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        private void CheckConnectOnTime(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sshClientInit.IsConnected)
            {
                Status.Dispatcher.BeginInvoke(new Action(delegate () //Enables asynchronous calls from multiple threads.
                {
                    Status.Text = "SSH connection active";
                    Status.Foreground = new SolidColorBrush(Colors.Green);
                }
                ));
            }
            else
            {
                Status.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    Status.Text = "You are not connected.";
                    Status.Foreground = new SolidColorBrush(Colors.Red);
                }
                ));
            }
        }
        
        private void ButtonClickSend(object sender, RoutedEventArgs e) //Processing button click.
        {
            try
            {
                if (CheckConnect(sshClientInit))
                {
                    sshClientInit.RunCommand("msg * " + MessageField.Text);
                    MessageBox.Show("Успешно отправлено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            string IP = IPField.Text;
            string Port = PortField.Text;
            string User = UserField.Text;
            string Password = PassField.Password;

            //The password is not encrypted when saving, therefore, it is not recommended to save it.
            if (!String.IsNullOrWhiteSpace(Password) || !String.IsNullOrEmpty(Password))
            {
                MessageBoxResult warningPassword = MessageBox.Show("Не рекомендуется сохранять пароль, т.к. он сохраняется в незашифрованном виде." + "\r\n" + "Всё равно сохранить?", "Save not encrypted password.", MessageBoxButton.YesNo);
                if (warningPassword == MessageBoxResult.Yes)
                {
                    SaveDataToFile(IP, Port, User, Password);
                }
                else if (warningPassword == MessageBoxResult.No)
                {
                    SaveDataToFile(IP, Port, User);
                }
            }
            else if (String.IsNullOrWhiteSpace(Password) || String.IsNullOrEmpty(Password))
            {
                SaveDataToFile(IP, Port, User);
            }
        }
        private void SaveDataToFile(string IP, string Port, string User, string Password = "")
        {
            //Path to file for save settings.
            string path = @"C:\Users\Public\Documents\SshData.txt";
            //Save data.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("IP: " + IP);
                sw.WriteLine("Port: " + Port);
                sw.WriteLine("User: " + User);
                sw.WriteLine("Password: " + Password);
            }
        }
        private void ReadDataFromFile(out string IP, out string Port, out string User, out string Password)
        {
            //Set default values.
            IP = "0.0.0.0";
            Port = "22";
            User = "";
            Password = "";

            //Path for read saved settings.
            string path = @"C:\Users\Public\Documents\SshData.txt";
            if (File.Exists(path))
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    //Init list.
                    List<string> dataLines = new List<string>();
                    //Get data and insert to list.
                    while (sr.EndOfStream != true)
                    {
                        dataLines.Add(sr.ReadLine());
                    }
                    
                    //Get data and insert to out vars.
                    IP = System.Text.RegularExpressions.Regex.Replace(dataLines.Find(x => x.Contains("IP: ")), @"IP: ", "");
                    Port = System.Text.RegularExpressions.Regex.Replace(dataLines.Find(x => x.Contains("Port: ")), @"Port: ", "");
                    User = System.Text.RegularExpressions.Regex.Replace(dataLines.Find(x => x.Contains("User: ")), @"User: ", "");
                    Password = System.Text.RegularExpressions.Regex.Replace(dataLines.Find(x => x.Contains("Password: ")), @"Password: ", "");
                }
            }
        }

        //Don't work.
        /*private void SaveData_MouseEnter(object sender, MouseEventArgs e)
        {
            SaveData.BorderThickness = new Thickness(0);
            ImageBrush img = new ImageBrush();
            img.ImageSource = new BitmapImage(new Uri("../../pic/save-icon-dark-resized.png", UriKind.Relative));
            img.Stretch = Stretch.UniformToFill;
            SaveData.Background = img;
        }

        private void SaveData_MouseLeave(object sender, MouseEventArgs e)
        {
            SaveData.BorderThickness = new Thickness(0);
            ImageBrush img = new ImageBrush();
            img.ImageSource = new BitmapImage(new Uri("../../pic/save-icon-white-resized.png", UriKind.Relative));
            img.Stretch = Stretch.UniformToFill;
            SaveData.Background = img;
        }*/
    }
}