using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Heteroduino.Properties;

namespace Heteroduino
{
    public class RX : H_Component
    {
        private List<int> analogs = new List<int>();
        private List<bool> digitals = new List<bool>();

        public RX()
            : base("Receiver (RX Interpreter)", "RX.Heteroduino",
                "Interprets data from the pins gotten from RX")
        {
        }


    

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.RX;


        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{3177fa34-ad23-45cf-9d2d-fc2deaf9dd18}");

        public override void Doubleclick() => Tools.GetSource(OnPingDocument(), Params.Input[0], 0);

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "HeteroFilter", Bipolariom, true, GetValue("filter", false)).
                ToolTipText = "Ignore messages from other firmata than Heteroduino";
        }

        public void Bipolariom(object sender, EventArgs e)
        {
            RecordUndoEvent("Options");
            SetValue("filter", !GetValue("filter", false));
            ExpireSolution(true);
        }


        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_IntegerParam("Analog", "A", "All analog data receiving from Arduino", GH_ParamAccess.list);
            pManager.Register_BooleanParam("Digital", "D", "Digital data receiving from Input pins" +
                                                           "\n2,4,7 in Arduino-Uno or \n22,23,24,25,26,27,28,29,30,31 in Arduino-Board_Type ",
                GH_ParamAccess.list);
            pManager.Register_IntegerParam("Sonar", "S",
                "Distance by ultrasonic sensors connected to digital input pins", GH_ParamAccess.list);
        }

        public override void AddedToDocument(GH_Document document) => Tools.AddCoreRX(this);

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var commands = new List<string>();
            if (!DA.GetDataList(0, commands) || (commands.Count <= 0)) return;
            string c;
            var sonaris = new List<int>();
            if ((commands.Count > 0) && (c = commands[0]).StartsWith("#"))
            {
                var s = c.Split('#');
                if (s.Length != 4) return;
                analogs = s[1].Split('|').Select(i => Convert.ToInt32(i, 16)).ToList();
                digitals = s[2].Select(i => i == '1').ToList();
                if (s[3].StartsWith("@"))
                    sonaris = s[3].Substring(1).Split('@').Select(i => Convert.ToInt32(i)).ToList();
                else sonaris.Clear();
            }

            DA.SetDataList(0, analogs);
            DA.SetDataList(1, digitals);
            DA.SetDataList(2, sonaris);
        }

  

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("RX", "", "Recievd-Data from arduino", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }
    }
}