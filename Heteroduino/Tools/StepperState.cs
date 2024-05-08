namespace Heteroduino
{
    public struct StepperState 
    {

        public readonly int Position; 
        public readonly int Speed;
        public readonly int Acceleration ;
        public readonly int Pin;
        public readonly int Code;
        public StepperState(int data)
        {
            Pin = MotorDecompose(data, out Position, out Speed, out Acceleration);
            if (Acceleration >= 5) Acceleration = -1;
            Code = data;
        }

        public override bool Equals(object obj) => obj is StepperState s && s.Pin == this.Pin;
        public override int GetHashCode() => Pin;
        

        public StepperState(int pin, int pos,int spd, int acc)
        {
            Position = pos;
            Speed = spd;
            Acceleration = acc;
            Pin = pin;
            Code = MotorCombine(pin,pos,spd,acc);
        }

        public enum Motorstate{Jack, Reset,Remove}
        public StepperState(int pin, Motorstate state)
        {
            Position = 0;
            Speed = 0;
            Acceleration =(int)state+ 5;
            Pin = pin;
            Code =MotorCombine(pin,Position,Speed,Acceleration);
           
        }

            

        public static int Remove(int pin) =>
            MotorCombine(pin, 0, 0, 7);

        public static int MotorCombine(int pin, int pos, int spd, int acc) =>
            pos & 0xffff | (spd & 0x3ff) << 16 | (pin & 7) << 29 | (acc & 7) << 26;
        public static int MotorDecompose(int data, out int pos, out int spd, out int acc)
        {
           
            pos = (data >> 15 & 1) == 1 ? data | 0xffff << 16 : data & 0x7fff;
            spd = data >> 16 & 0x3ff;
            acc = data >> 26 & 7;
            return data >> 29 & 7;
        }
    }
}