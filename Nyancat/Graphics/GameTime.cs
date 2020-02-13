using System;

namespace Nyancat.Graphics
{
    public struct GameTime : IGameTime
    {
        public static GameTime Zero = new GameTime
        {
            ElapsedGameTime = TimeSpan.Zero,
            TotalGameTime = TimeSpan.Zero,
        };

        public TimeSpan ElapsedGameTime { get; private set; }
        public TimeSpan TotalGameTime { get; private set; }

        public void Update(TimeSpan elapsed)
        {
            ElapsedGameTime = elapsed;
            TotalGameTime += elapsed;
        }
    }
}
