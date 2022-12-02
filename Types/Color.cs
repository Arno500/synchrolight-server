using System.Drawing;

namespace LightServer.Types
{
    public struct Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color(string hex)
        {
            var color = ColorTranslator.FromHtml(hex);
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public Color(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
    }
}
