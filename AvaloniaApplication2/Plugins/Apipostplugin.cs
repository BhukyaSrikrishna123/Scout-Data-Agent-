using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AvaloniaApplication2.Interfaces; 

namespace AvaloniaApplication2.Plugins
{
    public class ApiPostPlugin : IMonitorPlugin
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiUrl;

        public string Name => "API Post Plugin";

        public ApiPostPlugin(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public async void OnUpdate(float cpu, float ramUsed, float diskUsed)
        {
            var data = new
            {
                cpu = cpu,
                ram_used = ramUsed,
                disk_used = diskUsed
            };

            try
            {
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to API: {ex.Message}");
            }
        }
    }
}

