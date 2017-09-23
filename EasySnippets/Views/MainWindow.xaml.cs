using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySnippets.ViewModels;

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

            SnippetsList = new ObservableCollection<Snippet>
            {
                new Snippet {Name = "Name1", Value = "Value1"},
                new Snippet {Name = "Name2", Value = "Value2"},
                new Snippet {Name = "Name3", Value = "  - if the current character is a digit (\'0\'-\'9\'), the machineValue2Value3Value1Value2 * - if the current character is a digit (\'0\'-\'9\'), the machine\r\n * pushes the value of that digit onto its stack;<br/>\r\n * - if the current character is \'+\', the machine pops the two\r\n * topmost values from its stack, adds them and pushes the result\r\n * onto the stack;<br/>\r\n * - if the current character is \'*\', the machine pops the two\r\n * topmost values from its stack, multiplies them and pushes the\r\n * result onto the stack;<br/>\r\n * - after the machine has processed the whole string it returns the\r\n * topmost value of its stack as the result;<br/>"}
            };

            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            #if !DEBUG
            e.Cancel = MessageBoxCentered.Show(this, "Are you sure?", "Exit",
                           MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No;
            #endif
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {

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
            EditorWindow editorWindow = new EditorWindow(new Snippet());

            if (editorWindow.ShowDialog() == true)
            {
                SnippetsList.Add(editorWindow.Snippet);
            }
        }

        private void SnippetsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int rowIndex = SnippetsDataGrid.SelectedIndex;

            Snippet snippet = SnippetsList[rowIndex];
            Console.WriteLine($@"Right clicked snippet {rowIndex} = {snippet.Name} ");

            EditorWindow editorWindow = new EditorWindow(snippet);

            if (editorWindow.ShowDialog() != true)
            {
                return;
            }

            SnippetsList[rowIndex].Name = editorWindow.Snippet.Name;
            SnippetsList[rowIndex].Value = editorWindow.Snippet.Value;
            Clipboard.SetText($"{editorWindow.Snippet.Value}");
        }
    }
}
