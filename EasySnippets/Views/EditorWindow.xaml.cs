using System.Windows;
using EasySnippets.ViewModels;

namespace EasySnippets.Views
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow
    {
        public Snippet Snippet { get; set; }
        public int SnippetIndex { get; set; }


        public EditorWindow(Snippet snippet)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Snippet = snippet;
            SnippetNameTextBox.Text = snippet.Name;
            SnippetValueTextBox.Text = snippet.Value;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Snippet.Name = SnippetNameTextBox.Text;
            Snippet.Value = SnippetValueTextBox.Text;

            DialogResult = true;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
