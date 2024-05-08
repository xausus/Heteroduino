#region Using region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Heteroduino.Properties;
// ReSharper disable All


#endregion

namespace Heteroduino
{
    public class StepperCommander : M_Components
    {
        public static readonly int[] Stri = {3, 5, 6, 8, 9, 10, 11, 12};

        string UnoPinNames(int i)
            => $"PIN: {Stri[i*2]:S 00->}{Stri[1+i*2]:00 D}";
        string MegaPinNames(int i)
             => $"PIN: S {38+i*2}->{39+i*2} D";


        
        public override bool AppendMenuItems(ToolStripDropDown menu)
        {

            Menu_AppendObjectName(menu);
            Menu_AppendEnableItem(menu);
            Menu_AppendSeparator(menu);
            var pin = GetValue("pin", -1);
            Menu_AppendItem(menu, "Release Motor", Pinevent, true, pin == -1);

            if(GetValue(MegaStr,false))
                for (int i = 0; i < 8; i++)
                    Menu_AppendItem(menu, MegaPinNames(i), Pinevent, true, pin == i );
            else
            for (var i = 0; i < 4; i++)
                Menu_AppendItem(menu,UnoPinNames(i), Pinevent, true, pin == i );

            Menu_AppendSeparator(menu);
    
            Menu_AppendItem(menu, "Mega Board", Megaswitch, true, GetValue(MegaStr, false));
            Menu_AppendSeparator(menu);
            Menu_AppendObjectHelp(menu);
            return true;
        }
      

  

        readonly List< string > accmodes=new List<string> {"Constant Speed" ,"Low Acceleration", "Lax Acceleration", "Normal Acceleration", "Strong Acceleration" ,"Idle Acceleration"};
        private readonly List<byte> Pinfinder =new List<byte>{3,6,9,11,38,40,42,44,46,48,50,52};

        private void Pinevent(object sender, EventArgs e)
        {
            removedpin= GetValue("pin", -1);
            RecordUndoEvent("pin#");
            
            var t =Convert.ToByte(sender.ToString().Substring(7, 2)) ;
            // SetValue("pin", Pinfinder.IndexOf(t, StringComparison.Ordinal) + 1);
            var pin = Pinfinder.IndexOf(t);
            if (pin >3) pin -= 4;
            SetValue("pin",pin);
            ExpireSolution(true);
        }

     

        public override void RemovedFromDocument(GH_Document document)
        {
    
            int pin = GetValue("pin", -1);
            if(pin==-1)return;
             var command = "%" + StepperState.Remove(pin).ToString("X8");
            Tools. ToArduino(command,doc);
 base.RemovedFromDocument(document);
        }


        /// <summary>
        ///     Initializes a new instance of the Stepper class.
        /// </summary>
        public StepperCommander()
            : base("Stepper Motor","SM.Heteroduino", 
                "Creates a controlling command for a Stepper Motor",1)
        {
          
        }
        
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.SM;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{4251c75a-ebb5-4f1d-baa9-abf1162d67e9}");


        /// <summary>
        ///     Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Target.Position", "T", "The target position of the motor to reach, which can be between -32767 to 32767 ", GH_ParamAccess.item,0);
            pManager.AddIntegerParameter("Speed", "S", "Optional speed using in Stepper mode 0~1024", GH_ParamAccess.item,700);
            
            var k= pManager.AddIntegerParameter("Acceleration", "A", "Acceleration mode:\n" +
                string.Join("\n",accmodes.Select((s,i)=>i.ToString("0: ")+s)), GH_ParamAccess.item, 3);
  Param_Integer J=pManager[k] as Param_Integer;
        
            for (int i = 0; i < accmodes.Count; i++)
            J.AddNamedValue(accmodes[i],i);
            pManager.AddBooleanParameter("Reset", "R", "Resetting current position as rest", GH_ParamAccess.item, false);
        }

        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Stepper Commands", "SM",
                "List of stepper motor commands by one motor", GH_ParamAccess.list);
        }

        

        private int removedpin = -1;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
     //  if(Neo) Doubleclick();

            if (removedpin >= 0)
            {
                
                DA.SetData(0,StepperState.Remove(removedpin));
                removedpin = -1;
                ExpireSolution(true);
                return;
            }
             
            var rs = false;
            var pin = GetValue("pin", -1);
            if (pin == -1) return;
            bool mega=GetValue(MegaStr,false);
            var acc = 0;
            DA.GetData("Acceleration", ref acc);
            if (acc > 5)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Undefined Acceleration Mode");
                return;
            }
            var pintex =mega?
 $"Stp:[{38+pin*2}]  Dir:[{39+pin*2}]\n{accmodes[acc]}"
               : $"Stp:[{Stri[pin*2]}]  Dir:[{Stri[pin*2+1 ]}]\n{accmodes[acc]}";
              

            Message =pintex;
           
                var pos = 0;
                DA.GetData(0, ref pos);
                var spd = -1;
                DA.GetData("Speed", ref spd);
            if (spd > 1023)
            {
                spd = 1023;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "The maximum possible value for Speed is 1023");
            }


            if (DA.GetData("Reset", ref rs) && rs) acc = 6;
          MOTOR=new StepperState(pin, pos, spd, acc);
            DA.SetData(0, MOTOR.Code);
            
        }
        StepperState MOTOR;

  }
}