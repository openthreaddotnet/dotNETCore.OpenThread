using System;
using System.IO.Ports;
using System.Text;
using dotNETCore.OpenThread.NCP;
using dotNETCore.OpenThread.Net;
using dotNETCore.OpenThread.Net.Lowpan;
using dotNETCore.OpenThread.Net.Sockets;
using dotNETCore.OpenThread.Spinel;

namespace Samples
{
    class FormLowpanNetworkAndUDPListener
    {      
        private static ushort port = 1234;
        private static string networkname = "OpenThreadCore";
        private static string masterkey = "00112233445566778899aabbccddeeff";
        private static byte channel = 11;
        private static ushort panid = 1000;

        private static LoWPAN loWPAN = new LoWPAN();

        static void Main(string[] args)
        {           
            if (args.Length != 1)
            {
                string[] ports = SerialPort.GetPortNames();
                Console.WriteLine("COM port parameter not provided.");
                Console.WriteLine("Available serial ports: ");
                foreach (var serialPort in ports)
                {
                    Console.WriteLine(serialPort);
                }
                Console.ReadKey();
                return;
            }
            
            try
            {
                loWPAN.Open(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }

            var lowpanScanner = loWPAN.LowpanScanner;
            LowpanBeaconInfo[] lowpannets = lowpanScanner.ScanBeacon();

            string tempString;
         
            Console.WriteLine("Enter new values or press enter to keep default values.");
            Console.WriteLine();

            bool netNotExists = true;

            while (netNotExists)
            {
                Console.Write("Networkname: {0} ? ", networkname);
                tempString = Console.ReadLine();

                if (tempString != string.Empty && tempString != networkname)
                {
                    networkname = tempString;
                }

                netNotExists = false;

                foreach (var lowpannet in lowpannets)
                {
                    if (lowpannet.NetworkName == networkname)
                    {
                        netNotExists = true;
                        Console.WriteLine("Network exists, please try again.");
                        break;
                    }
                }
            }
                   
            Console.Write("Channel:  {0} ? ", channel.ToString());
            tempString = Console.ReadLine();
            if (tempString != string.Empty && Convert.ToByte(tempString) != channel)
            {
                channel = Convert.ToByte(tempString);
            }
           
            Console.Write("Masterkey: {0} ? ", masterkey);
            tempString = Console.ReadLine();           
            if (tempString != string.Empty && masterkey != tempString)
            {
                masterkey = tempString;
            }

            Console.Write("Panid: {0} ? ", panid);
            tempString = Console.ReadLine();
            if (tempString != string.Empty && Convert.ToUInt16(tempString) != panid)
            {
                panid = Convert.ToUInt16(tempString);
            }
        
            Console.Write("Listener port: {0} ? ", port);
            tempString = Console.ReadLine();
            if (tempString != string.Empty && Convert.ToUInt16(tempString) != port)
            {
                port = Convert.ToUInt16(tempString);
            }
           
            loWPAN.OnLowpanPropertyChanged += LoWPAN_OnLowpanPropertyChanged;
            
            try
            
            {
                loWPAN.Form(networkname, channel, masterkey, panid);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }
        
            Socket receiver = new Socket();
            receiver.Bind(IPAddress.Any, port);
            IPEndPoint remoteIp = null;

            IPAddress[] iPAddresses = loWPAN.IPAddresses;

            Console.WriteLine("UDP Server listening.");

            while (true)
            {
                if (receiver.Poll(-1, SelectMode.SelectRead))
                {
                    byte[] data = receiver.Receive(ref remoteIp);
                    string message = Encoding.ASCII.GetString(data);
                    Console.WriteLine("\n");
                    Console.WriteLine("{0} bytes from {1} {2} {3}", message.Length, remoteIp.Address, remoteIp.Port, message);
                    Console.WriteLine(">");
                }
            }
        }

        private static void LoWPAN_OnLowpanPropertyChanged(uint PropertyId)
        {
            if(PropertyId== SpinelProperties.SPINEL_PROP_LAST_STATUS)
            {
                if(loWPAN.LastStatus!= SpinelStatus.SPINEL_STATUS_OK)                    
                {
                    Console.WriteLine(loWPAN.LastStatus.ToString());
                }
            }
        }   
    }
}
