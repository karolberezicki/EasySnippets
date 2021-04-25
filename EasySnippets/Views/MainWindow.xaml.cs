using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySnippets.Utils;
using EasySnippets.ViewModels;
using MahApps.Metro.SimpleChildWindow;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace EasySnippets.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<Snippet> SnippetsList { get; set; }
        public Settings AppSettings { get; set; }

        public MainWindow()
        {
            SnippetsList = new ObservableCollection<Snippet>();
            InitializeComponent();

            var observableSizeChanges = Observable
                .FromEventPattern<SizeChangedEventArgs>(this, "SizeChanged")
                .Select(x => x.EventArgs)
                .Throttle(TimeSpan.FromMilliseconds(300));

            var sizeChangedSubscription = observableSizeChanges
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => WindowSizeChanged());

            LoadSettings();

            SnippetsList.CollectionChanged += (_, _) => { TriggerAutoSave(); };
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            await this.ShowChildWindowAsync(new ClosingDialog());
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            await this.ShowChildWindowAsync(new ClosingDialog());
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            LoadFile(dialog.FileName);
        }

        private async void LoadFile(string path, bool initialLoad = false)
        {
            try
            {
                var json = await File.ReadAllTextAsync(path);
                var deserializedSnippets = JsonConvert.DeserializeObject<List<Snippet>>(json);
                SnippetsList.Clear();
                if (deserializedSnippets?.Any() ?? false)
                {
                    deserializedSnippets.ForEach(snippet => SnippetsList.Add(snippet));
                }
                AppSettings.CurrentFilePath = path;

                if (!initialLoad)
                {
                    AppSettings.AutoSaveEnabled = false;
                }
            }
            catch (Exception)
            {
                if (!initialLoad)
                {
                    await this.ShowChildWindowAsync(new MessageDialog {MessageBoxText = "Couldn't read file.", Caption = "Error" });
                }

                AppSettings.CurrentFilePath = null;
            }
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileUsingFileDialog();
        }

        private void SaveFileUsingFileDialog()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() == true)
            {
                SaveFile(dialog.FileName);
                AppSettings.CurrentFilePath = dialog.FileName;
            }
        }

        private void SaveFile(string path)
        {
            var json = JsonConvert.SerializeObject(SnippetsList, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private void AlwaysOnTopToggle(object sender, RoutedEventArgs e)
        {
            Topmost = ((MenuItem) sender).IsChecked;
            AppSettings.AlwaysOnTopEnabled = Topmost;
        }

        private void SnippetsDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnippetsDataGrid.SelectedItem is Snippet snippet){
                Clipboard.SetDataObject($"{snippet.Value}");
            }
        }

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            var editorWindow = new EditorWindow(new Snippet(), false);

            if (editorWindow.ShowDialog() == true)
            {
                SnippetsList.Add(editorWindow.Snippet);
            }
        }

        private void EditSnippet(object sender, MouseButtonEventArgs e)
        {
            var rowIndex = SnippetsDataGrid.SelectedIndex;

            if (rowIndex < 0)
            {
                return;
            }

            var snippet = rowIndex < SnippetsList.Count ? SnippetsList[rowIndex] : new Snippet();
            var isEdit = rowIndex < SnippetsList.Count;

            var editorWindow = new EditorWindow(snippet, isEdit);

            if (editorWindow.ShowDialog() != true)
            {
                return;
            }

            if (editorWindow.IsSetToDelete)
            {
                SnippetsList.RemoveAt(rowIndex);
                Clipboard.Clear();
                return;
            }

            if (rowIndex < SnippetsList.Count)
            {
                SnippetsList[rowIndex].Name = editorWindow.Snippet.Name;
                SnippetsList[rowIndex].Value = editorWindow.Snippet.Value;
            }
            else
            {
                SnippetsList.Add(editorWindow.Snippet);
            }

            Clipboard.SetDataObject($"{editorWindow.Snippet.Value}");
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            SnippetsDataGrid.UnselectAll();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSnippets();
        }

        private void SaveSnippets()
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.CurrentFilePath))
            {
                SaveFile(AppSettings.CurrentFilePath);
            }
            else
            {
                SaveFileUsingFileDialog();
            }
        }

        private void LoadSettings()
        {

            try
            {
                var json = File.ReadAllText(Settings.SettingsPath);
                AppSettings = JsonConvert.DeserializeObject<Settings>(json);
            }
            catch (Exception)
            {
                AppSettings = null;
            }

            AppSettings ??= new Settings
            {
                AlwaysOnTopEnabled = false,
                AutoSaveEnabled = false,
                AutoStartEnabled = StartUpManager.IsApplicationAddedToCurrentUserStartup(),
                CurrentFilePath = null
            };

            AppSettings.Height = AppSettings.Height > 0 ? AppSettings.Height : 300;
            AppSettings.Width = AppSettings.Width > 0 ? AppSettings.Width : 220;

            Height = AppSettings.Height;
            Width = AppSettings.Width;

            Topmost = AppSettings.AlwaysOnTopEnabled;
            AlwaysOnTopMenuItem.IsChecked = AppSettings.AlwaysOnTopEnabled;

            if (!string.IsNullOrWhiteSpace(AppSettings.CurrentFilePath))
            {
                LoadFile(AppSettings.CurrentFilePath, true);
            }

            AutoStartMenuItem.IsChecked = AppSettings.AutoStartEnabled;
            AutoSaveMenuItem.IsChecked = AppSettings.AutoSaveEnabled;

            ApplyAutoStart(AppSettings.AutoStartEnabled);
        }

        private void ApplyAutoStart(bool enabled)
        {
            AppSettings.AutoStartEnabled = enabled;

            if (AppSettings.AutoStartEnabled)
            {
                StartUpManager.AddApplicationToCurrentUserStartup();
            }
            else
            {
                StartUpManager.RemoveApplicationFromCurrentUserStartup();
            }
        }

        private void AutoStartToggle(object sender, RoutedEventArgs e)
        {
            ApplyAutoStart(((MenuItem)sender).IsChecked);
        }

        private void WindowSizeChanged()
        {
            AppSettings.Height = Convert.ToInt32(Height);
            AppSettings.Width = Convert.ToInt32(Width);
        }

        private void AutoSaveToggle(object sender, RoutedEventArgs e)
        {
            AppSettings.AutoSaveEnabled = ((MenuItem)sender).IsChecked;
            TriggerAutoSave();
        }

        private void TriggerAutoSave()
        {
            if (AppSettings.AutoSaveEnabled)
            {
                SaveSnippets();
            }
        }
    }
}
