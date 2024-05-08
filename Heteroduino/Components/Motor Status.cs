using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace Heteroduino
{



    public class MotorStatus : H_Component
    {
        /// <summary>
        /// Initializes a new instance of the MotorStatus class.
        /// </summary>
        public MotorStatus()
            : base("Motor Status Interpreter","SMS.Heteroduino", 
                "Interpret the feedback of stepper motors gotten from RX")
        {

        }

        public void Addsource() => Tools.GetSource(OnPingDocument(), Params.Input[0], 0);

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
 
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Motor", "M", "Active motors", GH_ParamAccess.list);
            pManager.Register_BooleanParam("Running", "R", "Is Running", GH_ParamAccess.list);
            pManager.Register_IntegerParam("Position", "P", "Current position", GH_ParamAccess.list);
            pManager.Register_IntegerParam("Target", "T", "Target to run", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Speed", "S", "Current Speed", GH_ParamAccess.list);
            int k = pManager.AddIntegerParameter("Acceleration", "A", "Acceleration Mode", GH_ParamAccess.list);
            var sed = new[]
            {
                "Constant speed", "acc Low", "acc Lax", "acc Normal", "acc High", "acc Idle", "Reset motor",
                "Remove Motor"
            };
            Param_Integer pin=pManager[k] as Param_Integer;
            for (int i = 0; i < 8; i++)
          pin.AddNamedValue(sed[i],i);





        }

       
       
       


      


       


       /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        


        public override void AddedToDocument(GH_Document document) => Tools.AddCoreRX(this);


 List<int> index=new List<int>();
            List<bool> runing = new List<bool>();
            List<int> poss =new List<int>();
            List<int> trgs = new List<int>();
            List<double> spds = new List<double>();
        List<int> acc=new List<int>();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var se=new List<string>();
            DA.GetDataList(0, se);
            var s = se.FindLast(i => i.StartsWith("%"));
            if(s==null) goto dasend;
            runing.Clear();
            index.Clear();
            poss.Clear();
            trgs.Clear();
            spds.Clear();
            acc.Clear();
            s = s.Substring(1);
            if(s=="~") return;
            var motors = s.Split('^');
            foreach (var m in motors)
            {
                var sp = m.Split('|');
                if(sp.Length<5)break;
                var pos = Convert.ToInt32(sp[1]);
                var trg= Convert.ToInt32(sp[2]);
                index.Add(Convert.ToInt32(sp[0]));
                poss.Add(pos);
                trgs.Add(trg);
                spds.Add(Convert.ToDouble(sp[3]));
                runing.Add(pos!=trg);
                acc.Add(Convert.ToInt32(sp[4]));

            }

            dasend:
            DA.SetDataList(0, index);
            DA.SetDataList(1, runing);
            DA.SetDataList(2, poss);
            DA.SetDataList(3, trgs);
            DA.SetDataList(4, spds);
            DA.SetDataList(5, acc);
        }
        
        protected override System.Drawing.Bitmap Icon => Properties.Resources.MF;


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("RX", "", "Recievd-Data from arduino", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }

        public override Guid ComponentGuid => new Guid("{1227c6e7-e4cf-4895-9567-9c63ac25ccb7}");
     
    }
}