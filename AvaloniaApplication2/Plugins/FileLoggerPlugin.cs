using AvaloniaApplication2.Interfaces;
using System;
using System.IO;

public class FileLoggerPlugin : IMonitorPlugin
{
    public string Name => "File Logger";

    private readonly string _logFilePath;

    public FileLoggerPlugin()
    {
        string logsDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsDirectory); // Ensure folder exists

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logFilePath = Path.Combine(logsDirectory, $"metrics_log_{timestamp}.txt");
    }

    public void OnUpdate(float cpuUsage, float ramUsage, float diskUsage)
    {
        string log = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: CPU={cpuUsage}% RAM={ramUsage}% DISK={diskUsage}%";
        File.AppendAllText(_logFilePath, log + Environment.NewLine);
    }
}
