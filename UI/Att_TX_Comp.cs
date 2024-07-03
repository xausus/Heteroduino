using System.Collections.Generic;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;

namespace Heteroduino
{
    
    public class Att_TX_Comp : GH_ComponentAttributes
    {
        private TX comp;
        public Att_TX_Comp(TX component)
            : base(component)
        {
            comp = component;
        
        }
        private PointF _txPairGrip;

        private void RenderCenterCircle(Graphics g, PointF c, float r)
        {
            g.DrawEllipse(Pens.Black,c.X-r,c.Y-r,r*2,r*2 );
        }

        public static void RenderPairedConnection(Graphics g, PointF anchor, float height, RectangleF box, Color col)
        {
            if (box.Contains(anchor))
                return;
            var txh = anchor.Y + height/2;
            var ch = box.Top + box.Height/2;
            Pen pen = new Pen(col, 5f)
            {
                StartCap = LineCap.Round,
                DashCap = DashCap.Round,
                EndCap = LineCap.Round,
                DashPattern = new float[2]
                {
                    1f,
                    0.8f
                }
            };
            // PointF pt1 = GH_GraphicsUtil.BoxClosestPoint(anchor + new SizeF(20f, 0.0f), box);
            var gap = 5f;
            var ofs = 25f;
            var ccX = box.X + box.Width/2;
            List<PointF> list;


            if (txh < ch)
            {
                var top =( box.Top < anchor.Y ? box.Top : anchor.Y)-ofs;
            list = new List<PointF>
            {
                new PointF(ccX, box.Top ),
                new PointF(ccX, top ),
                new PointF(anchor.X,top),
                new PointF(anchor.X, anchor.Y - gap)
            };
            }
            else

            {
                var bot= (box.Bottom > anchor.Y+height ? box.Bottom : anchor.Y+height) + ofs;
                list = new List<PointF>
                {
                    new PointF(ccX, box.Bottom ),
                    new PointF(ccX, bot),
                    new PointF(anchor.X,bot),
                    new PointF(anchor.X, anchor.Y + height + gap)
                };
            }


            var path = GH_GDI_Util.FilletPolyline(list.ToArray(), 15f);
            g.DrawPath(pen, path);
            path.Dispose();
            pen.Dispose();
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {

            if (channel == GH_CanvasChannel.Wires )
            {
                base.Render(canvas, graphics, channel);
                if (comp.CoreBase == null) return;
                _txPairGrip = new PointF(this.Bounds.X + Bounds.Width / 2, Bounds.Y );
                var rectangle1 = GH_Convert.ToRectangle(comp.CoreBase.Attributes.Bounds);
                var color = this.Selected? GH_Skin.wire_selected_a: GH_Skin.wire_default;
                if (this.Owner.Locked)
                    color = Color.FromArgb(50, color);
                RenderPairedConnection(graphics, _txPairGrip,Bounds.Height,rectangle1 , color);
                return;
            }

            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            GH_Skin.palette_hidden_standard = new GH_PaletteStyle(Hds.ardicolorboard, Color.LightGray, Color.LightGray);
            GH_Skin.palette_hidden_selected = Hds.Selected;
            GH_Skin.palette_warning_standard = Hds.Warning;
            GH_Skin.palette_warning_selected = Hds.Selected;
            GH_Skin.palette_error_standard = Hds.Error;
            GH_Skin.palette_error_selected = Hds.Selected;
            base.Render(canvas, graphics, channel);
            GH_Skin.palette_hidden_standard =Hds. StyleStandard;
            GH_Skin.palette_hidden_selected = Hds.StyleStyleSelected;
            GH_Skin.palette_warning_standard = Hds.StyleWStandard;
            GH_Skin.palette_warning_selected = Hds.StyleWSelected;
            GH_Skin.palette_error_standard = Hds.StyleEStandard;
            GH_Skin.palette_error_selected = Hds.StyleESelected;
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if(e.Button!=MouseButtons.Left)
                return base.RespondToMouseDoubleClick(sender, e);
            comp.RefreshSrources();
            comp.ExpireSolution(true);
            ExpireLayout();
            return GH_ObjectResponse.Release;
        }
    }
}
