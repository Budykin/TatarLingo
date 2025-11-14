using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TatarLingo.Views
{
    public partial class TestResultDialog : Window
    {
        public TestResultDialog(string result)
        {
            InitializeComponent();
            ResultTextBlock.Text = result;
        }

        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}