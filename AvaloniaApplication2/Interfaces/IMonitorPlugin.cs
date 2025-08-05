namespace AvaloniaApplication2.Interfaces
{
    public interface IMonitorPlugin
    {
        string Name { get; }
        void OnUpdate(float cpuUsage, float ramUsage, float diskUsage);
    }
}
