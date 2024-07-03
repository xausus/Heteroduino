#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Heteroduino.Properties;
using static Grasshopper.Utility;

#endregion

namespace Heteroduino
{


    class PMO<T> : Param_Integer where T : GH_Component, IArduinoController, new()
    {


        void editSource(IGH_Param s, bool add)
        {
            try
            {
                var q = s.Attributes.Parent.DocObject as IArduinoController;
                var owner = this.Attributes.Parent.DocObject as IArduinoBoardAware;
                if (add) owner.AddChild(q);
                else owner.RemoveChild(q);

            }
            catch (Exception )
            {

            }

        }

        public override void AddSource(IGH_Param source, int index)
        {


            if (source.Attributes.Parent.DocObject is T m)
            {
                base.AddSource(source, index);
                editSource(source, true);
            }
            else
                MessageBox.Show($"Please connect a [{new T().Name}] component to index {index}");


        }

        public override void AddSource(IGH_Param source)
        {
            if (source.Attributes.Parent.DocObject is T m)
            {
                base.AddSource(source);
                editSource(source, true);
            }
            else

                MessageBox.Show($"Please connect a [{new T().Name}] component");
        }


        public override void RemoveSource(IGH_Param source)
        {
            base.RemoveSource(source);
            editSource(source, false);
        }

    }


    public interface IArduinoController
    {
        void OnChangeBoard(BoardType board);

    }


    public interface IArduinoBoardAware
    {
        void EnsureChildren(List<IArduinoController> newlist);
        void AddChild(IArduinoController arduinoController);
        void RemoveChild(IArduinoController arduinoController);
    }

    public class TX : HetroBase_Component, IArduinoBoardAware
    {
        private readonly Dictionary<int, int> ColDic = new Dictionary<int, int>();
        private readonly int[] DigiUno = { 2, 4, 7 };


        public override GH_Exposure Exposure => GH_Exposure.primary;
        private readonly List<string> sonartags = new List<string>
        {
            "single",
            "double",
            "triple",
            "quadruple",
            "quintuple",
            "sextuple",
            "septuple",
            "octuple"
        };

        private Core _core;
        private List<string> comouot = new List<string>();
        private SerialPort serial => CoreBase.serial;


        /// <summary>
        ///     Each implementation of GH_Component must provide a public
        ///     constructor without any arguments.
        ///     Category represents the Tab in which the component will appear,
        ///     Subcategory the panel. If you use non-existing tab or panel names,
        ///     new tabs/panels will automatically be created.
        /// </summary>
        public TX()
            : base("TX Core", "TX.Heteroduino",
                "Combines and sends all commands directly into the Arduino port \nDouble-click to connect all sources and adjust board type settings"
               )
        {
        }

        //       public IGH_DocumentObject CorePair { get; private set; }

        protected override Bitmap Icon => Resources.TX;

        void OnCoreExpire(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs ghSolutionExpiredEventArgs)
        {
            var mm = '%' + string.Concat(StepperStack.Select(i => i.Code.ToString("X8")));
            StepperStack.Clear();
            SerialSend(mm);

        }
        public override Guid ComponentGuid => new Guid("{757a5edf-c9a5-405f-91bf-178c15daa58e}");

        /// <summary>
        /// ---------------------------------
        /// </summary>

        public Core CoreBase
        {
            get => _core;
            set
            {

            //    MessageBox.Show(_core?.NickName??"NAN");
                if (_core == value) return;
                if (_core != null)
                {
                    _core.SolutionExpired -= OnCoreExpire;
                    _core.BoardTypeChanged -= OnBoardChanged;
                }

                _core = value;
                if (value == null) return;
                if (value.Rx != null) value.Rx.CoreBase = null;
                value.Rx = this;
                value.SolutionExpired += OnCoreExpire;
                value.BoardTypeChanged += OnBoardChanged;

            }
        }








        public void OnBoardChanged(BoardType board)
        {
            
            Children.ForEach(i => i.OnChangeBoard(board));
            ExpireSolution(true);

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PMO<CC>(), "PIN Commands", "CC", "Predefined commands by Heteroduino firmata",
                GH_ParamAccess.list);

            var q = new PMO<StepperCommander>();

            pManager.AddParameter(q, "Stepper Motor Commands", "SM", "Commands to control stepper motor",
                   GH_ParamAccess.list);


            var x = pManager.AddIntegerParameter("Sonar Number", "SN",
                "The Number of Sonar-Sensors\nRight-Click and choose the number of sonars" +
                "\n The maximum allowed number of sonar sensors are 3 for Uno and 8 for Board_Type boards." + Properties.Resources.PinParam,
                GH_ParamAccess.item, 0);

            pManager.AddTextParameter("Direct Commands", "DC", "Commands to sent to Arduino's serial port directly",
                GH_ParamAccess.item);
            for (var i = 0; i < 4; i++)
                pManager[i].Optional = true;


            var sonarset = pManager[x] as Param_Integer;
            sonarset.AddNamedValue("No Ultrasonic Sensor", 0);

            var maxsonar = Megaset ? 8 : 3;
            for (var i = 0; i < maxsonar; i++)
                sonarset.AddNamedValue($"{sonartags[i]}  Sonar [+PIN: {(Megaset ? i + 22 : DigiUno[i])}]", i + 1);
        }

        public bool Megaset => CoreBase?.MegaMode == true;


        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendObjectNameEx(menu);
            Menu_AppendEnableItem(menu);
            Menu_AppendSeparator(menu);
            var b = Menu_AppendItem(menu, "Kicking Mode", Clickhandler, true, GetValue("kick", 0) > 0);
            b.ToolTipText = "Kicking state can kick (putting an expired flag on)" +
                            " the Core component after sending it the data immediately but may interrupt timing, otherwise its sync with core";
            var kickdon = b.DropDown;
            Menu_AppendItem(kickdon, "Kicking Off", Clickhandler, true, GetValue("kick", 0) == 0);
            Menu_AppendItem(kickdon, "Kicks by only new data", Clickhandler, true, GetValue("kick", 0) == 1);
            Menu_AppendItem(kickdon, "Kicking any way", Clickhandler, true, GetValue("kick", 0) == 2);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Connect all Sources", Clickhandler, true, false);
            Menu_AppendSeparator(menu);
            Menu_AppendObjectHelp(menu);
            return true;
        }

        private void Clickhandler(object sender, EventArgs e)
        {
            var t = sender.ToString();
            RecordUndoEvent("menuchack");
            switch (t)
            {
                case "Connect all Sources":
                    RefreshSrources();
                    break;
                case "Kicking Off":
                    SetValue("kick", 0);
                    break;
                case "Kicks by only new data":
                    SetValue("kick", 1);
                    break;
                case "Kicking any way":
                    SetValue("kick", 2);
                    break;
            }

            ExpireSolution(true);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Serial-Out ", "Out", "Messages sending to Arduino board", GH_ParamAccess.list);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (_core != null)
                _core.SolutionExpired -= OnCoreExpire;
            base.RemovedFromDocument(document);
        }


        public void SerialInstanceSend(string msg)
        {
            SerialSend(msg);
            CoreBase.TxBlink();
        }

        public bool SerialSend(string msg)
        {
            comouot.Add(msg);
            try
            {
                if (!serial.IsOpen) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The port is closed");
                serial.WriteLine(msg);
            }
            catch (Exception)
            {
                return Pairing(null);
            }
            return true;
        }

        protected override void BeforeSolveInstance()
        {

            comouot = new List<string>();
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Message =CoreBase?.Arduino_Type.ToString()??"-";
            var cc_in = new List<int>();

            ///     if(IGHCore!=null)   InvokeSetterSafe(IGHCore, "PairTag", this.NickName);
            //==============================================NORMAL==========================
            if (DA.GetDataList(0, cc_in))
            {
                var ccbek = "#";
                var pinholders = new List<int>();
                foreach (var s in cc_in)
                {
                    var pin = s >> 10;
                    if (pinholders.Contains(pin))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            $"There is more than one vale for PIN-> {TargetState.UnoPins[pin]}");
                        continue;
                    }
                    pinholders.Add(pin);
                    if (!ColDic.ContainsKey(pin)) ColDic.Add(pin, s);
                    else if (ColDic[pin] == s) continue;
                    ColDic[pin] = s;
                    ccbek += s.ToString("X4");
                }
                ccbek += '*';
                if (ccbek.Length > 2)
                    SerialInstanceSend(ccbek);
            }

            //===========================================================Motor===============================
            var steppers = new List<int>();
            if (DA.GetDataList(1, steppers))
                StepperStack.UnionWith(steppers.Select(i => new StepperState(i)));


            //==============================================Sonar===========================================
            var sn = 0;
            var maxsonar = Megaset ? 8 : 3;
            if (DA.GetData(2, ref sn))
            {

                if (sn > maxsonar)

                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"You can not use more than {maxsonar} Ultrasonic-Sensors or ");
                else

                 if (sn != NumberOfSonars && serial.IsOpen)
                {
                    UpdateSonars(sn);
                    NumberOfSonars = sn;
                }



            }


            //===================================================FreeSyntax=================================
            var DC = "";
            if (DA.GetData(3, ref DC) && DC != "")
                SerialSend(DC);

            DA.SetDataList(0, comouot);
        }

        public int NumberOfSonars = 0;
        public HashSet<StepperState> StepperStack = new HashSet<StepperState>();
        void UpdateSonars(int n) => SerialSend(n.ToString("@0"));

        /// <summary>
        /// ---------------------------------------------------------pairing---------------
        /// </summary>
        /// <param name="ghDocument"></param>
        /// <returns></returns>
        private bool Pairing(GH_Document ghDocument)
        {

            ghDocument ??= OnPingDocument();
            var level = Attributes.Pivot.Y;
            var os = ghDocument.Objects.Where(i => i is Core)
                .Cast<Core>().ToList();

         //   MessageBox.Show("[]: "+os.ToStringChain(i=>i.NickName));

            switch (os.Count)
            {
                case 0:
                    CoreBase = null;
                    return false;
                case 1:
                    CoreBase = os[0];
                    
              //  ExpireSolution(true);
                    return true;
                default:
                    CoreBase = os.MinItem(i => Math.Abs(i.Attributes.Pivot.Y - level));
                    if (GetValue("kick", 0) > 0) CoreBase?.ExpireSolution(true);
                 //   ExpireSolution(true);
                    return true;

            }

        }

        public override void CreateAttributes() => m_attributes = new Att_TX_Comp(this);

        public void RefreshSrources()
        {
            var doc = OnPingDocument();
            Pairing(doc);
            Tools.GetSource<CC>(doc, Params.Input[0], 0);
            Tools.GetSource<StepperCommander>(doc, Params.Input[1], 0);
        }

        public void EnsureChildren(List<IArduinoController> newlist)
        {


        }


        List<IArduinoController> Children = new List<IArduinoController>();
        public void AddChild(IArduinoController a) => Children.Add(a);

        public void RemoveChild(IArduinoController a) => Children.Remove(a);

    }

}