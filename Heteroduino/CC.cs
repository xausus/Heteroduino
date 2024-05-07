#region Using region

using System;
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
    public class CC : CCX_Components
    {
        private readonly int[] _lim = new int[3] {1, 255, 180};
        public static readonly string[] _mode = {"Digital", "PWM", "Servo"};
        public static readonly string[] UnoPins = {"13", "12", "11~", "10~", "9~", "8", "6~", "5~", "3~"};
        public static readonly string[] Megapins = { "53","52","51","50","49","48","47","46","45",
            "44","43","42","41","40","39","38","37","36","35","34","33","32","~13","~12","~11","~10",
            "~9","~8","~7","~6","~5","~4","~3","~2"};

        private readonly int[] Uno_pwm_able = {2,3,4,6,7,8};


      bool  pinable(int mod, int pin)
      {
          var v = !GetValue(MegaStr, false);
            return mod == 0 || (mod == 2 && pin > 0) ||
                   (pin > 21 || (v && Uno_pwm_able.Contains(pin)));
        }


        /// <summary>
        ///     Initializes a new instance of the CreateComman class.
        /// </summary>
        public CC()
            : base("Control Command", "CC.Heteroduino",
                "Create command for a pin",0)
        {
        }


        

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.CC;


        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{9d6aad38-2682-4d1f-aa0c-afc21ef8f8d1}");


        /// <summary>
        ///     Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
       
            pManager.AddIntegerParameter("Value", "V", "Pin Value \nfor Digital:" +
                                                       " 0-1\nfor PWM: 0-255\nfor Servo: 0-180",GH_ParamAccess.item);

            pManager.AddIntegerParameter("PinMode", "P", "Pin Mode : 0: Digital    1: PWM   2: Servo" + Resources.ParamOption, GH_ParamAccess.item, 0);
         Param_Integer PinModeNames    = pManager[1] as Param_Integer;
            for (int i = 0; i < 3; i++)
            PinModeNames.AddNamedValue(_mode[i],i);

        }
    
        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_IntegerParam("Command", "CC", "Created Command");
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            
            Menu_AppendObjectName(menu);
            Menu_AppendEnableItem(menu);
            var pin = GetValue("pin",8);
            Menu_AppendSeparator(menu);
            if (GetValue(MegaStr,false))
            {
                var p1 =Menu_AppendItem(menu,"43~53").DropDown;
                var p2= Menu_AppendItem(menu,"32~42").DropDown;
                 var p3= Menu_AppendItem(menu,"PWM 2~13").DropDown;
                 bool v = mod!=1;
                for (var i = 0; i < 11 ; i++)
                    Menu_AppendItem(p1, Megapins[i], megapin, v, GetValue("pin", 2) == i);
               
                for (var i = 11; i < 22; i++)
                    Menu_AppendItem(p2, Megapins[i], megapin, v, GetValue("pin", 2) == i);
                for (var i = 22; i < 34; i++)
                    Menu_AppendItem(p3, Megapins[i], megapin, true, GetValue("pin", 2) == i);
            }
               
            else
 for (int i = 0; i < UnoPins.Length; i++)
                Menu_AppendItem(menu, "Pin: " + UnoPins[i], pinevent, pinable(mod, i), pin == i);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Mega Board", Megaswitch, true, GetValue(MegaStr, false));
            Menu_AppendSeparator(menu);
            Menu_AppendObjectHelp(menu);
            return true;
        }

        private void megapin(object sender, EventArgs e)
        {
            RecordUndoEvent("megapin");
            SetValue("pin", Megapins.ToList().IndexOf(sender.ToString()));
            ExpireSolution(true);
        }

      

        private void pinevent(object sender, EventArgs e)
        {
            RecordUndoEvent("pin#");
            var t=  sender.ToString().Substring(5);
            SetValue("pin",UnoPins.ToList().IndexOf(t));
            ExpireSolution(true);
        }

     

   int mod = 0;
      

        protected override void SolveInstance(IGH_DataAccess DA)
        {
          
            
            var pin = GetValue("pin", 8);
            DA.GetData(1, ref mod);
            mod %= 3;
            var val = 0;
            DA.GetData(0, ref val);
            Message = string.Format("{1}: {0}", _mode[mod],GetValue(MegaStr,false)?Megapins[pin]: UnoPins[pin]);
 Limit(ref val, limit[mod]);
            DA.SetData(0,maker(pin,mod,val));
        }

        private readonly int[] limit = {1, 256, 180};

        int maker(int pin, int mod, int val) =>val| mod<<8|pin<<10;
       
        private  void Limit(ref int x, int max)
        {
            if (x > max)
            {
                x = max;
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"The value is more than expected");
            }
            else
            if (x < 0)
            {
                x = 0;
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "The value is less than expected");
            }
        }

        
    }
}