using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using acRemoteServerUDP_Example;

public partial class MainForm : Form
{
    TextBox udpIpTextBox = new TextBox() { Top = 10, Left = 10 };
    TextBox udpPortTextBox = new TextBox() { Top = 40, Left = 10 };
    TextBox wsIpTextBox = new TextBox() { Top = 70, Left = 10 };
    TextBox wsPortTextBox = new TextBox() { Top = 100, Left = 10 };
    Button connectButton = new Button() { Text = "Connect", Top = 130, Left = 10 };

    public MainForm()
    {
        this.Text = "Telemetry Client";
        this.Controls.Add(udpIpTextBox);
        this.Controls.Add(udpPortTextBox);
        this.Controls.Add(wsIpTextBox);
        this.Controls.Add(wsPortTextBox);
        this.Controls.Add(connectButton);

        // Use placeholder text without relying on color changes
        udpIpTextBox.Text = "Enter UDP IP";
        udpPortTextBox.Text = "Enter UDP Port";
        wsIpTextBox.Text = "Enter WebSocket IP";
        wsPortTextBox.Text = "Enter WebSocket Port";

        connectButton.Click += ConnectButton_Click;
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Get IP and Port values from the TextBoxes
            string udpIp = udpIpTextBox.Text;
            int udpPort = int.Parse(udpPortTextBox.Text);
            string wsIp = wsIpTextBox.Text;
            int wsPort = int.Parse(wsPortTextBox.Text);

            // Call the modified Run method in Program.cs with UI input values
            await Program.Run(udpIp, udpPort, wsIp, wsPort);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
}
