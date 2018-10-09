namespace Nyancat.Scenes
{
    public class NyancatSceneOptions
    {
        public bool ShowTitle { get; set; } = true;

        public bool ShowCounter { get; set; } = true;

        public int Frames { get; set; } = int.MaxValue;

        public bool Sound { get; set; } = false;
    }
}
