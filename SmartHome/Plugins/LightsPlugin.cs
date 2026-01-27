using Microsoft.SemanticKernel;
using SmartHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartHome.Plugins
{
    public class LightsPlugin
    {
        private readonly string _filePath = "Data/lights.json";
        private List<LightModel> _lights;

        public LightsPlugin()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _lights = JsonSerializer.Deserialize<List<LightModel>>(json);
            }
            else
            {
                _lights = new List<LightModel>();
            }
        }

        [KernelFunction("get_lights")]
        [Description("Gets a list of lights and their current state")]
        public List<LightModel> GetLights()
        {
            return _lights;
        }

        [KernelFunction("change_state")]
        [Description("Change the state of a light")]
        public LightModel? ChangeState(int id, bool isOn)
        { 
            var light = _lights.FirstOrDefault(l => l.Id == id);

            if (light == null)
            {
                return null;
            }

            light.IsOn = isOn;

            SaveLights();
            return light;
        }

        [KernelFunction("add_light")]
        [Description("Adds a new light to the system.")]
        public LightModel AddLight(string name, bool isOn)
        {
            int newId = _lights.Any() ? _lights.Max(l => l.Id) + 1 : 1;
            var newLight = new LightModel { Id = newId, Name = name, IsOn = isOn };
            
            _lights.Add(newLight);

            SaveLights();
            return newLight;
        }

        [KernelFunction("remove_light")]
        [Description("Removes a light by id")]
        public bool RemoveLight(int id)
        {
            var light = _lights.FirstOrDefault(l => l.Id == id);
            if (light == null)
            {
                return false;
            }

            _lights.Remove(light);

            SaveLights();
            return true;
        }

        private void SaveLights()
        {
            var json = JsonSerializer.Serialize(
                _lights,
                new JsonSerializerOptions { WriteIndented = true }
            );

            var tempPath = _filePath + ".tmp";
            const int maxRetries = 50;
            const int delayMs = 100; // 5 seconds total wait

            // 1) Write new content to temp file (no locking issues here)
            File.WriteAllText(tempPath, json);

            // 2) Try to replace original file when it becomes available
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Try to open exclusively just to test the lock
                    using (var fs = new FileStream(
                        _filePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None))
                    {
                        // Close immediately — we only needed the lock test
                    }

                    // Now replace the file atomically
                    File.Copy(tempPath, _filePath, overwrite: true);
                    File.Delete(tempPath);
                    return; // Success
                }
                catch (IOException)
                {
                    if (attempt == maxRetries - 1)
                    {
                        // Cleanup temp file if we give up
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);

                        throw; // or log + swallow if you prefer
                    }

                    Thread.Sleep(delayMs);
                }
            }
        }
    }
}

/*
history.AddSystemMessage(@"
Je bent een behulpzame slimme thuisassistent die de lampen in mijn huis kan bedienen.
Gebruik voor de namen van de lampen altijd een waarde uit deze lijst:
""woonkamer"", ""eetkamer"", ""keuken"", ""toilet"", ""slaapkamer 1"",
""slaapkamer 2"", ""studeerkamer"", ""badkamer 1"", ""badkamer 2"",
""badkamer 3"", ""veranda"", ""terras"", ""tuin"".
Geen andere namen en geen dubbels!
");
*/
