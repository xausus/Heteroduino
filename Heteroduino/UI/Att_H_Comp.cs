using Grasshopper.GUI.Canvas;
using System.Drawing;
using Grasshopper.GUI;


namespace Heteroduino
{
    public class Att_H_Comp : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public Att_H_Comp(H_Component  owner   ) 
            : base(  owner)
        {
            Comp = owner;
        }

        private H_Component Comp;
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
            Comp.Doubleclick();
            Comp.Connect();
            Comp.ExpireSolution(true);
            return GH_ObjectResponse.Release;
        }
    }

    }



