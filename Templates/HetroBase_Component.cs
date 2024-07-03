using Grasshopper.Kernel;

namespace Heteroduino
{
    public abstract class HetroBase_Component : GH_Component
    {





        public HetroBase_Component(string name, string nickname, string description)
            : base(name, nickname, description, "Extra", "Arduino")
        {

         

        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;
    }
}