using System;
using System.Linq;

namespace Heteroduino
{
    public class PINState
    {
            
     
        public PINState(int pin, bool mega)
        {

            int validpid = (mega ? Megapins.Length : UnoPins.Length) - 1;
            Pin = Math.Min(pin, validpid);
            Mega = mega;
        }



        public PINState(string pinname, bool pinMega)
        {
            Mega= pinMega;

            int validpid = (pinMega ? Megapins.Length : UnoPins.Length) - 1;
            var q = (Mega ? Megapins : UnoPins).ToList();
            var i = q.IndexOf(pinname);
            if (i == -1) i = 0;
            Pin =Math.Min(i,validpid);
               
        }

        public override string ToString()
            => Mega ? Megapins[Pin] : UnoPins[Pin];

        public static readonly string[] UnoPins = {"13", "12", "~11", "~10", "~9", "8", "~6", "~5", "~3"};
        public static readonly string[] Megapins = { "53","52","51","50","49","48","47","46","45",
            "44","43","42","41","40","39","38","37","36","35","34","33","32","~13","~12","~11","~10",
            "~9","~8","~7","~6","~5","~4","~3","~2"};

     

        public readonly bool Mega;
        public readonly int Pin;

          

      
        public  bool CheckMode(int mod) => mod != 1 || ToString()[0]=='~';

        public static bool CheckUnoMode(int mod,int pin) => mod != 1 ||  UnoPins[pin][0]=='~';
    }


}
