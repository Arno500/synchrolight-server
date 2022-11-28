using LightServer.Server;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace LightServer.Managers
{
    class SettingsManager
    {
        public class Settings : INotifyPropertyChanged, ICloneable
        {
            private bool artNetEnabled = true;
            public bool ArtNetEnabled
            {
                get => artNetEnabled;
                set => SetProperty(ref artNetEnabled, value);
            }
            private UInt16 dMXChannelR = 1;
            public UInt16 DMXChannelR
            {
                get => dMXChannelR;
                set => SetProperty(ref dMXChannelR, value);
            }
            private UInt16 dMXChannelG = 2;
            public UInt16 DMXChannelG
            {
                get => dMXChannelG;
                set => SetProperty(ref dMXChannelG, value);
            }
            private UInt16 dMXChannelB = 3;
            public UInt16 DMXChannelB
            {
                get => dMXChannelB;
                set => SetProperty(ref dMXChannelB, value);
            }
            private int dMXUniverse = 1;
            public int DMXUniverse
            {
                get => dMXUniverse;
                set => SetProperty(ref dMXUniverse, value);
            }


            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            protected bool SetProperty<T>(ref T storage, T value,
            [CallerMemberName] String propertyName = null)
            {
                if (object.Equals(storage, value)) return false;
                storage = value;
                if (!initialized) return true;
                OnPropertyChanged(propertyName);
                SettingsManager.Save();
                return true;
            }

            public object Clone()
            {
                return (Settings)MemberwiseClone();
            }
        }

        public static Settings settings = new Settings();
        private static Settings lastSavedSettings;

        private static string filePath = "";
        private static Mutex fileMutex = new Mutex();

        private static bool initialized = false;

        public SettingsManager()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\SynchroLight";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            filePath = folderPath + "\\settings.json";
            Load();
        }

        public static async void Load()
        {
            if (!File.Exists(filePath)) return;
            fileMutex.WaitOne(1000);
            using var fileReader = File.OpenRead(filePath);
            try
            {
                settings = await JsonSerializer.DeserializeAsync<Settings>(fileReader);
            }
            catch { }
            lastSavedSettings = (Settings)settings.Clone();
            fileMutex.ReleaseMutex();
            initialized = true;
        }

        public static async void Save()
        {
            fileMutex.WaitOne(1000);
            if (settings.ArtNetEnabled && !lastSavedSettings.ArtNetEnabled)
            {
                new ArtNetServer();
            }
            else if (!settings.ArtNetEnabled && lastSavedSettings.ArtNetEnabled)
            {
                ArtNetServer.Dispose();
            }
            FileStream fileWriter;
            fileWriter = File.OpenWrite(filePath);
            fileWriter.SetLength(0);
            await JsonSerializer.SerializeAsync(fileWriter, settings);
            fileWriter.Close();
            await fileWriter.DisposeAsync();
            lastSavedSettings = (Settings)settings.Clone();
            fileMutex.ReleaseMutex();
        }
    }
}
