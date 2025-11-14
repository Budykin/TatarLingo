using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TatarLingo.Models;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для отображения лекционного материала
    /// </summary>
    public partial class LectureViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;
        
        /// <summary>
        /// Коллекция пар слов для упражнений на сопоставление
        /// </summary>
        public IEnumerable<MatchingPair> wordPairs { get; set; }

        /// <summary>
        /// Заголовок текущего модуля обучения
        /// </summary>
        [ObservableProperty]
        private string moduleTitle = "Загрузка...";

        /// <summary>
        /// Содержимое лекции в формате Markdown
        /// </summary>
        [ObservableProperty]
        private string lectureContent = "Пожалуйста, подождите, контент загружается...";

        /// <summary>
        /// Идентификатор текущей темы
        /// </summary>
        public string topicID;
        
        /// <summary>
        /// Инициализирует новый экземпляр LectureViewModel
        /// </summary>
        /// <param name="topic">Тема лекции</param>
        /// <param name="mainWindowViewModel">Главная ViewModel приложения</param>
        public LectureViewModel(Topic topic, MainWindowViewModel? mainWindowViewModel = null)
        {
            _main = mainWindowViewModel;
            ModuleTitle = topic.Title;
            topicID = topic.Id;
            
            // Асинхронная загрузка контента
            _ = Task.Run(() => LoadContentFromEmbeddedResourceAsync(topic.MarkdownResourceName));
        }

        /// <summary>
        /// Команда перехода к упражнениям по текущей теме
        /// </summary>
        [RelayCommand]
        private void Next()
        {
            Console.WriteLine($"Кнопка 'Далее' нажата для темы: {ModuleTitle}");
            var authService = new AuthService();
            
            // Выбор соответствующего набора пар слов в зависимости от темы
            switch (topicID)
            {
                case "alphabet":
                    wordPairs = authService.GetWordPairs("AlphabetMatch");
                    break;
                case "phrases":
                    wordPairs = authService.GetWordPairs("PhrasesMatch");
                    break;
                case "numbers":
                    wordPairs = authService.GetWordPairs("NumbersMatch");
                    break;
                case "family":
                    wordPairs = authService.GetWordPairs("FamilyMatch");
                    break;
                case "food":
                    wordPairs = authService.GetWordPairs("FoodMatch");
                    break;
            }
            
            _main?.NavigateToTask(wordPairs);
        }

        /// <summary>
        /// Команда возврата к выбору тем
        /// </summary>
        [RelayCommand]
        private void NavigateBack()
        {
            _main.NavigateToLearningSwitch();
        }

        /// <summary>
        /// Загружает содержимое лекции из встроенного ресурса
        /// </summary>
        /// <param name="resourceName">Имя ресурса в сборке</param>
        private async Task LoadContentFromEmbeddedResourceAsync(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            try
            {
                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                           LectureContent = $"Ошибка: Ресурс конспекта не найден.\n" +
                                           $"Убедитесь, что файл существует и его 'Build Action' -> 'Embedded Resource'.\n" +
                                           $"Ожидаемое имя ресурса: {resourceName}";
                        });
                        return;
                    }
                    
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = await reader.ReadToEndAsync();
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            LectureContent = content;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LectureContent = $"Произошла фатальная ошибка при загрузке конспекта: {ex.Message}";
                });
            }
        }
    }
}