using Grasshopper.Kernel;

namespace Heteroduino
{
    public abstract class H_Component : HetroBase_Component,RXF
    {
        protected H_Component(string name, string nickname, string description) : base(name, nickname, description)
        {
        }
 
        public virtual bool Connect() => true;

        public virtual void Doubleclick()
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override void CreateAttributes() => m_attributes = new Att_H_Comp(this);



    }
}