namespace Symphony.Shell.WindowManagement
{
    public interface IWindowPlacementSettings
    {
        string GetPlacementAsXml();
        void Save(string placementAsXml);
    }
}