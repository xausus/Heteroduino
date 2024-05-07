#region Using region

using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas;
using Color = System.Drawing.Color;

#endregion

namespace Heteroduino
{
    public struct Hds
    {
        public static readonly Color ardicolor = Color.FromArgb(0x6200788e);
        public static readonly Color ardicolorError = Color.FromArgb(0x62000000);
        public static readonly Color ardicolortrans = Color.FromArgb(0x7800788e);
        public static readonly Color ardicolortrand = Color.FromArgb(0xf006977);
        public static readonly Color ardicolorlight = Color.FromArgb(0x7f00929f);
        public static readonly Color ardicolorERR = Color.FromArgb(0x7f183147);
        public static readonly Color ardicolorboard = Color.FromArgb(-0xff8772);
        public static readonly Color ardiTransparent = Color.FromArgb(0xf6f6f6);

        public static readonly GH_PaletteStyle Normal = new GH_PaletteStyle(ardicolorlight, ardicolor, ardicolor);
        public static readonly GH_PaletteStyle Selected =new GH_PaletteStyle(ardicolor, Color.LightGray, Color.White);
        public static readonly GH_PaletteStyle Warning = new GH_PaletteStyle(ardiTransparent, ardicolor, Color.DimGray);
        public static readonly GH_PaletteStyle Error = new GH_PaletteStyle(ardicolorError, ardicolor, Color.NavajoWhite);

        public static readonly GH_PaletteStyle StyleStandard = GH_Skin.palette_hidden_standard;
        public static readonly GH_PaletteStyle StyleStyleSelected = GH_Skin.palette_hidden_selected;
        public static readonly GH_PaletteStyle StyleWStandard = GH_Skin.palette_warning_standard;
        public static readonly GH_PaletteStyle StyleWSelected = GH_Skin.palette_warning_selected;
        public static readonly GH_PaletteStyle StyleEStandard = GH_Skin.palette_error_standard;
        public static readonly GH_PaletteStyle StyleESelected = GH_Skin.palette_error_selected;


        public static readonly Color basecol = Color.FromArgb(25, Color.CadetBlue);
        public static Pen mypen = new Pen(Color.Navy, 2) {DashStyle = DashStyle.Dash, DashPattern = new[] {2f, 3f}};

        public static readonly Color selcol = Color.FromArgb(90, Color.CadetBlue);
    }
}