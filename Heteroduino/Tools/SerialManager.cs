using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
namespace Heteroduino
{
    public class SerialManager
    {
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern int IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern IntPtr IOServiceMatching(string name);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern IntPtr IORegistryEntryCreateCFProperty(int entry, IntPtr key, 
            IntPtr allocator, uint options);

        public static string GetM()
        {
            IntPtr matchingDict = IOServiceMatching("IOSerialBSDClient");
            IntPtr iterator = IntPtr.Zero;

            IOServiceGetMatchingService(0, matchingDict);
            IOServiceGetMatchingService(0, matchingDict);

            
            while (true)
            {
                IntPtr key = (IntPtr)0x696f7073; // 'iop' in ASCII
                IntPtr value = IORegistryEntryCreateCFProperty((int)iterator, key, IntPtr.Zero, 0);
                if (value != IntPtr.Zero)
                {
                    string portName = Marshal.PtrToStringAnsi(value);
                   return portName;
                }

                break;
            }

            return "nono";
        }   
            
            
            
            
    }
}