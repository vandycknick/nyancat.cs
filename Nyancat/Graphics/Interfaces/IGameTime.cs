using System;

namespace Nyancat.Graphics
{
    public interface IGameTime
    {
        TimeSpan ElapsedGameTime { get; }
        TimeSpan TotalGameTime { get; }
    }
}
