using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper.GUI;
// ReSharper disable All

namespace Heteroduino
{
    public class Attri_ArdiComps : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public Attri_ArdiComps(GH_Component  owner   ) 
            : base(  owner)  
        {
            if (owner is RXF)
                owner.Description += "\n Double-click to auto connect required wires and set other adjustments";
        }
        

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {

            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
             }
            GH_Skin.palette_hidden_standard = Hds.Normal;
            GH_Skin.palette_hidden_selected = Hds.Selected;
                GH_Skin.palette_warning_standard = Hds.Warning;
            GH_Skin.palette_warning_selected = Hds.Selected;
            GH_Skin.palette_error_standard = Hds.Error;
            GH_Skin.palette_error_selected = Hds.Selected;
            base.Render(canvas, graphics, channel);
            GH_Skin.palette_hidden_standard = Hds.StyleStandard;
            GH_Skin.palette_hidden_selected = Hds.StyleStyleSelected;
            GH_Skin.palette_warning_standard = Hds.StyleWStandard;
            GH_Skin.palette_warning_selected = Hds.StyleWSelected;
            GH_Skin.palette_error_standard = Hds.StyleEStandard;
            GH_Skin.palette_error_selected = Hds.StyleESelected;
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
         (Owner as RXF)?.Doubleclick();
            (Owner as CCX_Components)?.Connect();
            Owner.ExpireSolution(true);
            return GH_ObjectResponse.Release;
        }
    }


    


    }



