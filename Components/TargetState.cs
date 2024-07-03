using System;
using System.Linq;
using static Heteroduino.ARDUINO_BOARD;

namespace Heteroduino
{
    public class TargetState
    {
            
     
        public TargetState(int pin, BoardType boardType)
        {

            int validpid = (boardType == BoardType.Uno ?UnoPins.Length: Megapins.Length  ) - 1;
            Pin = Math.Min(pin, validpid);
            Board_Type = boardType;
        }



        public TargetState(string pinname, BoardType pinBoardType)
        {
            Board_Type= pinBoardType;
            var isNotUno = pinBoardType != BoardType.Uno;

            int validpid = (isNotUno? Megapins.Length : UnoPins.Length) - 1;
            var q = (isNotUno ? Megapins : UnoPins).ToList();
            var i = q.IndexOf(pinname);
            if (i == -1) i = 0;
            Pin =Math.Min(i,validpid);
               
        }


        public override string ToString()
            => Board_Type switch
            {
                BoardType.Mega => Megapins[Pin],
                BoardType.Due => Duopins[Pin],
                _ => UnoPins[Pin],
            };
        
        
  

        public static readonly string[] UnoPins = {"13", "12", "~11", "~10", "~9", "8", "~6", "~5", "~3"};
        public static readonly string[] Megapins = { "53","52","51","50","49","48","47","46","45",
            "44","43","42","41","40","39","38","37","36","35","34","33","32","~13","~12","~11","~10",
            "~9","~8","~7","~6","~5","~4","~3","~2"};
        public static readonly string[] Duopins = { "~53", "~52", "~51", "~50", "~49", "~48", "~47", "~46", "~45",
            "~44", "~43", "~42", "~41", "~40", "~39", "~38", "~37", "~36", "~35", "~34", "~33", "~32","~13","~12","~11","~10",
            "~9","~8","~7","~6","~5","~4","~3","~2"};


        public readonly BoardType Board_Type;
        public readonly int Pin;

          

      
        public  bool CheckMode(int mod) => mod != 1 || ToString()[0]=='~';

        public static bool CheckUnoMode(int mod,int pin) => mod != 1 ||  UnoPins[pin][0]=='~';
    }


}
