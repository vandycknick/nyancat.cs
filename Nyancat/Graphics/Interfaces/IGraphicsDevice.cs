using System;
using System.Drawing;

namespace Nyancat.Graphics
{
    public interface IGraphicsDevice
    {
        int Width { get; }

        int Height { get; }

        string Title { get; set; }

        bool IsRunning { get; }

        void Clear(Color color);

        void Clear(char character, Color color);

        void Draw(string message, Position position, Color color);

        void Draw(string message, Position position, Color color, Color background);

        void Draw(ITexture texture, Position position);

        void Render();

        void Exit();
    }
}
