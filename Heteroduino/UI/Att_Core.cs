using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using static Heteroduino.Properties.Resources;

namespace Heteroduino
{
    public class Att_Core : GH_ComponentAttributes
    {
        private readonly Color basecolor = Hds.ardicolorboard;
        private readonly Core comp;
        private readonly Brush offColor = new SolidBrush(Color.FromArgb(174, 20, 55, 62));
        private readonly Brush onColor = new SolidBrush(Color.FromArgb(81, 255, 106, 0));
        private readonly List<RectangleF> rects = new List<RectangleF>();
        private RectangleF[] bram;
        private List<RectangleF> crects = new List<RectangleF>();
        private bool enable;
        private List<PointF> nodes;
        private PointF portnametag;

        public Att_Core(Core component) : base(component)
        {
            comp = component;
        }

        public bool Rx_led { get; set; }
        private bool Tx_led { get;  set; }

        public bool TX_State
        {
            get
            {
                var e = Tx_led;
                Tx_led = false;
                return e;
            }
            set
            {
                Tx_led = value;
                ExpireLayout();
            }
        }

        protected override void Layout()
        {
            if (comp.MegaMode)
                LayOut_Mega();
            else
                LayOut_Uno();
            portnametag = Bounds.Location + new SizeF(45, 170);
        }

        private void LayOut_Uno()
        {
            var msc = 230f;
            var sc = msc / 167.5f;

            var ww = msc;
            var hh = msc / 1.3f;
            base.Layout();
            Bounds = new RectangleF(Pivot.X - ww / 2 + msc / 35, Pivot.Y - hh / 2, ww, hh);

            foreach (var p in Owner.Params.Output)
            {
                var pat = p.Attributes;
                var b = pat.Bounds;
                b.X = Pivot.X + 85;
                pat.Bounds = b;
                var pp = pat.Pivot;
                pp.X += 83;
                pat.Pivot = pp;
            }

            rects.Clear();

            var ss = 5f * sc;
            var s = new SizeF(ss, ss);
            var ba = new PointF(Pivot.X - 20.5f * sc, Pivot.Y - 53.25f * sc);
            for (var i = 0; i < 18; i++)
            {
                if (i == 10) ba.X += 3f * sc;
                rects.Add(new RectangleF(ba, s));
                ba.X += ss;
            }

            ba = new PointF(Pivot.X - 2.24f * sc, Pivot.Y + 48f * sc);
            for (var i = 0; i < 14; i++)
            {
                if (i == 8) ba.X += 4.5f * sc;
                rects.Add(new RectangleF(ba, s));
                ba.X += ss;
            }


            var ly = 2.8f * sc;
            ss += .3f * sc;
            var scx = Pivot.X - 2.8f * sc;
            crects = new List<RectangleF>
            {
                new RectangleF(scx, Pivot.Y - 32f * sc, ss, ly),
                new RectangleF(scx, Pivot.Y - 22.3f * sc, ss, ly),
                new RectangleF(scx, Pivot.Y - 17.3f * sc, ss, ly),
                new RectangleF(Pivot.X + 59f * sc, Pivot.Y - 22.3f * sc, ss, ly)
            };


            rects.AddRange(crects);


            bram = rects.ToArray();
            for (var i = 0; i < bram.Length; i++)
                bram[i].Inflate(-2f, -2f);
            enable = !Owner.Locked && comp.enable;
            nodes = rects.Select(center).ToList();
        }

        private void LayOut_Mega()
        {
            var hh = 177f;
            var msc = hh * 1.89f;
            var sc = 1.48f;
            var ww = msc;
            base.Layout();
            Bounds = new RectangleF(Pivot.X - ww / 2 + msc / 35, Pivot.Y - hh / 2, ww, hh);

            var mover = 142;
            foreach (var p in Owner.Params.Output)
            {
                var pat = p.Attributes;
                var b = pat.Bounds;
                b.X = Pivot.X + mover;
                pat.Bounds = b;
                var pp = pat.Pivot;
                pp.X += mover - 2;
                pat.Pivot = pp;
            }

            rects.Clear();
            var ss = 5f * sc;
            var s = new SizeF(ss, ss);
            var ba = new PointF(Pivot.X - 20.5f * sc, Pivot.Y - 53.25f * sc);
            for (var i = 0; i < 18; i++)
            {
                if (i == 10) ba.X += 3f * sc;
                rects.Add(new RectangleF(ba, s));
                ba.X += ss;
            }

            ba = new PointF(Pivot.X - 2.24f * sc, Pivot.Y + 48f * sc);

            for (var i = 0; i < 14; i++)
            {
                if (i == 8) ba.X += 4.5f * sc;
                rects.Add(new RectangleF(ba, s));
                ba.X += ss;
            }


            var ly = 2.8f * sc;
            ss += .3f * sc;
            var scx = Pivot.X - 36.4f * sc;
            crects = new List<RectangleF>
            {
                new RectangleF(scx, Pivot.Y - 32f * sc, ss, ly),
                new RectangleF(scx, Pivot.Y - 21.3f * sc, ss, ly),
                new RectangleF(scx, Pivot.Y - 16.7f * sc, ss, ly),
                new RectangleF(Pivot.X + 59f * sc, Pivot.Y - 22.3f * sc, ss, ly)
            };

            rects.AddRange(crects);
            bram = rects.ToArray();
            for (var i = 0; i < bram.Length; i++)
                bram[i].Inflate(-2f, -2f);
            enable = !Owner.Locked;
            nodes = rects.Select(center).ToList();
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            var zoom = canvas.Viewport.Zoom;
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Overlay && zoom > .7)
                    graphics.DrawString("Heteroduino Beta 2.91",
                        GH_FontServer.ConsoleSmall, Brushes.White, new PointF(portnametag.X, portnametag.Y + 12));
                return;
            }

            if (comp.MegaMode)
                Render_Mega(graphics, zoom);
            else
                Render_Uno(graphics, zoom);
            if (zoom > .7)
                graphics.DrawString(comp.PortName, GH_FontServer.ConsoleSmall, Brushes.Teal, portnametag);
        }

        private void Render_Uno(Graphics graphics, float zoom)
        {
            var k = Bounds;
            var f = Bounds;
            f.Inflate(-12, -10);
            var cap = GH_Capsule.CreateCapsule(k, GH_Palette.Black, 5, 9);
            cap.AddOutputGrip(Owner.Params.Output[0].Attributes.Pivot);
            //cap.AddOutputGrip(Owner.Params.Output[1].Attributes.Pivot);
            cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (zoom < 1.4)
            {
                graphics.FillPath(new SolidBrush(
                    GH_GraphicsUtil.FadeColour
                        (1.4, 1, zoom, basecolor)), GH_GDI_Util.FilletRectangle(Rectangle.Round(f), 10));
                if (zoom < .5) return;
                var ff = .4;

                GH_GraphicsUtil.RenderFadedImage(graphics, uno, Rectangle.Round(k),
                    (zoom - ff) / (.7 - ff));
                if (zoom < .6) return;
            }
            else
            {
                graphics.DrawImage(uno, k);
            }

            graphics.FillRectangle(offColor, crects[0]);
            graphics.FillRectangle(TX_State ? onColor : offColor, crects[1]);
            graphics.FillRectangle(Rx_led ? onColor : offColor, crects[2]);
            graphics.FillRectangle(enable ? onColor : offColor, crects[3]);
            if (zoom < 1.4) return;

            //   cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (Owner.Locked)
                cap.Render(graphics,
                    new GH_PaletteStyle(Owner.Locked || !comp.enable ? Hds.selcol : Hds.basecol, Color.CadetBlue));

            cap.Dispose();


            //   graphics.DrawRectangles(Pens.DarkBlue, rects.ToArray());
            graphics.FillRectangle(Brushes.DarkGray, bram[comp.Index]);
            int i;
            graphics.DrawLine(Pens.DarkGray, nodes[i = 5], nodes[++i]);
            graphics.DrawLine(Pens.DarkGray, nodes[i = 10], nodes[++i]);
        }

        private void Render_Mega(Graphics graphics, float zoom)
        {
            var k = Bounds;
            var f = Bounds;
            f.Inflate(-12, -10);
            var cap = GH_Capsule.CreateCapsule(k, GH_Palette.Black, 5, 9);
            cap.AddOutputGrip(Owner.Params.Output[0].Attributes.Pivot);
            //cap.AddOutputGrip(Owner.Params.Output[1].Attributes.Pivot);
            cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (zoom < 1.4)
            {
                graphics.FillPath(new SolidBrush(
                        GH_GraphicsUtil.FadeColour
                            (1.4, 1, zoom, basecolor)),
                    GH_GDI_Util.FilletRectangle(Rectangle.Round(f), 10));
                if (zoom < .5) return;
                var ff = .4;

                GH_GraphicsUtil.RenderFadedImage(graphics, Mega, Rectangle.Round(k),
                    (zoom - ff) / (.7 - ff));
                if (zoom < .6) return;
            }
            else
            {
                graphics.DrawImage(Mega, k);
            }

            graphics.FillRectangle(offColor, crects[0]);
            graphics.FillRectangle(TX_State ? onColor : offColor, crects[1]);
            graphics.FillRectangle(Rx_led ? onColor : offColor, crects[2]);
            graphics.FillRectangle(enable ? onColor : offColor, crects[3]);
            if (zoom < 1.4) return;

            //   cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (Owner.Locked)
                cap.Render(graphics, new GH_PaletteStyle(Owner.Locked ? Hds.selcol : Hds.basecol, Color.CadetBlue));

            cap.Dispose();
        }

        private PointF center(RectangleF r)
        {
            return new PointF(r.X + r.Width / 2, r.Y + r.Height / 2);
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (sender.Viewport.Zoom < 1.5) return base.RespondToMouseDown(sender, e);
            var k = sender.CursorCanvasPosition;
            comp.baraks = k;

            var index = rects.FindIndex(i => i.Contains(k));
            if (index >= 0) comp.Index = index;
            ExpireLayout();
            return base.RespondToMouseDown(sender, e);
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            comp.Resetports();
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}