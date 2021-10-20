using System;
using System.Linq;
using System.Windows.Forms;

namespace SampleSerialCommunication
{
    public partial class Form1 : Form
    {
        private readonly ControlLog controlLog = new();
        private InterfaceMcuUart interfaceMcuUart;
        //serial port list
        private string[] comlist;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            interfaceMcuUart = new(this);
            PanelLog.Controls.Add(controlLog);

            //serialport list
            comlist = System.IO.Ports.SerialPort.GetPortNames();
            if (comlist.Length > 0)
            {
                comboBox1.Items.AddRange(comlist);
            }

            //select serialport
            string serialPort = SerialCommunicationRegistry.SerialPort;
            if (serialPort.Length > 0)
            {
                comboBox1.Text = serialPort;
            }
        }

        public void SetLogText(string text)
        {
            if (controlLog != null)
            {
                controlLog.SetLogText(text);
            }
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (interfaceMcuUart == null)
            {
                return;
            }

            string serialPort = comboBox1.Text;
            if (serialPort.Length > 0)
            {
                SerialCommunicationRegistry.SerialPort = serialPort;
                interfaceMcuUart.ReconnectSerialPort(serialPort);
            }
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            //case string
            string data = textBox1.Text;
            if (data.Length > 0)
            {
                if (interfaceMcuUart != null)
                {
                    try
                    {
                        interfaceMcuUart.SendSerialData(data);
                    }
                    catch { }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (interfaceMcuUart != null)
            {
                try
                {
                    interfaceMcuUart.DisconnectSerialPort();
                }
                catch { }
            }
        }

        //refresh serialport list
        private void ComboBox1_DropDown(object sender, EventArgs e)
        {
            if (comlist != null)
            {
                string[] new_comlist = System.IO.Ports.SerialPort.GetPortNames();
                if (!Enumerable.SequenceEqual(comlist, new_comlist))
                {
                    string OldText1 = comboBox1.Text;
                    comboBox1.Items.Clear();
                    comlist = new_comlist;
                    comboBox1.Items.AddRange(comlist);
                    comboBox1.Text = OldText1;
                }
            }
        }
    }
}
