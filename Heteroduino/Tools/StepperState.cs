namespace Heteroduino
{
    internal struct StepperState
    {
        private  int _position;
        private  int _speed;
        private  int _acceleration;
        private  int _pin;
        private  int code;

        public int Position => _position;
        public int Speed => _speed;
        public int Acceleration => _acceleration<5?_acceleration:-1;
        public int Pin => _pin;
        public int Code => code;
        public StepperState(int data)
        {
            _pin = MotorDecompose(data, out _position, out _speed, out _acceleration);
            code = data;
        }
        
        public StepperState(int pin, int pos,int spd, int acc)
        {
            _position = pos;
            _speed = spd;
            _acceleration = acc;
            _pin = pin;
            code = MotorCombine(pin,pos,spd,acc);
        }

        public enum Motorstate{Jack, Reset,Remove}
        public StepperState(int pin, Motorstate state)
        {
            _position = 0;
            _speed = 0;
            _acceleration =(int)state+ 5;
            _pin = pin;
            code =MotorCombine(pin,_position,_speed,_acceleration);
           
        }

        public void Remove() => _acceleration = 7;
            

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