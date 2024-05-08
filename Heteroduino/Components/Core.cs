#region

using Grasshopper.Kernel;
using Heteroduino.Properties;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows.Forms;

#endregion

// ReSharper disable All

namespace Heteroduino
{




    public class Core : HetroBase_Component
    {
        readonly Interval normalrate = new Interval(50, 100);
        private readonly List<int> speedbank = new List<int>() { 1000, 500, 250, 100, 50, 35, 20 };
        private List<int> ardiporti;
        Att_Core att;

        public override GH_Exposure Exposure => GH_Exposure.primary;
        private List<string> avaporti4DisplayList;

        public PointF baraks;
        char[] cats = new char[] { 'R', 'M', 'S', 'E' };

        public string comindex;

        int defport = -1;
        public bool enable = false;
        public int Index;
        string[] lastcommand = new string[] { "", "", "", "", "" };

        List<string> outrx = new List<string>();


        public string PortName
        {
            get { return _portName; }
            set
            {
                _portName = value;
                SetValue("Port", _portName);
            }
        }
        private string _portName = "";
        public SerialPort serial;
        int[] spds = { -1, 500, 100, 35 };
        private string stack = "";

        public List<int> SteppersMessage = new List<int>();
        List<string> TXout = new List<string>();

        public Core()
            : base("Heteroduino Core", "Core.Heteroduino",
                "Heteroduino Core is the base component in Heteroduino, interconnecting to the arduino board which is connected to the computer " +
                "and the other components. It's also equipped with RX output,the row data receiving from arduino." +
                " \n \n-Zoom and click to refresh RX while the engine is off\n-Double-Click to reset the port settings")
        {
            att = this.m_attributes as Att_Core;
        }

        protected override Bitmap Icon => Resources.arduilogo;
        public override Guid ComponentGuid => new Guid("{2edff0e3-63d0-495f-82ff-83edb73656f8}");

        private List<string> Pure_Rx => Purify(Rxs);

        public IGH_Component IGHTX { get; set; }

        private List<string> Rxs
        {
            get
            {
                stack += serial.ReadExisting();
                if (stack.Length > 0)
                {
                    var sep = stack.Split('\n');

                    if (!stack.ToCharArray().Last().Equals("\n"))
                    {
                        if (sep.Length > 1)
                        {
                            stack = sep.Last();
                            return sep.Take(sep.Length - 1).ToList();
                        }
                    }
                    else
                    {
                        stack = "";
                        return sep.ToList();
                    }
                }
                return new List<string>();
            }
        }

        public bool Megaset { get; private set; }

        public int TimingMode
        {
            get { return _timingMode; }
            set
            {
                _timingMode = value;
                RecordUndoEvent("Timing Mode");
                SetValue("TimingMode", _timingMode);
                ExpireSolution(true);
            }
        }

      

        public void Resetports()
        {
            close();
            Serial();
        }

        private List<string> Purify(List<string> c)
        {
            var s = c.ToList();
            if (s.Count > 0)
            {
                var frontrx = new List<string>();
                try
                {
                    var kn = s.Last(i => i.StartsWith("#"));
                    frontrx.Add(kn);
                    s.RemoveAll(i => i.StartsWith("#"));
                    kn = s.Last(i => i.StartsWith("%"));
                    frontrx.Add(kn);
                    s.RemoveAll(i => i.StartsWith("%"));
                }
                catch (Exception)
                {
                }
                frontrx.AddRange(s);
                outrx = frontrx;
            }
            return outrx;
        }

        private int _timingMode = -1;
        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendObjectName(menu);
            Menu_AppendEnableItem(menu);
            Menu_AppendSeparator(menu);
        
            Menu_AppendItem(menu, "Mega",
                Megaseter, true, GetValue("mega", false));
            Menu_AppendSeparator(menu);
        
        
            Menu_AppendItem(menu, "Purify Rx",
                Pureseter, true, GetValue("pure", true)).ToolTipText = "Filtering excess data";
            Menu_AppendSeparator(menu);
        
        
            Menu_AppendItem(menu, "No-Port", portselect, true, PortName.StartsWith("No"));


            try
            {
    foreach (var s in avaporti4DisplayList)
                Menu_AppendItem(menu, s, portselect, true, PortName.StartsWith(s));
            }
            catch (Exception)
            {
                Menu_AppendItem(menu, " -- No Appropriate Port --");
            }
        



            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Self-Engine off", spdclick, true,
                TimingMode == -1);
        
            var rundrop = Menu_AppendItem(menu, "Running", null, true,
            TimingMode != -1).DropDown;
            for (var index = 0; index < speedbank.Count; index++)
            {
                var i = speedbank[index];
                var prefix = normalrate.IncludesParameter(i)
                    ? "Normal"
                    : i < normalrate.T0 ? "Fast" : "Slow";
                Menu_AppendItem(rundrop, $"{prefix} ({i}ms)", spdclick, true,
                TimingMode == index);
            }
        
        
            var bauddrop = Menu_AppendItem(menu, "Baudrate").DropDown;
            Menu_AppendItem(bauddrop, "Baudrate: 9600", boud_Clicked, true,
                GetValue("baudrate", 9600) == 9600);
            Menu_AppendItem(bauddrop, "Baudrate: 11520", boud_Clicked, true,
                GetValue("baudrate", 9600) == 11520);
            Menu_AppendSeparator(menu);
            Menu_AppendObjectHelp(menu);
            return true;
        }

        private void Pureseter(object sender, EventArgs e)
        {
            RecordUndoEvent("Rx_Purity");
            SetValue("pure", !GetValue("pure", true));
        }

        private void Megaseter(object sender, EventArgs e)
        {
            RecordUndoEvent("megamod");
            SetValue("mega", !GetValue("mega", false));
            ExpireSolution(true);
        }

        private void boud_Clicked(object sender, EventArgs e)
        {
            var val = int.Parse(sender.ToString().Substring(10));
            RecordUndoEvent("baud");
            SetValue("baudrate", val);
            Serial();
            ExpireSolution(true);
        }

        bool Serial()
        {
            comindex = string.Concat(PortName.TakeWhile(i => i != ':'));
            bool r;
            try
            {
                serial?.Dispose();
                serial = new SerialPort(comindex, GetValue("baudrate", 9600));
                serial.Open();
                r = true;
            }
            catch (Exception)
            {
                serial = new SerialPort();
                PortName = "No Port!";
                r = false;
            }
            return r;
        }

        void close()
        {
            if (serial != null && serial.IsOpen) serial.Close();

        }

        private void portselect(object sender, EventArgs e)
        {
            RecordUndoEvent("Port select");
            var p = sender.ToString();
            close();
            PortName = p;
            if (!p.StartsWith("No"))
                Serial();
            ExpireSolution(true);
        }

        private void spdclick(object sender, EventArgs e)
        {

            var tc = sender.ToString();
            if (tc == "Self-Engine off")
            {
                TimingMode = -1;
                return;
            }
            var vs = new string(tc.SkipWhile(i => i != '(').Skip(1).TakeWhile(i => i != 'm').ToArray());
            var v = int.Parse(vs);
            TimingMode = speedbank.IndexOf(v);

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Serial-In ", "RX", "Serial output from Arduino board", GH_ParamAccess.list);
        }


        public override void AddedToDocument(GH_Document document)
        {

            try
            {
                AutodetectArduinoPort();
                _timingMode = GetValue("TimingMode", -1);
                _portName = GetValue("Port", "No-Port");
                Serial();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// -----------------------------------------------------------------------------------Solve Instance-------------
        /// </summary>
        /// <param name="DA"></param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            // Megaset = GetValue("mega", false);
            // enable = PortName.Contains("Arduino");
            // att.Rx_led = enable && !att.Rx_led;
            // var temp = new List<string>();
            // temp.AddRange(SteppersMessage.Select(i => i.ToString()));
            // if (serial != null && serial.IsOpen)
            //     DA.SetDataList(0, GetValue("pure", true) ? Pure_Rx : Rxs);
            //
            //
            // if (TimingMode < 0) return;

          //  OnPingDocument()?.ScheduleSolution(speedbank[TimingMode], ScheduleCallback);
        }


        private void ScheduleCallback(GH_Document doc) => this.ExpireSolution(false);


        public override void RemovedFromDocument(GH_Document document) => close();
        public override void DocumentContextChanged(GH_Document document, GH_DocumentContext context) => close();
          public override void CreateAttributes() => m_attributes = new Att_Core(this);

        private bool AutodetectArduinoPort()
        {


            //
            // using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
            // {
            //     var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
            //     var portList = portNames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();
            //
            //     foreach (string s in portList)
            //     {
            //         Console.WriteLine(s);
            //     }
            // }
            //

            string SerialPortToFind = "Arduino"; // Change this to your desired device name
            ardiporti = new List<int>();
            avaporti4DisplayList = new List<string>();
            var AvailablePorts =
                SerialPort.GetPortNames().ToList();
            
            using (var entitySearcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%{SerialPortToFind}%'"))
            {
               
                foreach (var entity in entitySearcher.Get())
                {
                    try
                    {
                        // Process the found devices (e.g., check if it's an Arduino)
                        string deviceName = entity["Name"].ToString();
                        string deviceId = entity["DeviceID"].ToString();
                        string desc = entity["Description"].ToString();
                        if (desc.Contains("Arduino"))
                        {
                            var portindex = AvailablePorts.IndexOf(deviceId);
                            avaporti4DisplayList[portindex] = $"{deviceId}: {desc}";
                            ardiporti.Add(portindex);
                            if (desc.Contains("Mega"))
                            {
                                RecordUndoEvent("mega-mod");
                                SetValue("mega", true);
                            }
                        }

                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    // Now you can use the deviceId to find the corresponding COM port using MSSerial_PortName
                    // ...
                }
            }

            if (ardiporti.Count == 0) return false;
            defport = ardiporti[0];
            RecordUndoEvent("Port");
            PortName = avaporti4DisplayList[defport];
            return true;
        }
    }
}