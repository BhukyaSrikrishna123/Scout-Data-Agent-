# Scout Data Agent – System Resource Monitor App
Scout Data Agent is a cross-platform system monitoring tool built using .NET 8 and AvaloniaUI. The application continuously collects key system resource metrics like:
1.CPU Usage 2.RAM Usage 3.Disk Usage
Key Features
Clean UI built with Avalonia
Real-time system resource monitoring
Plugin-based architecture to:
✅ Log metrics to a local file
✅ Send metrics to a remote API (e.g., a webhook)
Responsive checkbox system — you can enable/disable plugins anytime during app runtime.
Simple, visually appealing interface with clearly labeled controls and status messages.

# Directory and File Structure
├── AvaloniaApplication2/    
│   ├── Dependencies
│   ├── Interfaces/                
│   │   ├── IMonitorPlugin.cs    
│   ├── Plugins/                 
│   │   ├── FileLoggerPlugin.cs    
│   │   └── ApiPostPlugin.cs        
│   ├── App.axaml/                  
│   │   └──App.axaml.cs
│   ├── Mainwindow.axamal/                
│   │    └── Mainwindow.axamal.cs    
│   │         └── Mainwindow       
│   ├── PluginSettings.json   
│   ├── Program.cs             

# How Each Part Works
1.  Interfaces
    IMonitorPlugin.cs: Interface that all plugins must implement (OnUpdate() method).
2.  Plugins
    FileLoggerPlugin.cs: Logs system metrics to a file every 2 seconds.
    ApiPostPlugin.cs: Sends metrics as a POST request to an API endpoint 
3.  App.axaml
    Application-level styles, themes, and resources.
4.  Mainwindow.axamal:
   Defines the layout of the application UI using Avalonia XAML:
   Contains checkboxes for enabling/disabling logging plugins.
   Displays CPU/RAM/Disk usage in a friendly UI.
   Shows status messages when plugins are toggled.

    MainWindow.axaml.cs:
   This is the code-behind file:
   Initializes performance counters
   Starts monitoring on app startup
   Reads checkbox state every 500ms and activates/deactivates plugins accordingly
   Updates UI with messages like “API logger started” or “Metrics logger stopped”

6. PluginSettings.json
   Provides an API Endpoint

