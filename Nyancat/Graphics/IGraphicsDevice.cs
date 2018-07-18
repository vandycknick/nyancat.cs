using System;

namespace Nyancat.Graphics
{
    public interface IGraphicsDevice : IDisposable
    {
        int Width { get; }

        int Height { get; }

        string Title { get; set; }

        bool IsRunning { get; }

        Action OnResize { set; }

        void Fill(char character, string color);

        void Write(char character, string color);

        void NewLine();

        void MoveTo(int row, int col);

        void SwapBuffers();

        void Exit();
    }
}
