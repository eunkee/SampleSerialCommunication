using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace SampleSerialCommunication
{
    class InterfaceMcuUart
    {
        private SerialPort serialPort = null;
        private static Form1 Parentform;

        //data global variable
        private byte[] globalBuff = new byte[12];
        private int globalBuffLength = 0;

        public InterfaceMcuUart(Form1 form)
        {
            Parentform = form;
            Array.Clear(globalBuff, 0x00, globalBuff.Length);
        }

        public static bool GetIsPortExistConfirm(string portName)
        {
            bool rslt = false;
            if (portName.Length > 0)
            {
                string[] comlist = System.IO.Ports.SerialPort.GetPortNames();
                foreach (string com in comlist)
                {
                    if (com == portName)
                    {
                        rslt = true;
                        break;
                    }
                }
            }
            return rslt;
        }

        public void DisconnectSerialPort()
        {
            try
            {
                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.DataReceived -= new SerialDataReceivedEventHandler(ReceiveDataSerialPort);
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();

                        try
                        {
                            if (serialPort != null)
                            {
                                if (serialPort.IsOpen)
                                {
                                    serialPort.Close();
                                    serialPort.Dispose();
                                    serialPort = null;
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            System.Diagnostics.Trace.WriteLine("Failed: Interface DisconnectserialPort Dispose");
                            System.Diagnostics.Trace.WriteLine(e.Message);
                            System.Diagnostics.Trace.WriteLine(e.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Failed: Interface DisconnectserialPort");
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }

        public void CreateConnectSerialPort(string portName)
        {
            if (portName.Length > 0)
            {
                if (GetIsPortExistConfirm(portName))
                {
                    Task.Run(() =>
                    {
                        serialPort = new SerialPort
                        {
                            PortName = portName,
                            DataBits = 8,
                            Parity = Parity.None,
                            StopBits = StopBits.One,
                            Encoding = Encoding.ASCII,
                            BaudRate = 115200
                        };
                        serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveDataSerialPort);

                        try
                        {
                            serialPort.Open();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine("Failed: Open serialPort");
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Failed: No Exist serialPort");
                }
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Failed: No Exist Registry serialPort");
            }
        }

        public void ReconnectSerialPort(string portName)
        {
            if (portName.Length > 0)
            {
                try
                {
                    DisconnectSerialPort();
                    CreateConnectSerialPort(portName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Failed: Interface ReconnectSerialPort");
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                }
            }
        }

        public void SendSerialData(string data)
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        byte[] signal = Encoding.UTF8.GetBytes(data);
                        serialPort.Write(signal, 0, signal.Length);
                        Parentform.SetLogText($"Send: {BitConverter.ToString(signal)}");
                    }
                    catch { }
                }
            }
        }

        public void SendSerialData(byte[] data)
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Write(data, 0, data.Length);
                        Parentform.SetLogText($"Send: {BitConverter.ToString(data)}");
                    }
                    catch { }
                }
            }
        }

        private void ReceiveDataSerialPort(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort == null)
            {
                return;
            }
            if (serialPort.IsOpen)
            {
                try
                {
                    int len = serialPort.BytesToRead;
                    if (len == 0)
                    {
                        return;
                    }
                    byte[] buff = new byte[len];
                    //Seleted Read. Bacause: ReadByte -> low speed, ReadLine -> exception
                    serialPort.Read(buff, 0, len);

                    for (int i = 0; i < len; i++)
                    {
                        DataPhashing(buff[i]);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Failed: Interface ReceiveDataSerialPort");
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                }
            }
        }

        //example data phashing
        private void DataPhashing(int readByte)
        {
            //System.Diagnostics.Trace.WriteLine($"readByte: {readByte}");
            //0x02
            if (readByte == 2)
            {
                Array.Clear(globalBuff, 0x00, globalBuff.Length);
                globalBuffLength = 0;
            }
            //0x03
            if (readByte != 3)
            {
                if (globalBuffLength > 8)
                {
                    Array.Clear(globalBuff, 0x00, globalBuff.Length);
                    globalBuffLength = 0;
                }
                else
                {
                    globalBuff[globalBuffLength] = (byte)readByte;
                    globalBuffLength++;
                }
                return;
            }

            //check length -> init exception
            if (globalBuffLength != 7 && globalBuffLength != 8)
            {
                Array.Clear(globalBuff, 0x00, globalBuff.Length);
                globalBuffLength = 0;
                return;
            }

            //case 0x03
            globalBuff[globalBuffLength] = (byte)readByte;

            if (globalBuffLength >= 7)
            {
                globalBuff[globalBuffLength] = (byte)readByte;
                byte[] buff = new byte[globalBuffLength + 1];

                //case end
                Array.Copy(globalBuff, 0, buff, 0, buff.Length);
                Array.Clear(globalBuff, 0x00, globalBuff.Length);
                globalBuffLength = 0;
                //printf hex data
                //System.Diagnostics.Trace.WriteLine($"Hex: {BitConverter.ToString(buff)}");

                //this case length == 8
                if (buff.Length == 8)
                {
                    if (SensorDataParsing(buff, out byte[] data))
                    {
                        if (data != null)
                        {
                            //data
                            Parentform.SetLogText($"Receive: {BitConverter.ToString(data)}");
                        }
                    }
                }
            }
            else
            {
                Array.Clear(globalBuff, 0x00, globalBuff.Length);
                globalBuffLength = 0;
            }
        }

        //extraction real data + check checksum
        public static bool SensorDataParsing(byte[] buff, out byte[] result)
        {
            bool rslt = false;
            result = new byte[5];
            byte[] temp = new byte[6];
            for (var i = 0; i < buff.Length; i++)
            {
                //찾는 데이터 총 길이: 8
                if (buff.Length > i + 7)
                {
                    //STX && ETX
                    if (buff[i] == 0x02 && buff[i + 7] == 0x03)
                    {
                        //except STX & ETX
                        Array.Copy(buff, i + 1, temp, 0, 6);
                        if (CheckReceiveChecksum(temp))
                        {
                            //except checksum -> real data
                            Array.Copy(buff, i + 1, result, 0, 5);
                            rslt = true;
                            break;
                        }
                    }
                }
            }
            return rslt;
        }

        //checksum
        private static bool CheckReceiveChecksum(byte[] buff)
        {
            bool rslt = false;
            byte[] preData = new byte[buff.Length - 1];
            Buffer.BlockCopy(buff, 0, preData, 0, buff.Length - 1);
            if (PlusChecksum(preData) == buff[buff.Length - 1])
            {
                rslt = true;
            }
            return rslt;
        }

        //checksum type
        private static byte PlusChecksum(byte[] byteToCalculate)
        {
            uint checksum = 0;
            foreach (byte chData in byteToCalculate)
            {
                checksum += chData;
            }
            if (checksum > 255)
            {
                checksum %= 256;
            }
            checksum += 256;
            return Convert.ToByte(checksum);
        }
    }
}
