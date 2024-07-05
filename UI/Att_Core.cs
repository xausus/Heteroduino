using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Special;
using Heteroduino;
using Heteroduino.Properties;
using static Grasshopper.GUI.GH_GraphicsUtil;
using static Heteroduino.GEx;


namespace Heteroduino
{
    public static class GEx
    {

        public static void Circle(this Graphics graphics, PointF center, float radius, Brush brush) =>
            graphics.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
        public static void Circle(this Graphics graphics, PointF center, float radius, Pen pen) =>
            graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2);


      public static List<RectangleF> PinStrip(PointF pivot,PointF start, float end,int number, bool vertical,out float size)
        {
            var r = new List<RectangleF>();
            size= (end - (vertical?start.Y:start.X))/(number-1);
            var x = start.X - size / 2+pivot.X;
            var y = start.Y - size / 2+pivot.Y;

            if (vertical)
            {
                for (int i = 0; i < number; i++)
                    r.Add(new RectangleF(x , y+ size * i, size, size));
            }
            else
            {
                           for (int i = 0; i < number; i++)
                               r.Add(new RectangleF(x + size * i, y, size, size)); 
            }

            return r;
        }

      public static List<RectangleF> PinStrip(PointF pivot, PointF start, float size, int number, bool vertical)
      {
          var r = new List<RectangleF>();
          var x = start.X - size / 2 + pivot.X;
          var y = start.Y - size / 2 + pivot.Y;

          if (vertical)
          {
              for (int i = 0; i < number; i++)
                  r.Add(new RectangleF(x, y + size * i, size, size));
          }
          else
          {
              for (int i = 0; i < number; i++)
                  r.Add(new RectangleF(x + size * i, y, size, size));
          }

          return r;
      }


    }

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

        private bool RX;


        private bool Tx_led;

        public Att_Core(Core component) : base(component)
        {
            comp = component;


            comp.SerialChangeState += OnSerialChanged;

            comp.Jump += () => { TX_State = true; };
        }


        public bool TX_State
        {
            get
            {
                var e = Tx_led;
                if (Tx_led)
                {
                    Tx_led = false;
                    ExpireLayout();
                }

                return e;
            }
            set
            {
                Tx_led = value;
                if (value) ExpireLayout();
            }
        }


        private void OnSerialChanged(SerialPort serial,bool state)
        {
            enable = state;
            ExpireLayout();
        }

        protected override void Layout()
        {
            RX = comp.Rx_led && enable && !comp.Locked;
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

            var y1 = -71.25f;
            var y2 = 70.82f;
            var x1 = 144.24f;
         

            rects.AddRange(PinStrip(Pivot,new PointF(-76.42f,y1),-9.23f,10,false,out var s));
            rects.AddRange(PinStrip(Pivot,new PointF(2.95f,y1),s,8,false));
            rects.AddRange(PinStrip(Pivot,new PointF(70,y1),s,8,false));

            rects.AddRange(PinStrip(Pivot,new PointF(-50.0f,y2),s,8,false));
            rects.AddRange(PinStrip(Pivot,new PointF(17.44f,y2),s,8,false));
            rects.AddRange(PinStrip(Pivot,new PointF(84.82f,y2),s,8,false));

  
            rects.AddRange(PinStrip(Pivot,new PointF(x1,y1),s,18,true));
            rects.AddRange(PinStrip(Pivot,new PointF(x1+s,y1),s,18,true));



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

            nodes = rects.Select(center).ToList();
        }


      

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            var zoom = canvas.Viewport.Zoom;
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Overlay && zoom > .7)
                    graphics.DrawString("Heteroduino " + comp.__version,
                        GH_FontServer.ConsoleSmall, Brushes.White, new PointF(portnametag.X, portnametag.Y + 12));
                return;
            }

            if (comp.MegaMode)
                Render_Mega(graphics, zoom);
            else
                Render_Uno(graphics, zoom);
    
            if (!(zoom > .7)) return;
         if(jump!=null)   graphics.DrawLine(Pens.DarkGray, nodes[jump[0]], nodes[jump[1]]);
            graphics.FillRectangle(offColor, crects[0]);
            graphics.FillRectangle(TX_State ? onColor : offColor, crects[1]);
            graphics.FillRectangle(RX ? onColor : offColor, crects[2]);
            graphics.FillRectangle(enable ? onColor : offColor, crects[3]);
            graphics.DrawString(comp.ActiveBoard?.ToString() ?? "No Device!  ", GH_FontServer.ConsoleSmall,
                Brushes.Teal, portnametag);
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
                    FadeColour
                        (1.4, 1, zoom, basecolor)), GH_GDI_Util.FilletRectangle(Rectangle.Round(f), 10));
                if (zoom < .5) return;
                var ff = .4;

                RenderFadedImage(graphics, Resources.Uno, Rectangle.Round(k),
                    (zoom - ff) / (.7 - ff));
                if (zoom < .6) return;
            }
            else
            {
                graphics.DrawImage(Resources.Uno, k);
            }

            if (zoom < 1.4) return;

            //   cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (Owner.Locked)
                cap.Render(graphics,
                    new GH_PaletteStyle(Owner.Locked || !enable ? Hds.selcol : Hds.basecol, Color.CadetBlue));
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
                        FadeColour
                            (1.4, 1, zoom, basecolor)),
                    GH_GDI_Util.FilletRectangle(Rectangle.Round(f), 10));
                if (zoom < .5) return;
                var ff = .4;

                RenderFadedImage(graphics, Resources.Mega, Rectangle.Round(k),
                    (zoom - ff) / (.7 - ff));
                if (zoom < .6) return;
            }
            else
            {
                graphics.DrawImage(Resources.Mega, k);
            }

            if (zoom < 1.4) return;
            //   cap.RenderEngine.RenderGrips_Alternative(graphics);
            if (Owner.Locked)
                cap.Render(graphics, new GH_PaletteStyle(Owner.Locked ? Hds.selcol : Hds.basecol, Color.CadetBlue));
            cap.Dispose();
            graphics.FillRectangle(Brushes.DarkGray, bram[comp.Index]);
            int i;
            graphics.DrawLine(Pens.DarkGray, nodes[i = 5], nodes[++i]);
            graphics.DrawLine(Pens.DarkGray, nodes[i = 10], nodes[++i]);
          //  graphics.Circle(comp.baraks, 2, Brushes.PowderBlue);
         //   graphics.DrawRectangles(pento, rects.ToArray());
        //    RenderCenteredText(graphics, $"[{(comp.baraks.X - Pivot.X):0.00} , {(comp.baraks.Y - Pivot.Y):0.00}]" ,GH_FontServer.Small,Color.Azure, comp.baraks+offf);
         

        }

        private readonly SizeF offf = new SizeF(0, 6);

        private readonly Pen pento = new Pen(Brushes.LemonChiffon, .4f);
        private int[] jump=null;

        private PointF center(RectangleF r)
        {
            return new PointF(r.X + r.Width / 2, r.Y + r.Height / 2);
        }

   
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {

            if ( (e.Button & MouseButtons.Left) == 0 
                 || sender.Viewport.Zoom < 1.5  ) 
                return base.RespondToMouseDown(sender, e);
            

            var k = sender.CursorCanvasPosition;
           comp.baraks = k;
           var index = rects.FindIndex(i => i.Contains(k));
           if (index >= 0)
           {

               if (index == 1 + comp.Index) jump = new[] { comp.Index, index };
               comp.Index = index;
           }

           return base.RespondToMouseDown(sender, e);

        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            comp.Resetports();
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }

   


}