using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaApplication2.Interfaces;
using AvaloniaApplication2.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaApplication2
{
    public partial class MainWindow : Window
    {
        private List<IMonitorPlugin> _plugins = new();
        private PerformanceCounter? cpuCounter;
        private DispatcherTimer? timer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCpuCounter();
            StartMonitoring();
           
        }
        public class Settings
        {
            public string ApiEndpoint { get; set; }
        }

        private Settings LoadSettings()
        {
            string json = File.ReadAllText("PluginSettings.json");
            return JsonSerializer.Deserialize<Settings>(json);
        }
        private void InitializeCpuCounter()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // prime
            }
        }

        private float GetCpuUsage()
        {
            if (cpuCounter == null) return 0;
            return cpuCounter.NextValue();
        }


        private float GetRamUsage()
        {
            var (percent, _, _) = GetMemoryUsage();
            return (float)percent;
        }


        private async Task<float> GetDiskUsage()
        {
            var diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            diskCounter.NextValue();
            await Task.Delay(1000); // Async wait (non-blocking)
            return diskCounter.NextValue();
        }


        private void StartMonitoring()
        {
            Task.Run(async () =>
            {
                // Prime CPU counter once if needed
                cpuCounter?.NextValue();
                await Task.Delay(500); // wait before first read

                while (true)
                {
                    var cpu = GetCpuUsage();
                    var ram = GetRamUsage();
                    var disk = await GetDiskUsage();

                    foreach (var plugin in _plugins)
                        plugin.OnUpdate(cpu, ram, disk);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CpuProgress.Value = Math.Round(cpu);
                        CpuLabel.Text = $"{Math.Round(cpu)}%";

                        var (usedMemPercent, usedMemMB, totalMemMB) = GetMemoryUsage();
                        RamProgress.Value = Math.Round(usedMemPercent);
                        RamLabel.Text = $"{Math.Round(usedMemMB)} MB / {Math.Round(totalMemMB)} MB ({Math.Round(usedMemPercent)}%)";

                        DiskProgress.Value = Math.Min(100, Math.Max(0, disk));
                        DiskLabel.Text = $"{Math.Round(disk)}%";

                        HandlePluginState();
                    });

                    await Task.Delay(500);
                }
            });
        }

        private void HandlePluginState()
        {
            // Metrics Logger Plugin
            var filePlugin = _plugins.OfType<FileLoggerPlugin>().FirstOrDefault();
            if (MetricsLoggerCheckBox.IsChecked == true)
            {
                if (filePlugin == null)
                {
                    _plugins.Add(new FileLoggerPlugin());
                    StatusText.Text = "Metrics logger started";
                }
            }
            else
            {
                if (filePlugin != null)
                {
                    _plugins.Remove(filePlugin);
                    StatusText.Text = "Metrics logger stopped";
                }
            }

            // API Plugin
            var apiPlugin = _plugins.OfType<ApiPostPlugin>().FirstOrDefault();
            if (ApiCheckBox.IsChecked == true)
            {
                if (apiPlugin == null)
                {
                    var settings = LoadSettings();
                    _plugins.Add(new ApiPostPlugin(settings.ApiEndpoint));
                    StatusText.Text = "API logger started";
                }
            }
            else
            {
                if (apiPlugin != null)
                {
                    _plugins.Remove(apiPlugin);
                    StatusText.Text = "API logger stopped";
                }
            }
        }



        private static (double percent, double usedMB, double totalMB) GetMemoryUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var memInfo = File.ReadAllLines("/proc/meminfo");
                var total = ParseMemValue(memInfo, "MemTotal");
                var free = ParseMemValue(memInfo, "MemAvailable");
                var used = total - free;
                return ((double)used / total * 100, used / 1024, total / 1024);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var output = "";
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "wmic";
                    process.StartInfo.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                }

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var totalLine = lines.FirstOrDefault(l => l.StartsWith("TotalVisibleMemorySize"));
                var freeLine = lines.FirstOrDefault(l => l.StartsWith("FreePhysicalMemory"));

                if (totalLine != null && freeLine != null)
                {
                    var totalKb = ulong.Parse(totalLine.Split('=')[1]);
                    var freeKb = ulong.Parse(freeLine.Split('=')[1]);
                    var usedKb = totalKb - freeKb;
                    return ((double)usedKb / totalKb * 100, usedKb / 1024.0, totalKb / 1024.0);
                }
            }
            return (0, 0, 0);
        }

        private static ulong ParseMemValue(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(key));
            return line != null ? ulong.Parse(line.Split(':')[1].Trim().Split(' ')[0]) : 0;
        }

        private double GetLinuxCpuUsage()
        {
            string[] cpuStats1 = File.ReadLines("/proc/stat").First().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            ulong idle1 = ulong.Parse(cpuStats1[4]);
            ulong total1 = cpuStats1.Skip(1).Select(ulong.Parse).Aggregate((a, b) => a + b);

            Task.Delay(500).Wait();

            string[] cpuStats2 = File.ReadLines("/proc/stat").First().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            ulong idle2 = ulong.Parse(cpuStats2[4]);
            ulong total2 = cpuStats2.Skip(1).Select(ulong.Parse).Aggregate((a, b) => a + b);

            ulong idleDelta = idle2 - idle1;
            ulong totalDelta = total2 - total1;

            return totalDelta == 0 ? 0 : (1.0 - (double)idleDelta / totalDelta) * 100;
        }
    }
}