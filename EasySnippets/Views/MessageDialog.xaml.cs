using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace EasySnippets.Views
{
    /// <summary>
    /// Interaction logic for ClosingDialog.xaml
    /// </summary>
    public partial class MessageDialog : ChildWindow
    {
        public string MessageBoxText { get; set; }
        public string Caption { get; set; }

        public MessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        //public MessageDialog(string messageBoxText, string caption)
        //{
        //    MessageBoxText = messageBoxText;
        //    Caption = caption;
        //}

        private void DialogDismiss(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
