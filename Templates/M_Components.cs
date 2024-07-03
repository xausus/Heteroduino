using System;using Grasshopper.Kernel;
using static Heteroduino.ARDUINO_BOARD;
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


        private int Connector;



        public virtual bool Connect()
 {
 //  Mega= checkmegatx(this);
    // Message = Params.Output[0].Recipients.Count.ToString();
                return     Connectparam<TX>(OnPingDocument(),Params.Output[0] ,Connector);
                
 }

        protected bool isNonUno ;

       

    }
   
}
