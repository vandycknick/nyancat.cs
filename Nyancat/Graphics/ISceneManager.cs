namespace Nyancat.Graphics
{
    public interface ISceneManager
    {
        IScene GetCurrentScene();

        void GoTo<T>();
    }
}
