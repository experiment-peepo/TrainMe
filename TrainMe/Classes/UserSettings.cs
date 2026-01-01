using System;
using System.IO;
using System.Text.Json;

namespace TrainMe.Classes {
    public class UserSettings {
        public double Opacity { get; set; } = 0.2;
        public double Volume { get; set; } = 0.5;
        public bool AutoLoadSession { get; set; } = false;
        public bool PreventOverlayMinimize { get; set; } = false;
        public double DefaultOpacity { get; set; } = 0.9;
        public double DefaultVolume { get; set; } = 0.5;

        private static readonly string SettingsFile = "settings.json";

        public static UserSettings Load() {
            try {
                if (File.Exists(SettingsFile)) {
                    string json = File.ReadAllText(SettingsFile);
                    return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            } catch (Exception ex) {
                Logger.Warning("Failed to load settings, using defaults", ex);
            }
            return new UserSettings();
        }

        public void Save() {
            try {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            } catch (Exception ex) {
                Logger.Error("Failed to save settings", ex);
            }
        }
    }
}
