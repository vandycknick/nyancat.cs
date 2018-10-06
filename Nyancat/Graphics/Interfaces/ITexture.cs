namespace Nyancat.Graphics
{
    public interface ITexture
    {
         int Width { get; }
         int Height { get; }

         ConsoleChar[,] ToBuffer();
    }
}
