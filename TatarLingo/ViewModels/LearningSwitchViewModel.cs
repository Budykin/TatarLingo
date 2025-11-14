using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TatarLingo.Models;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для экрана выбора тем обучения.
    /// Позволяет пользователю выбирать темы для изучения и переходить к финальному тесту.
    /// </summary>
    public partial class LearningSwitchViewModel : ObservableObject
    {
        /// <summary>
        /// Коллекция пар слов для упражнений на сопоставление.
        /// </summary>
        public IEnumerable<MatchingPair> wordPairs { get; set; }
        
        private readonly MainWindowViewModel _main;
        
        /// <summary>
        /// Коллекция доступных тем для изучения.
        /// </summary>
        public ObservableCollection<Topic> AvailableTopics { get; }
        
        private User _user = UserSession.CurrentUser!;
        
        /// <summary>
        /// Инициализирует новый экземпляр LearningSwitchViewModel.
        /// </summary>
        /// <param name="main">Главная ViewModel приложения</param>
        public LearningSwitchViewModel(MainWindowViewModel main)
        {
            _main = main;
            
            // Инициализация списка доступных тем
            AvailableTopics = new ObservableCollection<Topic>
            {
                new Topic
                {
                    Id = "alphabet",
                    Title = "1. Алфавит и произношение",
                    MarkdownResourceName = "TatarLingo.Assets.Lectures.Alphabet.md"
                },
                new Topic
                {
                    Id = "phrases",
                    Title = "2. Простые фразы и приветствия",
                    MarkdownResourceName = "TatarLingo.Assets.Lectures.Phrases.md"
                },
                new Topic
                {
                    Id = "numbers",
                    Title = "3. Числа и счёт",
                    MarkdownResourceName = "TatarLingo.Assets.Lectures.Numbers.md"
                },
                new Topic
                {
                    Id = "family",
                    Title = "4. Семья и люди",
                    MarkdownResourceName = "TatarLingo.Assets.Lectures.Family.md"
                },
                new Topic
                {
                    Id = "food",
                    Title = "5. Еда и покупки",
                    MarkdownResourceName = "TatarLingo.Assets.Lectures.Food.md"
                },
            };
        }

        /// <summary>
        /// Команда выбора темы для изучения.
        /// Устанавливает выбранную тему как текущую и переходит к лекции.
        /// </summary>
        /// <param name="selectedTopic">Выбранная тема</param>
        [RelayCommand]
        private void SelectTopic(Topic? selectedTopic)
        {
            if (selectedTopic == null) return;
            _user.CurrentTopic = selectedTopic;
            _main.NavigateToLecture(selectedTopic);
        }

        /// <summary>
        /// Команда возврата к экрану входа в систему.
        /// </summary>
        [RelayCommand]
        public void NavigateBack()
        {
            _main.NavigateToLogin();
        }

        /// <summary>
        /// Команда перехода к финальному тесту.
        /// Загружает задания теста и инициализирует тестовую сессию.
        /// </summary>
        [RelayCommand]
        private void NavigateToFinalTest()
        {
            var authService = new AuthService();
            var finalTasks = authService.GetFinalTestTasks();

            UserSession.InitializeFinalTest(finalTasks);
            _main.NavigateToFinalTest();
        }
    }
}