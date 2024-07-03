#region Using region

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Heteroduino.Properties;
using static Heteroduino.ARDUINO_BOARD;


// ReSharper disable All

#endregion

namespace Heteroduino
{
    public class CC : HetroBase_Component,IArduinoController
    {


        public static readonly string[] _mode = { "Digital", "PWM", "Servo" };



        /// <summary>
        ///     Initializes a new instance of the CreateComman class.
        /// </summary>
        public CC()
            : base("Control Command", "CC.Heteroduino",
                "Create command for a pin")
        {

        }

        /// <summary>
        ///     Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.CC;

        /// <summary>
        ///     Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{9d6aad38-2682-4d1f-aa0c-afc21ef8f8d1}");


        /// <summary>
        ///     Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddIntegerParameter("Value", "V", "PIN Value \nfor Digital:" +
                                                       " 0-1\nfor PWM: 0-255\nfor Servo: 0-180", GH_ParamAccess.item);
            pManager.AddIntegerParameter("PinMode", "P", "PIN Mode : 0: Digital    1: PWM   2: Servo" + Resources.PinParam, GH_ParamAccess.item, 0);
            Param_Integer PinModeNames = pManager[1] as Param_Integer;
            for (int i = 0; i < 3; i++)
                PinModeNames.AddNamedValue(_mode[i], i);

            pManager[0].Optional = true;
            pManager[1].Optional = true;

        }

        /// <summary>
        ///     Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.Register_IntegerParam("Command", "CC", "Created Command");
        }

        private TargetState _target;

        public TargetState Target
        {
            get => _target ??= DefrinePin();
            set
            {
                if (_target.Pin != value.Pin)
                    SetValue("pin", value.Pin);
                if (_target.Board_Type != value.Board_Type)
                {
                    SetValue("board", value.Board_Type.ToString());
                    LastBoard = value.Board_Type;
                }
                _target = value;
            }
        }


        private TargetState DefrinePin() => new TargetState(GetValue("pin", 8), RetriveBoard());

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {

            Menu_AppendObjectName(menu);
            Menu_AppendEnableItem(menu);
            Menu_AppendSeparator(menu);

            var p = Target.Pin;
            if (Target.Board_Type !=BoardType.Uno )
            {
                var p1 = Menu_AppendItem(menu, "43~53").DropDown;
                var p2 = Menu_AppendItem(menu, "32~42").DropDown;
                var p3 = Menu_AppendItem(menu, "PWM 2~13").DropDown;
                bool v = MOD != 1;

                for (var i = 0; i < 11; i++)
                    Menu_AppendItem(p1, "PIN: " + TargetState.Megapins[i], changePin, v, p == i);
                for (var i = 11; i < 22; i++)
                    Menu_AppendItem(p2, "PIN: " + TargetState.Megapins[i], changePin, v, p == i);
                for (var i = 22; i < 34; i++)
                    Menu_AppendItem(p3, "PIN: " + TargetState.Megapins[i], changePin, true, p == i);
            }

            else

                for (int i = 0; i < TargetState.UnoPins.Length; i++)
                    Menu_AppendItem(menu, "PIN: " + TargetState.UnoPins[i], changePin, TargetState.CheckUnoMode(MOD, i), p == i);
            Menu_AppendSeparator(menu);

            var pinset = Menu_AppendItem(menu, "Board Type ").DropDown;

            foreach (var s in Extensions.GetEnumArray<BoardType>().Skip(1))
                Menu_AppendItem(pinset, s.ToString(), changeBoard, true, s == this.Target.Board_Type);

            Menu_AppendSeparator(menu);
            Menu_AppendObjectHelp(menu);
            return true;
        }




        private void changePin(object sender, EventArgs e)
        {
            var f = this.Target.Board_Type switch
                {
                    BoardType.Uno => TargetState.UnoPins,
                    BoardType.Mega => TargetState.Megapins,
                    BoardType.Due => TargetState.Duopins,
                    _ => TargetState.UnoPins
                };

            var s =sender.ToString();
            var index = f.ToList().FindIndex(i => s.EndsWith(i));

         RecordUndoEvent("Pin Changed");
         SetValue("pin",index);
         this.Target = new TargetState(index, Target.Board_Type);
            ExpireSolution(true);
        }


        private void changeBoard(object sender, EventArgs e)
        {

            if (Enum.TryParse(sender.ToString(), out BoardType b))
            {
                if (b == Target.Board_Type) return;
                RecordUndoEvent("Change to " + b);
                OnChangeBoard(b);
                ExpireSolution(true);
            }
        }

        public void OnChangeBoard(BoardType board)
        {

            if (board == BoardType.Uno && Target.Board_Type != BoardType.Uno)
                Target = new TargetState(Target.Pin % TargetState.UnoPins.Length, BoardType.Uno);
            else
              Target = new TargetState(Target.Pin, board);
           
            Show();
        }


        BoardType RetriveBoard()
        {
            var j = GetValue("board", null);
            if (j != null && BoardType.TryParse(j, out BoardType b)) 
                      return b;

            SetValue("board",LastBoard.ToString());
            return LastBoard;
        }

        int MOD = 0;

        void Show()=>    Message = $"{Target}: {_mode[MOD]}";
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            DA.GetData(1, ref MOD);
            MOD %= 3;


        Show();
            if (!Target.CheckMode(MOD))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Change the pin to PWM one");
                return;
            }
            var val = 0;
            if (!DA.GetData(0, ref val)) return;

            Limit(ref val, limit[MOD]);
            DA.SetData(0, maker(Target.Pin, MOD, val));
        }

        private readonly int[] limit = { 1, 256, 180 };


        int maker(int pin, int mod, int val) => val | mod << 8 | pin << 10;

        private void Limit(ref int x, int max)
        {
            if (x > max)
            {
                x = max;
                base.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "The value is more than expected");
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