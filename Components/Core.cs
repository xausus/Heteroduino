#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Heteroduino.Properties;
using Rhino.Geometry;

#endregion

// ReSharper disable All

namespace Heteroduino
{
    public class Core : HetroBase_Component
    {
        private static readonly BoardType[] Megalike = new[] { BoardType.Mega, BoardType.Due };

        readonly Interval normalrate = new Interval(50, 100);
        private readonly List<int> speedbank = new List<int>() { 1000, 500, 250, 100, 50, 35, 20 };
        public string __version = "-";
        private ARDUINO_BOARD _activeBoard;




        private bool? _puremode;

        private int _timingMode = -1;


        public PointF baraks;
        char[] cats = new char[] { 'R', 'M', 'S', 'E' };


        public int Index;
        string[] lastcommand = new string[] { "", "", "", "", "" };

        List<string> outrx = new List<string>();

        public bool Rx_led;
        public SerialPort serial;
        int[] spds = { -1, 500, 100, 35 };
        private string stack = "";

        public List<int> SteppersMessage = new List<int>();
        List<string> TXout = new List<string>();
        private BoardType _arduinoBoardType;

        public Core()
            : base("Heteroduino Core", "Core.Heteroduino",
                "Heteroduino Core is the base component in Heteroduino, interconnecting to the arduino board which is connected to the computer " +
                "and the other components. It's also equipped with RX output,the row data receiving from arduino." +
                " \n \n-Zoom and click to refresh RX while the engine is off\n-Double-Click to reset the port settings")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

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


    

        public bool PureMode
        {
            get => _puremode ??= this.GetValue("pure", true);
            set
            {
                _puremode = value;
                SetValue("pure", value);
            }
        }

        public ARDUINO_BOARD ActiveBoard
        {
            get => _activeBoard;
            set
            {
                if (value == _activeBoard) return;
                _activeBoard = value;
                CloseSerial();
          
                  
                if (value == null)
                {
                    _activeBoard = null;
                    serial = null;
                }
                else
                {
                    _activeBoard = value;
                    ArduinoBoardType = value.TYPE;
                    OpenSerial();
                    ExpireSolution(true);

                }
            }
        }

        public bool MegaMode { get;private set; }

        public TX Rx { get; set; }

        bool OpenSerial()
        {
            if (ActiveBoard == null) return false;

            if (serial != null && serial.PortName != ActiveBoard.Port)
            {
                serial.Dispose();
                serial = null;
            }

            try
            {
                if (serial == null)
                    serial = new SerialPort(ActiveBoard.Port, GetValue("baudrate", 9600));
                serial.Open();
                SerialChangeState.Invoke(serial, true);
                return true;
            }
            catch (Exception e)
            {
                ActiveBoard = null;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                return false;
            }
        }

        public event Action<SerialPort, bool> SerialChangeState;

        void CloseSerial()
        {
            if (serial != null && serial.IsOpen)
            {
                serial.Close();
                SerialChangeState.Invoke(serial,false);
            }
        }


        public void Resetports()
        {
            CloseSerial();
            OpenSerial();
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

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendObjectName(menu);
            Menu_AppendEnableItem(menu);
            Menu_AppendSeparator(menu);


            // var m = Menu_AppendItem(menu, "Board Type >>").DropDown;
            // var cb = GetValue("board", ARDUINO_BOARD.BoardType.UNO.ToString());
            // foreach (var name in Extensions.GetNames<ARDUINO_BOARD.BoardType>())
            //     Menu_AppendItem(m, name,
            //         BoardSetter, true, cb == name);

           // Menu_AppendItem(menu, "Auto-Detect Board!!", (o, e) => Refresh(), true, false);
            Menu_AppendItem(menu, "Purify Rx", Pureseter, true, GetValue("pure", true)).ToolTipText = "Filtering excess data";

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Refresh Ports !", (o, i) => ARDUINO_BOARD.Update(), true, false);

            if (ActiveBoard == null || ActiveBoard.Undefined)
            {

                var pinset = Menu_AppendItem(menu, "Arduino Board >>").DropDown;
                foreach (var s in Extensions.GetEnumArray<BoardType>().Skip(1))
                    Menu_AppendItem(pinset, s.ToString(), changeBoardType_callback, true, s == ArduinoBoardType);
               
            }

 Menu_AppendSeparator(menu);
                Menu_AppendItem(menu, "No-Port", ChooseBoard_callback, true, ActiveBoard == null);
            foreach (var s in ARDUINO_BOARD.Bank)
                try
                {
                    Menu_AppendItem(menu, s.Fullname, ChooseBoard_callback,
                        true, ActiveBoard?.Port == s.Port).Tag = s;
                }
                catch (Exception)
                {
                    Menu_AppendItem(menu, " -- No Appropriate Port --");
                }

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Self-Engine off", spdclick_callback, true,
                TimingMode == -1);

            var rundrop = Menu_AppendItem(menu, "Running", null, true,
                TimingMode != -1).DropDown;
            for (var index = 0; index < speedbank.Count; index++)
            {
                var i = speedbank[index];
                var prefix = normalrate.IncludesParameter(i)
                    ? "Normal"
                    : i < normalrate.T0
                        ? "Fast"
                        : "Slow";
                Menu_AppendItem(rundrop, $"{prefix} ({i}ms)", spdclick_callback, true,
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

        private void Refresh(int n = -1)
        {
            //    MessageBox.Show(ARDUINO_BOARD.AvailablePorts.Join());
            MessageBox.Show(ARDUINO_BOARD.Bank.Select(i => i.ToDescString()).Join());
            //    ActiveBoard =(n<0)? ARDUINO_BOARD.Bank.First(): ARDUINO_BOARD.Bank[n];
        }

        private void changeBoardType_callback(object sender, EventArgs e)
        {
             
            if (Enum.TryParse(sender.ToString(), out BoardType newboard))
            {
                if (newboard == ArduinoBoardType) return;
                ArduinoBoardType= newboard;
                ExpireSolution(true);    
            }

        }


        public BoardType ArduinoBoardType
        {
            get => _arduinoBoardType;
            set
            {
                if(_arduinoBoardType == value) return;
                _arduinoBoardType = value;
                MegaMode = value == BoardType.Mega || value == BoardType.Due;
                BoardTypeChanged.Invoke(value);
                this.Attributes.ExpireLayout();
            }
        }

          



        private void BoardSetter(object sender, EventArgs e)
        {
        }

        private void Pureseter(object sender, EventArgs e)
        {
            RecordUndoEvent("Rx_Purity");
            PureMode = !PureMode;
        }

   

        public event Action Jump;
        public void TxBlink() => Jump?.Invoke();


        private void boud_Clicked(object sender, EventArgs e)
        {
            var val = int.Parse(sender.ToString().Substring(10));
            RecordUndoEvent("baud");
            SetValue("baudrate", val);
            OpenSerial();
            ExpireSolution(true);
        }


        private void ChooseBoard_callback(object sender, EventArgs e) => 
            ActiveBoard = (sender as ToolStripMenuItem).Tag as ARDUINO_BOARD;


        private void spdclick_callback(object sender, EventArgs e)
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
            __version = new HeteroduinoInfo().Version;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Serial-In ", "RX", "Serial output from Arduino board", GH_ParamAccess.list);
        }


        public override void AddedToDocument(GH_Document document)
        {
            try
            {
                ARDUINO_BOARD.Update();
                _timingMode = GetValue("TimingMode", -1);

                OpenSerial();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     -----------------------------------------------------------------------------------Solve Instance-------------
        /// </summary>
        /// <param name="DA"></param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rx_led = !Rx_led;
            var temp = new List<string>();
            temp.AddRange(SteppersMessage.Select(i => i.ToString()));
            if (serial != null && serial.IsOpen)
                DA.SetDataList(0, PureMode ? Pure_Rx : Rxs);


            if (TimingMode < 0) return;
            OnPingDocument()?.ScheduleSolution(speedbank[TimingMode],
                doc => this.ExpireSolution(false));
        }


        public override void RemovedFromDocument(GH_Document document) => CloseSerial();
        public override void DocumentContextChanged(GH_Document document, GH_DocumentContext context) => CloseSerial();
        public override void CreateAttributes() => m_attributes = new Att_Core(this);
        public event Action<BoardType> BoardTypeChanged;

        public class TT : EventArgs
        {
            public bool State;

            public TT(bool state)
            {
                State = state;
            }
        }
    }
}