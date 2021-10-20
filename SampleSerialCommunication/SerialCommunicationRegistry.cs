using Microsoft.Win32;
using System;

namespace SampleSerialCommunication
{
    class SerialCommunicationRegistry
    {
        public static RegistryKey RegKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SampleSerialCommunication", true);
        private static readonly object regObject = new();
        public static string SerialPort
        {
            get
            {
                string rslt = string.Empty;
                if (RegKey != null)
                {
                    lock (regObject)
                    {
                        try
                        {
                            rslt = Convert.ToString(RegKey.GetValue("SerialPort", rslt));
                        }
                        catch { }
                    }
                }
                return rslt;
            }
            set
            {
                lock (regObject)
                {
                    if (RegKey != null)
                    {
                        try
                        {
                            RegKey.SetValue("SerialPort", value);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
