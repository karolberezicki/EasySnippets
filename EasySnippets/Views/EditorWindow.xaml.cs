using System.Windows;
using EasySnippets.ViewModels;
using MahApps.Metro.Controls;

namespace EasySnippets.Views
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : MetroWindow
    {
        public Snippet Snippet { get; set; }
        public bool IsEdit { get; set; }
        public bool IsSetToDelete { get; private set; }

        public EditorWindow(Snippet snippet, bool isEdit)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Snippet = snippet;
            SnippetNameTextBox.Text = snippet.Name;
            SnippetValueTextBox.Text = snippet.Value;
            IsEdit = isEdit;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SnippetNameTextBox.Text) || string.IsNullOrWhiteSpace(SnippetValueTextBox.Text))
            {
                return;
            }

            Snippet.Name = SnippetNameTextBox.Text;
            Snippet.Value = SnippetValueTextBox.Text;

            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            IsSetToDelete = true;
            DialogResult = true;
        }
    }
}
