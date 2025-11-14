using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TatarLingo.Views
{
    public partial class FailDialog : Window
    {
        public FailDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}