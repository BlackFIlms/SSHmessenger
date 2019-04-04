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

namespace SSHmessenger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Define SshClient.
        PasswordConnectionInfo connectionInfo;
        SshClient sshClient;

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
        }

        ~MainWindow()
        {
            try
            {
                sshClient.Disconnect();
                sshClient.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error." + "\r\n" + "Details:" + "\r\n" + ex.ToString());
            }
        }

        private void ButtonClickConnect(object sender, RoutedEventArgs e) //Processing button click.
        {
            ConnectToServer();
        }
        private void ConnectToServer()
        {
            // Init.
            try
            {
                // Set values for connection.
                connectionInfo = new PasswordConnectionInfo(IPField.Text, Convert.ToInt32(PortField.Text), UserField.Text, PassField.Password);
                connectionInfo.Timeout = TimeSpan.FromSeconds(10);

                // Init SSH object.
                sshClient = new SshClient(connectionInfo);
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
        private bool CheckConnect() //Check connect for ConnectToServer method.
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
                        ConnectToServer();
                        CheckConnect();
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
            if (sshClient.IsConnected)
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
                if (CheckConnect())
                {
                    sshClient.RunCommand("msg * " + MessageField.Text);
                    MessageBox.Show("Успешно отправлено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}