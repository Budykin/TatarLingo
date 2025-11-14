using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TatarLingo.Models;
using TatarLingo.Views;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для экрана регистрации пользователя
    /// </summary>
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _main;

        // Свойства для визуального оформления полей ввода
        [ObservableProperty]
        private IBrush emailBorderColor = new SolidColorBrush(Color.Parse("#DCDCDC"));
        
        [ObservableProperty]
        private Thickness emailBorderThickness = new Thickness(0);
        
        [ObservableProperty]
        private IBrush usernameBorderColor = new SolidColorBrush(Color.Parse("#DCDCDC"));
        
        [ObservableProperty]
        private Thickness usernameBorderThickness = new Thickness(0);
        
        [ObservableProperty]
        private IBrush password1BorderColor = new SolidColorBrush(Color.Parse("#DCDCDC"));
        
        [ObservableProperty]
        private Thickness password1BorderThickness = new Thickness(0);
        
        [ObservableProperty]
        private IBrush password2BorderColor = new SolidColorBrush(Color.Parse("#DCDCDC"));
        
        [ObservableProperty]
        private Thickness password2BorderThickness = new Thickness(0);
        
        // Свойства для данных пользователя
        [ObservableProperty]
        private string email = string.Empty;
        
        [ObservableProperty]
        private string username = string.Empty;
        
        [ObservableProperty]
        private string password1 = string.Empty;

        [ObservableProperty]
        private string password2 = string.Empty;

        /// <summary>
        /// Конструктор ViewModel регистрации
        /// </summary>
        /// <param name="main">Главная ViewModel приложения</param>
        public RegisterViewModel(MainWindowViewModel main)
        {
            _main = main;
        }

        /// <summary>
        /// Команда регистрации нового пользователя
        /// </summary>
        [RelayCommand]
        private void Registrate()
        {
            // Нормализация введенных данных
            Username = Username.Trim();
            Password1 = Password1.Trim();
            Password2 = Password2.Trim();
            Email = Email.Trim();
            
            var registerService = new AuthService();
            bool canRegister = true;
            
            Debug.WriteLine($"email = {Email}");
            
            // Валидация email
            if (!EmailCheck())
            {
                canRegister = false;
                EmailBorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                EmailBorderThickness = new Thickness(2);
            }

            // Валидация имени пользователя
            if (Username == string.Empty)
            {
                canRegister = false;
                UsernameBorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                UsernameBorderThickness = new Thickness(2);
            }

            // Валидация пароля
            if (!PasswordCheck())
            {
                canRegister = false;
                Password1BorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                Password1BorderThickness = new Thickness(2);
            }

            // Проверка подтверждения пароля
            if (Password2 == string.Empty)
            {
                canRegister = false;
                Password2BorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                Password2BorderThickness = new Thickness(2);
            }

            // Проверка совпадения паролей
            if (Password1 != Password2)
            {
                canRegister = false;
                Password1BorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                Password1BorderThickness = new Thickness(2);
                Password2BorderColor = new SolidColorBrush(Color.Parse("#E30016"));
                Password2BorderThickness = new Thickness(2);
            }

            // Если все проверки пройдены
            if (canRegister)
            {
                if (registerService.RegisterUser(Username, Password1, Email))
                {
                    var newWindow = new SuccessDialog();
                    newWindow.Show();
                }
                else
                {
                    var newWindow = new FailDialog();
                    newWindow.Show();
                    _main.NavigateToLogin();
                }
            }
        }

        /// <summary>
        /// Проверка валидности email
        /// </summary>
        /// <returns>True если email валиден, иначе False</returns>
        private bool EmailCheck()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
                return addr.Address == Email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка сложности пароля
        /// </summary>
        /// <returns>True если пароль соответствует требованиям, иначе False</returns>
        private bool PasswordCheck()
        {
            if (string.IsNullOrWhiteSpace(Password1))
                return false;

            // Минимальная длина пароля
            if (Password1.Length < 8)
                return false;

            // Проверка наличия символов разного типа
            bool hasUpper = Password1.Any(char.IsUpper);
            bool hasLower = Password1.Any(char.IsLower);
            bool hasDigit = Password1.Any(char.IsDigit);
            bool hasSpecial = Password1.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Команда перехода к экрану входа
        /// </summary>
        [RelayCommand]
        private void GoToLogin()
        {
            _main.NavigateToLogin();
        }
    }
}