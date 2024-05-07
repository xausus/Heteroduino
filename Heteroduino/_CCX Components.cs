using System;
using Grasshopper.Kernel;
using static Heteroduino.Tools;


// ReSharper disable All

namespace Heteroduino
{
    public abstract class CCX_Components : GH_Component
    {
        public CCX_Components(string name, string nickname, string description,int P)
            : base(name, nickname, description, "Heteroptera", "Arduino")
        {
          
            Connector =P;
          
        }

        private bool _mega;
        private int Connector;
      
        public readonly string MegaStr = "Mega";
        public override GH_Exposure Exposure => GH_Exposure.secondary;

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
 public bool Connect()
 {
   Mega= checkmegatx(this);
    // Message = Params.Output[0].Recipients.Count.ToString();
                return     Connectparam<TX>(doc,Params.Output[0] ,Connector);
          

 }

  public override void CreateAttributes() => m_attributes = new Attri_ArdiComps(this);

         public void Megaswitch(object sender, EventArgs e)
        {

          RecordUndoEvent("Megamode");
             Mega = !Mega;
         ExpireSolution(true);
        }

        

    }

   
}
