using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TatarLingo.Models;
using TatarLingo.Views;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для экрана входа в систему
    /// </summary>
    public partial class LoginViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _main;

        /// <summary>
        /// Имя пользователя для входа
        /// </summary>
        [ObservableProperty]
        private string username = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [ObservableProperty]
        private string password = string.Empty;

        /// <summary>
        /// Инициализирует новый экземпляр LoginViewModel
        /// </summary>
        /// <param name="main">Главная ViewModel приложения</param>
        public LoginViewModel(MainWindowViewModel main)
        {
            _main = main;
        }

        /// <summary>
        /// Команда перехода к экрану регистрации
        /// </summary>
        [RelayCommand]
        private void GoToRegister()
        {
            _main.NavigateToRegister();
        }
        
        /// <summary>
        /// Команда выполнения входа в систему
        /// Проверяет учетные данные и либо переходит к обучению,
        /// либо показывает сообщение об ошибке
        /// </summary>
        [RelayCommand]
        public void GoToLearningSwitch()
        {
            var authService = new AuthService();
            
            if (authService.CheckCredentials(Username.Trim(), Password.Trim()))
            {
                _main.NavigateToLearningSwitch();
            }
            else
            {
                var newWindow = new FailLoginDialog();
                newWindow.Show();
            }
        }
    }
}