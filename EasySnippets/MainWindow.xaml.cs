using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace EasySnippets
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Snippet> SnippetsList { get; set; }

        public MainWindow()
        {

            SnippetsList = new ObservableCollection<Snippet>
            {
                new Snippet {Name = "Name1", Value = "Value1"},
                new Snippet {Name = "Name2", Value = "Value2"},
                new Snippet {Name = "Name3", Value = "Value3"}
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

        private void SnippetsDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null || dep is DataGridColumnHeader)
            {
                return;
            }

            DataGridCell cell = dep as DataGridCell;

            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                return;
            }

            DataGridRow row = dep as DataGridRow;

            int columnIndex = cell.Column.DisplayIndex;
            int rowIndex = FindRowIndex(row);

            if (rowIndex == -1 || rowIndex > SnippetsList.Count - 1)
            {
                return;
            }

            Snippet snippet = SnippetsList[rowIndex];
            Clipboard.SetText($"{snippet.Value}");
        }

        private static int FindRowIndex(DependencyObject row)
        {
            if (!(ItemsControl.ItemsControlFromItemContainer(row) is DataGrid dataGrid))
            {
                return -1;
            }

            return dataGrid.ItemContainerGenerator.IndexFromContainer(row);
        }

        private void AlwaysOnTopToggle(object sender, RoutedEventArgs e)
        {
            Topmost = ((MenuItem) sender).IsChecked;
        }
    }
}
