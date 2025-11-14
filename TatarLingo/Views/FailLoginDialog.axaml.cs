using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TatarLingo.Views
{
    public partial class FailLoginDialog : Window
    {
        public FailLoginDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}