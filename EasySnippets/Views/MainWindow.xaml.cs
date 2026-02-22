using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage.Pickers;
using EasySnippets.Utils;
using EasySnippets.ViewModels;
using WinRT.Interop;

namespace EasySnippets.Views;

public sealed partial class MainWindow : Window
{
    public ObservableCollection<Snippet> SnippetsList { get; set; }
    public Settings AppSettings { get; set; }

    private readonly DispatcherTimer _resizeTimer;
    private bool _isExiting;
    private bool _isLoadingFile;
    private AppWindow _appWindow;
    private OverlappedPresenter _presenter;

    public MainWindow()
    {
        SnippetsList = new ObservableCollection<Snippet>();
        this.InitializeComponent();

        SnippetsListView.ItemsSource = SnippetsList;

        _appWindow = GetAppWindow();
        _presenter = _appWindow.Presenter as OverlappedPresenter;

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "es.ico");
        if (File.Exists(iconPath))
        {
            _appWindow.SetIcon(iconPath);
        }

        _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _resizeTimer.Tick += ResizeTimer_Tick;

        _appWindow.Changed += AppWindow_Changed;
        _appWindow.Closing += AppWindow_Closing;
        this.Activated += MainWindow_Activated;

        LoadSettings();

        SnippetsList.CollectionChanged += (_, _) => { TriggerAutoSave(); };
    }

    private AppWindow GetAppWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    private IntPtr GetHwnd()
    {
        return WindowNative.GetWindowHandle(this);
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidSizeChange)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }
    }

    private void ResizeTimer_Tick(object sender, object e)
    {
        _resizeTimer.Stop();
        if (AppSettings != null)
        {
            var size = _appWindow.Size;
            AppSettings.Height = size.Height;
            AppSettings.Width = size.Width;
        }
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (_isExiting)
            return;

        args.Cancel = true;
        await ShowExitConfirmation();
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            SnippetsListView.SelectedItem = null;
        }
    }

    private async void Exit_Click(object sender, RoutedEventArgs e)
    {
        await ShowExitConfirmation();
    }

    private async System.Threading.Tasks.Task ShowExitConfirmation()
    {
        var dialog = new ContentDialog
        {
            Title = "Exit",
            Content = "Are you sure you want to exit?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _isExiting = true;
            this.Close();
        }
    }

    private async void MenuOpen_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, GetHwnd());
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");
        picker.FileTypeFilter.Add("*");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await LoadFile(file.Path);
        }
    }

    private async System.Threading.Tasks.Task LoadFile(string path, bool initialLoad = false)
    {
        try
        {
            _isLoadingFile = true;
            var json = await File.ReadAllTextAsync(path);
            var deserializedSnippets = JsonSerializer.Deserialize<List<Snippet>>(json);
            SnippetsList.Clear();
            if (deserializedSnippets?.Any() ?? false)
            {
                foreach (var snippet in deserializedSnippets)
                {
                    SnippetsList.Add(snippet);
                }
            }
            AppSettings.CurrentFilePath = path;
        }
        catch (Exception)
        {
            if (!initialLoad)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Couldn't read file.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }

            AppSettings.CurrentFilePath = null;
        }
        finally
        {
            _isLoadingFile = false;
        }
    }

    private async void MenuSaveAs_Click(object sender, RoutedEventArgs e)
    {
        await SaveFileUsingFileDialog();
    }

    private async System.Threading.Tasks.Task SaveFileUsingFileDialog()
    {
        var picker = new FileSavePicker();
        InitializeWithWindow.Initialize(picker, GetHwnd());
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("JSON Files", new List<string> { ".json" });
        picker.SuggestedFileName = "snippets";

        var file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            SaveFile(file.Path);
            AppSettings.CurrentFilePath = file.Path;
        }
    }

    private void SaveFile(string path)
    {
        var json = JsonSerializer.Serialize(SnippetsList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private void MenuSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSnippets();
    }

    private async void SaveSnippets()
    {
        if (!string.IsNullOrWhiteSpace(AppSettings.CurrentFilePath))
        {
            SaveFile(AppSettings.CurrentFilePath);
        }
        else
        {
            await SaveFileUsingFileDialog();
        }
    }

    private void AlwaysOnTopToggle(object sender, RoutedEventArgs e)
    {
        var isChecked = ((ToggleMenuFlyoutItem)sender).IsChecked;
        _presenter.IsAlwaysOnTop = isChecked;
        AppSettings.AlwaysOnTopEnabled = isChecked;
    }

    private void AutoStartToggle(object sender, RoutedEventArgs e)
    {
        ApplyAutoStart(((ToggleMenuFlyoutItem)sender).IsChecked);
    }

    private void ApplyAutoStart(bool enabled)
    {
        AppSettings.AutoStartEnabled = enabled;

        if (enabled)
        {
            StartUpManager.AddApplicationToCurrentUserStartup();
        }
        else
        {
            StartUpManager.RemoveApplicationFromCurrentUserStartup();
        }
    }

    private void AutoSaveToggle(object sender, RoutedEventArgs e)
    {
        AppSettings.AutoSaveEnabled = ((ToggleMenuFlyoutItem)sender).IsChecked;
        TriggerAutoSave();
    }

    private void SnippetsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Snippet snippet)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(snippet.Value ?? string.Empty);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }

    private async void AddNewClick(object sender, RoutedEventArgs e)
    {
        var editorWindow = new EditorWindow(new Snippet(), false)
        {
            XamlRoot = this.Content.XamlRoot
        };

        var result = await editorWindow.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            SnippetsList.Add(editorWindow.Snippet);
        }
    }

    private async void SnippetsListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var selectedIndex = SnippetsListView.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= SnippetsList.Count)
            return;

        var snippet = SnippetsList[selectedIndex];
        var editorWindow = new EditorWindow(new Snippet { Name = snippet.Name, Value = snippet.Value }, true)
        {
            XamlRoot = this.Content.XamlRoot
        };

        var result = await editorWindow.ShowAsync();

        if (editorWindow.IsSetToDelete)
        {
            SnippetsList.RemoveAt(selectedIndex);
            Clipboard.SetContent(null);
            Clipboard.Flush();
            return;
        }

        if (result == ContentDialogResult.Primary)
        {
            SnippetsList[selectedIndex].Name = editorWindow.Snippet.Name;
            SnippetsList[selectedIndex].Value = editorWindow.Snippet.Value;

            var dataPackage = new DataPackage();
            dataPackage.SetText(editorWindow.Snippet.Value ?? string.Empty);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }

    private void TriggerAutoSave()
    {
        if (_isLoadingFile)
            return;

        if (AppSettings?.AutoSaveEnabled == true)
        {
            SaveSnippets();
        }
    }

    private void LoadSettings()
    {
        AppSettings = Settings.LoadSettings();

        AppSettings ??= new Settings
        {
            AlwaysOnTopEnabled = false,
            AutoSaveEnabled = false,
            AutoStartEnabled = StartUpManager.IsApplicationAddedToCurrentUserStartup(),
            CurrentFilePath = null
        };

        AppSettings.Height = AppSettings.Height > 0 ? AppSettings.Height : 300;
        AppSettings.Width = AppSettings.Width > 0 ? AppSettings.Width : 220;

        _appWindow.Resize(new SizeInt32(AppSettings.Width, AppSettings.Height));
        CenterOnScreen();

        _presenter.IsAlwaysOnTop = AppSettings.AlwaysOnTopEnabled;
        AlwaysOnTopMenuItem.IsChecked = AppSettings.AlwaysOnTopEnabled;

        if (!string.IsNullOrWhiteSpace(AppSettings.CurrentFilePath))
        {
            _ = LoadFile(AppSettings.CurrentFilePath, true);
        }

        AutoStartMenuItem.IsChecked = AppSettings.AutoStartEnabled;
        AutoSaveMenuItem.IsChecked = AppSettings.AutoSaveEnabled;

        ApplyAutoStart(AppSettings.AutoStartEnabled);
    }

    private void CenterOnScreen()
    {
        var hwnd = GetHwnd();
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            var workArea = displayArea.WorkArea;
            var size = _appWindow.Size;
            var x = (workArea.Width - size.Width) / 2 + workArea.X;
            var y = (workArea.Height - size.Height) / 2 + workArea.Y;
            _appWindow.Move(new PointInt32(x, y));
        }
    }
}