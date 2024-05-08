#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Heteroduino.Properties;
using static Grasshopper.Utility;

#endregion

namespace Heteroduino
{
    public class TX : HetroBase_Component
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
        private SerialPort serial=>CoreBase.serial;
  

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
            ForceSerialsend(mm);
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
                
               if(_core==value) return;
               if(_core!=null)
                   _core.SolutionExpired -= this.OnCoreExpire;
               _core = value;
               if(value!=null)
                   _core.SolutionExpired += this.OnCoreExpire;

            }
        }


        

        public override void AddedToDocument(GH_Document document)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("PIN Commands", "CC", "Predefined commands by Heteroduino firmata",
                GH_ParamAccess.list);

            pManager.AddIntegerParameter("Stepper Motor Commands", "SM", "Commands to control stepper motor",
                GH_ParamAccess.list);

            var x = pManager.AddIntegerParameter("Sonar Number", "SN",
                "The Number of Sonar-Sensors\nRight-Click and choose the number of sonars\n The maximum allowed number of sonar sensors are 3 for Uno and 8 for Mega boards." + Resources.ParamOption,
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

        
        public bool ForceSerialsend(string msg)
        {
            comouot.Add(msg);
            try
            {
                if (!serial.IsOpen) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The port is closed");
                serial.WriteLine(msg);
            }
            catch (Exception)
            { return Pairing(); }
            return true;
        }

        protected override void BeforeSolveInstance()
        {

            comouot = new List<string>();
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Message = Megaset ? "Mega Mode" : "";
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
                            $"There is more than one vale for PIN-> {PINState.UnoPins[pin]}");
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
                    ForceSerialsend(ccbek);
            }

            //===========================================================Motor===============================
            var steppers = new List<int>();
            if (DA.GetDataList(1, steppers)) 
                StepperStack.UnionWith(steppers.Select(i=>new StepperState(i)));


            //==============================================Sonar===========================================
            var sn = 0;
            var maxsonar = Megaset ? 8 : 3;
            if (DA.GetData(2, ref sn))
            {
                
                if (sn > maxsonar)

                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"You can not use more than {maxsonar} Ultrasonic-Sensors or ");
                else
          
                 if   (sn != NumberOfSonars && serial.IsOpen)
                {
                    UpdateSonars(sn);
                    NumberOfSonars = sn;
                }
            

                
            }  
              

            //===================================================FreeSyntax=================================
            var DC = "";
            if (DA.GetData(3, ref DC) && DC != "")
                ForceSerialsend(DC);

            DA.SetDataList(0, comouot);
        }

        public int NumberOfSonars = 0;
        public HashSet<StepperState> StepperStack = new HashSet<StepperState>();
        void UpdateSonars(int n)=> ForceSerialsend(n.ToString("@0"));

        /// <summary>
        /// ---------------------------------------------------------pairing---------------
        /// </summary>
        /// <returns></returns>
        private bool Pairing()
        {

            var level = Attributes.Pivot.Y;
            var os = OnPingDocument().Objects.Where(i => i is Core)
                .Cast<Core>().ToList();
            if (os.Count == 0)

            {
                CoreBase = null;
                return false;
            }
            var levelDif = os.Select(i => Math.Abs(i.Attributes.Pivot.Y - level)).ToList();
            var index = levelDif.IndexOf(levelDif.Min());

            CoreBase = os[index];
            if (GetValue("kick", 0) > 0) CoreBase?.ExpireSolution(true);
            return true;
        }




        public override void CreateAttributes() => m_attributes = new Att_TX_Comp(this);

        public void RefreshSrources()
        {
            var doc = OnPingDocument();
            Pairing();
            Tools.GetSource<CC>(doc, Params.Input[0], 0);
            Tools.GetSource<StepperCommander>(doc, Params.Input[1], 0);
        }
    }
}