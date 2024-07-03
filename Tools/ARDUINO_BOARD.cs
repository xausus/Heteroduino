using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Grasshopper;

namespace Heteroduino
{


    public enum BoardType
        {
            NAN,
            Uno,
            Mega,
            Due
        }

    public class ARDUINO_BOARD
    {
        public static BoardType LastBoard = BoardType.Uno;

    

        private Dictionary<string, BoardType> boards = Extensions.GetDictionary<BoardType>();

        const string SerialPortToFind = "Arduino"; // Change this to your desired device name

        public static List<ARDUINO_BOARD> Bank = new List<ARDUINO_BOARD>();
        public static bool HiddenDisplay => Properties.Settings.Default.hidden_ports;

        public override string ToString()
      => $"{Port}: {Name}";

        public string ToDescString()
            => $"{TYPE} {Index}: {Name} | {Port} ";
        public readonly string Port;

        public  int Index;
        public BoardType TYPE;
   
 

     //   private readonly string Manufacturer;
        public readonly string Name ;
        public readonly string Fullname ;

        private ARDUINO_BOARD( ManagementBaseObject entity)
        {
            Name = entity["Description"].ToString();
             Port=entity["DeviceID"].ToString();
             Fullname=entity["Name"].ToString();
            
        }

        public bool Undefined => TYPE == BoardType.NAN;

        public static bool Update()
        {
           
            Bank.Clear();
           var AvailablePorts = SerialPort.GetPortNames().ToList();
            using (var entitySearcher = new ManagementObjectSearcher(
                       "SELECT * FROM WIN32_SerialPort"))
            {
                foreach (var entity in entitySearcher.Get())
                {
                    try
                    {
                        var t = new ARDUINO_BOARD(entity);
                        t.Index=AvailablePorts.IndexOf(t.Port);
                        t.Detect();
                            Bank.Add(t);
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                // Now you can use the deviceId to find the corresponding COM port using MSSerial_PortName
                // ...
            }
            return true;
        }

        private void Detect()
        {
            TYPE = Name.StartsWith("Arduino")?
                boards.FirstOrDefault(i => 
                    Name.Contains(i.Key)).Value:BoardType.NAN;
        }

    }
}