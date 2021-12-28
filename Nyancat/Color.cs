namespace Nyancat
{
    public struct Color
    {
        public Color(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public override bool Equals(object obj) => obj is Color other &&
            this.R == other.R && this.G == other.G && this.B == other.B;

        public override int GetHashCode() => (R, G, B).GetHashCode();

        public static bool operator ==(Color colorA, Color colorB) => colorA.Equals(colorB);
        public static bool operator !=(Color colorA, Color colorB) => !(colorA == colorB);

        public static Color FromRgb(int r, int g, int b) =>
            new Color(r, g, b);

        public static Color Blue = Color.FromRgb(0, 0, 95);
        public static Color White = Color.FromRgb(255, 255, 255);
        public static Color Black = Color.FromRgb(0, 0, 0);
        public static Color Tan = Color.FromRgb(255, 255, 215);
        public static Color Pink = Color.FromRgb(255, 192, 203);
        public static Color DarkRed = Color.FromRgb(215, 0, 135);
        public static Color Red = Color.FromRgb(255, 0, 0);
        public static Color Orange = Color.FromRgb(255, 165, 0);
        public static Color Yellow = Color.FromRgb(255, 255, 0);
        public static Color Green = Color.FromRgb(135, 255, 0);
        public static Color LightBlue = Color.FromRgb(0, 135, 255);
        public static Color DarkBlue = Color.FromRgb(0, 0, 175);
        public static Color Gray = Color.FromRgb(88, 88, 88);
        public static Color LightPink = Color.FromRgb(215, 135, 175);
    }
}
