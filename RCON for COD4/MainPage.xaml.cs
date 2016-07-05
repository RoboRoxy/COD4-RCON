using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace RCON_for_COD4
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MessageDialog msgbox;
        DatagramSocket socket = new DatagramSocket();
        string host = "", port = "";
        bool hasError = false;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            socket.MessageReceived += socket_MessageReceived;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        public async Task sendCommand(string rconCommand, string gameServerIP, string password, string gameServerPort)
        {
            rconCommand = "rcon " + password + " " + rconCommand;
            using (var stream = await socket.GetOutputStreamAsync(new HostName(gameServerIP), gameServerPort))
            {
                using (var writer = new DataWriter(stream))
                {
                    byte[] bufferTemp = Encoding.UTF8.GetBytes(rconCommand);
                    byte[] bufferSend = new byte[bufferTemp.Length + 5];

                    bufferSend[0] = byte.Parse("255");
                    bufferSend[1] = byte.Parse("255");
                    bufferSend[2] = byte.Parse("255");
                    bufferSend[3] = byte.Parse("255");
                    bufferSend[4] = byte.Parse("02");
                    int j = 5;

                    for (int i = 0; i < bufferTemp.Length; i++)
                    {
                        bufferSend[j++] = bufferTemp[i];
                    }

                    writer.WriteBytes(bufferSend);
                    await writer.StoreAsync();
                }
            }
        }

        async void socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            IInputStream result = args.GetDataStream();
            var resultStream = result.AsStreamForRead(1024);
            string text = "";

            using (var reader = new StreamReader(resultStream))
            {
                text = await reader.ReadToEndAsync();
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtServerResponse.Text = text;
            });
        }

        async Task ValidateAndSendRconCommand()
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text.Trim()))
            {
                msgbox = new MessageDialog("Please enter the host name/IP and port", "No host name/IP and port entered");
                await msgbox.ShowAsync();
                return;
            }

            try
            {
                host = txtHost.Text.Split(':')[0];
                port = txtHost.Text.Split(':')[1];
            }
            catch (Exception)
            {
                hasError = true;
            }
            if (hasError)
            {
                msgbox = new MessageDialog("Please enter the host name/IP and port correctly", "Invalid format of host name/IP and port entered");
                await msgbox.ShowAsync();
                hasError = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(port.Trim()))
            {
                msgbox = new MessageDialog("Please enter port after IP", "No port entered");
                await msgbox.ShowAsync();
                return;
            }

            int intport = 0;

            if (!int.TryParse(port, out intport))
            {
                msgbox = new MessageDialog("Please enter port in integer", "Invalid port entered");
                await msgbox.ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password.Trim()))
            {
                msgbox = new MessageDialog("Please enter the password", "No password entered");
                await msgbox.ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCommandText.Text.Trim()))
            {
                msgbox = new MessageDialog("Please enter the command", "No command entered");
                await msgbox.ShowAsync();
                return;
            }

            await sendCommand(txtCommandText.Text, host, txtPassword.Password, port);
        }

        async private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            await ValidateAndSendRconCommand();
        }
    }
}
