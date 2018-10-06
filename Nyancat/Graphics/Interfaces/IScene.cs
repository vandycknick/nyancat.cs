namespace Nyancat.Graphics
{
    public interface IScene
    {
         void Initialize();

         void Update();

         void Render(IGameTime gameTime);
    }
}
