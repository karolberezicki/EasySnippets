using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EasySnippets.ViewModels
{
    public class Settings : ViewModelBase
    {
        public const string SettingsPath = @".\settings.json";

        private bool _autoStartEnabled;

        public bool AutoStartEnabled
        {
            get => _autoStartEnabled;
            set => SetProperty(ref _autoStartEnabled, value);
        }

        private bool _alwaysOnTopEnabled;

        public bool AlwaysOnTopEnabled
        {
            get => _alwaysOnTopEnabled;
            set => SetProperty(ref _alwaysOnTopEnabled, value);
        }

        private bool _autoSaveEnabled;

        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set => SetProperty(ref _autoSaveEnabled, value);
        }

        private string _currentFilePath;

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        private int _height;

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _width;

        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        protected new virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            SaveSettings();
            return true;
        }

        private void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
    }
}