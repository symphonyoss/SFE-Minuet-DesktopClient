namespace Paragon.Plugins
{
    public interface IParagonPlugin
    {
        void Initialize(IApplication application);

        void Shutdown();
    }
}