using System;

namespace Nyancat.Graphics
{
    public class GameTime : IGameTime
    {
        public TimeSpan ElapsedGameTime => _elapsedTime;
        public TimeSpan TotalGameTime => _totalTime;

        private TimeSpan _elapsedTime;
        private TimeSpan _totalTime;

        public GameTime()
        {
            _elapsedTime = _totalTime = TimeSpan.Zero;
        }

        public void Update(TimeSpan elapsed)
        {
            _elapsedTime = elapsed;
            _totalTime += elapsed;
        }
    }
}
