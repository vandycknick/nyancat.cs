namespace Nyancat.Graphics
{
    public interface IScene
    {
        void Initialize();

        void Update(IGameTime gameTime);

        void Render(IGameTime gameTime);
    }
}
