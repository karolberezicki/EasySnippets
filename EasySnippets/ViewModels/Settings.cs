using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace EasySnippets.ViewModels;

public sealed class Settings : ViewModelBase
{
    public static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    public bool AutoStartEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool AlwaysOnTopEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool AutoSaveEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string CurrentFilePath
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int Height
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int Width
    {
        get;
        set => SetProperty(ref field, value);
    }

    private new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
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
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    public static Settings LoadSettings()
    {
        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Settings>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }
}