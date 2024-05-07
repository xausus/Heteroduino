using System;
using System.Drawing;
using Grasshopper.Kernel;
// ReSharper disable All

namespace Heteroduino
{
    public class HeteroduinoInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Heteroduino";
            }
        }
        public override Bitmap Icon
        {
            get => Properties.Resources.arduilogo;
        }
        public override string Description
        {
            get => "This is an easy to use plug-in, created to work simultaneously with different Sensors and Actuators.\nThis plugin is designed to work with an Arduino UNO board, programmed via “Heteroduino Firmata”. ";
        }
        public override Guid Id
        {
            get
            {
                return new Guid("cbc9fa6f-60cf-4590-ad47-77f196775232");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Helioripple Studio: \nAmin Bahrami\nHoda Farazandeh\nNiloofar Najafi\nSonay Servatkhah";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "Helioripple@gmail.com ";
            }
        }
    }
}
