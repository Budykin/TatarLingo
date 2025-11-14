using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TatarLingo.Views
{
    public partial class SuccessDialog : Window
    {
        public SuccessDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}