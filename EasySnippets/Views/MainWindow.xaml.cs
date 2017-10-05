using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySnippets.Utils;
using EasySnippets.ViewModels;
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

        public MainWindow()
        {

            SnippetsList = new ObservableCollection<Snippet>();

            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClose())
            {
                return;
            }

            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = CancelClose();
        }

        private bool CancelClose()
        {
            return MessageBoxCentered.Show(this, "Are you sure?", "Exit",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No;
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "JSON Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                List<Snippet> deserializedSnippets = JsonConvert.DeserializeObject<List<Snippet>>(json);
                SnippetsList.Clear();
                deserializedSnippets.ForEach(snippet => SnippetsList.Add(snippet));
            }
            catch (Exception)
            {
                MessageBoxCentered.Show(this, "Couldn't read file.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(SnippetsList, Formatting.Indented);

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "JSON Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void AlwaysOnTopToggle(object sender, RoutedEventArgs e)
        {
            Topmost = ((MenuItem) sender).IsChecked;
        }

        private void SnippetsDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnippetsDataGrid.SelectedItem is Snippet snippet){
                Clipboard.SetText($"{snippet.Value}");
            }
        }

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            EditorWindow editorWindow = new EditorWindow(new Snippet(), false);

            if (editorWindow.ShowDialog() == true)
            {
                SnippetsList.Add(editorWindow.Snippet);
            }
        }

        private void EditSnippet(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = SnippetsDataGrid.SelectedIndex;

            Snippet snippet = rowIndex < SnippetsList.Count ? SnippetsList[rowIndex] : new Snippet();
            bool isEdit = rowIndex < SnippetsList.Count;

            EditorWindow editorWindow = new EditorWindow(snippet, isEdit);

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

            Clipboard.SetText($"{editorWindow.Snippet.Value}");
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            SnippetsDataGrid.UnselectAll();
        }
    }
}
