using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.GUI.Canvas;


namespace Heteroduino
{
    public class AttriSMS : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public AttriSMS(IGH_Component component)
            : base(component)
        {
        }


      
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            GH_Skin.palette_hidden_standard = new GH_PaletteStyle(Color.LightGray, Hds.ardicolor, Hds.ardicolor);
            GH_Skin.palette_hidden_selected = Hds.Selected;
            GH_Skin.palette_warning_standard = Hds.Warning;
            GH_Skin.palette_warning_selected = Hds.Selected;
            GH_Skin.palette_error_standard = Hds.Error;
            GH_Skin.palette_error_selected = Hds.Selected;

            // Allow the base class to render itself.
            base.Render(canvas, graphics, channel);
            // Restore the cached styles.
            GH_Skin.palette_hidden_standard = Hds.StyleStandard;
            GH_Skin.palette_hidden_selected = Hds.StyleStyleSelected;
            GH_Skin.palette_warning_standard = Hds.StyleWStandard;
            GH_Skin.palette_warning_selected = Hds.StyleWSelected;
            GH_Skin.palette_error_standard = Hds.StyleEStandard;
            GH_Skin.palette_error_selected = Hds.StyleESelected;
        }
    }

}