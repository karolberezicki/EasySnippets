using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace EasySnippets.Views
{
    /// <summary>
    /// Interaction logic for ClosingDialog.xaml
    /// </summary>
    public partial class ClosingDialog : ChildWindow
    {
        public ClosingDialog()
        {
            InitializeComponent();
        }

        private void ApplicationShutdown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DialogDismiss(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
