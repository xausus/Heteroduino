#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Heteroduino.Properties;
using static Grasshopper.Utility;

#endregion

namespace Heteroduino
{
    public class TX : GH_Component
    {
        private readonly Dictionary<int, int> ColDic = new Dictionary<int, int>();
 private readonly int[] DigiUno = {2, 4, 7};

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

        public IGH_DocumentObject _core;
        private AttArduinoCore attcore;
        private List<string> comouot = new List<string>();
        private string lastSTP = "";
        private SerialPort serial;
        private int sonarcount;
        private Dictionary<int, int> StepDic;


        /// <summary>
        ///     Each implementation of GH_Component must provide a public
        ///     constructor without any arguments.
        ///     Category represents the Tab in which the component will appear,
        ///     Subcategory the panel. If you use non-existing tab or panel names,
        ///     new tabs/panels will automatically be created.
        /// </summary>
        public TX()
            : base("TX Core", "TX.Heteroduino",
                "Combines and sends all commands directly into the Arduino port \nDouble-click to connect all sources and adjust board type settings",
                "Heteroptera", "Arduino")
        {
        }

 //       public IGH_DocumentObject CorePair { get; private set; }


        protected override Bitmap Icon => Resources.TX;
        public override Guid ComponentGuid => new Guid("{757a5edf-c9a5-405f-91bf-178c15daa58e}");

 

        /// <summary>
        /// ---------------------------------
        /// </summary>
        public Core CoreBase;
      
        public IGH_DocumentObject IGHCore
        {
            get { return _core; }
            set
            {
                if (value == null)
                {
                    InvokeSetter(IGHCore, "PairTxIGH", null);
                    InvokeSetter(IGHCore, "PairTag", "");
                    
                    IGHCore = null;
                    attcore = null;
                    serial = null;
                    return;
                }
                _core = value;
                 CoreBase = _core as Core;
              if ( CoreBase.IGHTX==this) return;
                IGH_Component Null = null;
               // InvokeSetter(CoreBase.IGHTX, "IGHCore", Null);
              //   InvokeSetterSafe(IGHCore, "TxPaired", this);
                var guid = $"{this.InstanceGuid.ToString().Substring(0, 5)}..";
                Message = guid;

                InvokeSetterSafe(IGHCore, "PairTag",$"→ {guid}");
                attcore = IGHCore.Attributes as AttArduinoCore;
                serial = CoreBase.serial;
                IGHCore.ExpireSolution(true);
            // CoreBase.IGHTX.ExpireSolution(true);
            }
        }
        


        public override void AddedToDocument(GH_Document document)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Pin Commands", "CC", "Predefined commands by Heteroduino firmata",
                GH_ParamAccess.list);

            pManager.AddIntegerParameter("Stepper Motor Commands", "SM", "Commands to control stepper motor",
                GH_ParamAccess.list);

            var x = pManager.AddIntegerParameter("Sonar Number", "SN",
                "The Number of Sonar-Sensors" + Resources.ParamOption,
                GH_ParamAccess.item, 0);

            pManager.AddTextParameter("Direct Commands", "DC", "Commands to sent to Arduino's serial port directly",
                GH_ParamAccess.item);
            for (var i = 0; i < 4; i++)
                pManager[i].Optional = true;


            var sonarset = pManager[x] as Param_Integer;
            sonarset.AddNamedValue("No Ultrasonic Sensor", 0);

           
            var maxsonar = Megaset ? 8 : 3;

            for (var i = 0; i < maxsonar; i++)
                sonarset.AddNamedValue($"{sonartags[i]}  Sonar [+Pin: {(Megaset ? i + 22 : DigiUno[i])}]", i + 1);
        }

        public bool Megaset => CoreBase?.Megaset==true;


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

        public bool Serialsend(string msg)
        {
            comouot.Add(msg);
            try
            {
              if (!serial.IsOpen) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The port is closed");
              CoreBase.Sentcommand = msg;
              serial.WriteLine(msg);
            }
            catch (Exception)
            { return  Pairing(); }
            return true;
        }

        protected override void BeforeSolveInstance()
        {
            StepDic = new Dictionary<int, int>();
     
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
                            $"There is more than one vale for Pin-> {CC.UnoPins[pin]}");
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
                    Serialsend(ccbek);
            }

            //===========================================================Motor===============================
            var steppers = new List<int>();
            StepDic.Clear();
            if (DA.GetDataList(1, steppers))
            {
                foreach (var i in steppers)
                {
                    var m = new StepperState(i);

                    var pin = m.Pin;
                    if (!StepDic.ContainsKey(pin)) StepDic.Add(pin, i);
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                            $"Stepper #some motors may have more than one data");
                }

                var mm = '%' + string.Concat(StepDic.Values.Select(i => i.ToString("X8")));
                if (lastSTP == mm || mm == "%") return;
                lastSTP = mm;
                Serialsend(mm);
            }


            //==============================================Sonar===========================================
            var sn = 0;
            var maxsonar = Megaset ? 8 : 3;
            if (DA.GetData(2, ref sn) && sn != sonarcount && serial?.IsOpen == true)
                if (sn <= maxsonar && sn != sonarcount)
                    Serialsend((sonarcount = sn).ToString("@0"));
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"You can not use more than {maxsonar} Ultrasonic-Sensors");

            //===================================================FreeSyntax=================================
            var DC = "";
            if (DA.GetData(3, ref DC) && DC != "")
                Serialsend(DC);

            DA.SetDataList(0, comouot);
        }

        /// <summary>
        /// ---------------------------------------------------------pairing---------------
        /// </summary>
        /// <returns></returns>
        private bool Pairing()
        {
           
            var level = Attributes.Pivot.Y;
            var os = OnPingDocument().Objects.Where(i => i is Core).ToList();
            if (os.Count == 0)

            {
                IGHCore = null;
                return false;
            }
            var levelDif = os.Select(i => Math.Abs(i.Attributes.Pivot.Y - level)).ToList();
            var index = levelDif.IndexOf(levelDif.Min());
    
            IGHCore = os[index];
 
            if (attcore != null) attcore.TX_State = true;
            if (GetValue("kick", 0) > 0) CoreBase?.ExpireSolution(true);
  return true;
        }

       


        public override void CreateAttributes() => m_attributes = new Attri_Tx(this);

        public void RefreshSrources()
        {
            var doc = OnPingDocument();
            Pairing();
            Tools.GetSource<CC>(doc, Params.Input[0], 0);
            Tools.GetSource<StepperCommander>(doc, Params.Input[1], 0);
        }
    }
}