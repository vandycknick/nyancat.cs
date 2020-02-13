namespace Nyancat
{
    public class NyancatOptions
    {
        public bool ShowIntro { get; set; } = false;
        public bool ShowTitle { get; set; } = true;
        public bool ShowCounter { get; set; } = true;
        public int Frames { get; set; } = int.MaxValue;
        public bool Sound { get; set; } = false;
    }
}
