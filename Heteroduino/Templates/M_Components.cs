using System;
using Grasshopper.Kernel;
using static Heteroduino.Tools;


// ReSharper disable All

namespace Heteroduino
{
    public abstract class M_Components : HetroBase_Component
    {
        public M_Components(string name, string nickname, string description,int P)
            : base(name, nickname, description)
        {
          
            Connector =P;
          
        }

        private bool _mega;
        private int Connector;
      
        public readonly string MegaStr = "Mega";
     

        public bool Mega
        {
            get { return _mega; }
            set
            {
                _mega = value;
                SetValue("Mega", _mega);
            }
        }

        public override void AddedToDocument(GH_Document document)
        {
          //  base.AddedToDocument(document);
            doc = OnPingDocument();
            _mega = GetValue("Mega", false);
        }

   
 internal GH_Document doc;
 public virtual bool Connect()
 {
   Mega= checkmegatx(this);
    // Message = Params.Output[0].Recipients.Count.ToString();
                return     Connectparam<TX>(doc,Params.Output[0] ,Connector);
                
 }

         public void Megaswitch(object sender, EventArgs e)
        {

          RecordUndoEvent("Megamode");
             Mega = !Mega;
         ExpireSolution(true);
        }

    }
   
}
