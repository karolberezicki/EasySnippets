using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using EasySnippets.ViewModels;

namespace EasySnippets.Views;

public sealed partial class EditorWindow : ContentDialog
{
    public Snippet Snippet { get; set; }
    public bool IsEdit { get; set; }
    public bool IsSetToDelete { get; private set; }

    public EditorWindow(Snippet snippet, bool isEdit)
    {
        this.InitializeComponent();
        Snippet = snippet;
        IsEdit = isEdit;
        SnippetNameTextBox.Text = snippet.Name ?? string.Empty;
        SnippetValueTextBox.Text = snippet.Value ?? string.Empty;

        if (isEdit)
        {
            DeleteButton.Visibility = Visibility.Visible;
            DeleteButton.Foreground = new SolidColorBrush(Colors.White);
            DeleteButton.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 196, 43, 28));
        }
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(SnippetNameTextBox.Text) || string.IsNullOrWhiteSpace(SnippetValueTextBox.Text))
        {
            args.Cancel = true;
            return;
        }

        Snippet.Name = SnippetNameTextBox.Text;
        Snippet.Value = SnippetValueTextBox.Text;
    }

    private void DeleteClick(object sender, RoutedEventArgs e)
    {
        IsSetToDelete = true;
        this.Hide();
    }
}