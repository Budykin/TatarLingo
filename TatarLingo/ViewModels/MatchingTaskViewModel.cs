using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using TatarLingo.Models;

namespace TatarLingo.Models
{
    /// <summary>
    /// Модель данных для пары слов (слово и его перевод)
    /// </summary>
    /// <param name="SourceWord">Исходное слово</param>
    /// <param name="TargetWord">Перевод слова</param>
    public record MatchingPair(string SourceWord, string TargetWord);
}

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для элемента сопоставления в упражнении
    /// </summary>
    public partial class MatchItemViewModel : ObservableObject
    {
        /// <summary>
        /// Уникальный идентификатор элемента
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Текст элемента (слово или перевод)
        /// </summary>
        [ObservableProperty]
        private string _text;

        /// <summary>
        /// Флаг, указывающий что элемент уже сопоставлен
        /// </summary>
        [ObservableProperty]
        private bool _isMatched;

        /// <summary>
        /// Флаг временной подсветки при неправильном выборе
        /// </summary>
        [ObservableProperty]
        private bool _isInvalidMatch;

        /// <summary>
        /// Конструктор элемента сопоставления
        /// </summary>
        /// <param name="id">Уникальный идентификатор</param>
        /// <param name="text">Текст элемента</param>
        public MatchItemViewModel(int id, string text)
        {
            Id = id;
            Text = text;
        }
    }

    /// <summary>
    /// ViewModel для упражнения на сопоставление слов
    /// </summary>
    public partial class MatchingTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;

        /// <summary>
        /// Название текущего модуля
        /// </summary>
        [ObservableProperty]
        private string _moduleTitle;

        /// <summary>
        /// Коллекция исходных слов (левая колонка)
        /// </summary>
        public ObservableCollection<MatchItemViewModel> SourceWords { get; } = new();

        /// <summary>
        /// Коллекция переводов (правая колонка)
        /// </summary>
        public ObservableCollection<MatchItemViewModel> TargetWords { get; } = new();

        /// <summary>
        /// Выбранное слово в левой колонке
        /// </summary>
        [ObservableProperty]
        private MatchItemViewModel? _selectedSourceWord;

        /// <summary>
        /// Выбранное слово в правой колонке
        /// </summary>
        [ObservableProperty]
        private MatchItemViewModel? _selectedTargetWord;

        /// <summary>
        /// Количество правильно сопоставленных пар
        /// </summary>
        [ObservableProperty]
        private int _correctlyMatchedCount;
        
        /// <summary>
        /// Флаг завершения упражнения
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted = false;

        /// <summary>
        /// Текст кнопки действия
        /// </summary>
        [ObservableProperty] 
        private string _buttonText = "Далее";
        
        private User _user = UserSession.CurrentUser!;

        /// <summary>
        /// Общее количество пар для сопоставления
        /// </summary>
        public int TotalPairsCount { get; private set; }

        /// <summary>
        /// Конструктор ViewModel упражнения
        /// </summary>
        /// <param name="wordPairs">Коллекция пар слов</param>
        /// <param name="mainWindowViewModel">Главная ViewModel приложения</param>
        public MatchingTaskViewModel(IEnumerable<Models.MatchingPair> wordPairs, MainWindowViewModel? mainWindowViewModel = null)
        {
            if (_user.CurrentTopic.Id == "alphabet" || _user.CurrentTopic.Id == "family"
                || _user.CurrentTopic.Id == "numbers") ButtonText = "Завершить";
            _moduleTitle = _user.CurrentTopic.Title;
            _main = mainWindowViewModel;
            InitializeExercise(wordPairs);
        }

        /// <summary>
        /// Инициализация упражнения
        /// </summary>
        /// <param name="wordPairs">Коллекция пар слов</param>
        private void InitializeExercise(IEnumerable<Models.MatchingPair> wordPairs)
        {
            var random = new Random();
            var pairs = wordPairs.OrderBy(x => random.Next()).Take(6).ToList();
            TotalPairsCount = pairs.Count;
            
            var targetItems = new List<MatchItemViewModel>();

            int id = 0;
            foreach (var pair in pairs)
            {
                SourceWords.Add(new MatchItemViewModel(id, pair.SourceWord));
                targetItems.Add(new MatchItemViewModel(id, pair.TargetWord));
                id++;
            }

            foreach (var item in targetItems.OrderBy(x => random.Next()))
            {
                TargetWords.Add(item);
            }
        }
        
        /// <summary>
        /// Обработчик изменения выбранного слова в левой колонке
        /// </summary>
        /// <param name="value">Выбранный элемент</param>
        async partial void OnSelectedSourceWordChanged(MatchItemViewModel? value)
        {
            if (value != null) await CheckForMatchAsync();
        }

        /// <summary>
        /// Обработчик изменения выбранного слова в правой колонке
        /// </summary>
        /// <param name="value">Выбранный элемент</param>
        async partial void OnSelectedTargetWordChanged(MatchItemViewModel? value)
        {
            if (value != null) await CheckForMatchAsync();
        }

        /// <summary>
        /// Проверка соответствия выбранных слов
        /// </summary>
        private async Task CheckForMatchAsync()
        {
            if (SelectedSourceWord == null || SelectedTargetWord == null)
                return;

            var source = SelectedSourceWord;
            var target = SelectedTargetWord;

            if (source.Id == target.Id)
            {
                source.IsMatched = true;
                target.IsMatched = true;
                CorrectlyMatchedCount++;
                if (CorrectlyMatchedCount == TotalPairsCount) IsCompleted = true;
            }
            else
            {
                source.IsInvalidMatch = true;
                target.IsInvalidMatch = true;
                await Task.Delay(1000);
                source.IsInvalidMatch = false;
                target.IsInvalidMatch = false;
            }

            SelectedSourceWord = null;
            SelectedTargetWord = null;
        }

        /// <summary>
        /// Команда завершения упражнения
        /// </summary>
        [RelayCommand]
        private void FinishExercise()
        {
            Console.WriteLine($"Упражнение завершено! Правильных пар: {CorrectlyMatchedCount}/{TotalPairsCount}");
            
            // Обработка завершения в зависимости от текущей темы
            if (_user.CurrentTopic.Id == "alphabet")
            {
                _user.Module1Passed = true;
                var authService = new AuthService();
                authService.MarkModuleAsCompleted(_user.Id, _user.CurrentTopic.Id);
                _main?.NavigateToLearningSwitch();
                return;
            }
            
            if (_user.CurrentTopic.Id == "family")
            {
                _user.Module4Passed = true;
                var authService = new AuthService();
                authService.MarkModuleAsCompleted(_user.Id, _user.CurrentTopic.Id);
                _main?.NavigateToLearningSwitch();
                return;
            }
            
            if (_user.CurrentTopic.Id == "numbers")
            {
                _user.Module3Passed = true;
                var authService = new AuthService();
                authService.MarkModuleAsCompleted(_user.Id, _user.CurrentTopic.Id);
                _main?.NavigateToLearningSwitch();
                return;
            }

            if (_user.CurrentTopic.Id == "food")
            {
                var authService = new AuthService();
                var imageData = authService.GetImageChoiceData();
                _main?.NavigateToImageTask(imageData);
            }

            if (_user.CurrentTopic.Id == "phrases")
            {
                var authService = new AuthService();
                var data = authService.GetFillInBlankData("FrasesFillInBlank");
                _main?.NavigateToFillInBlankTask(data);
            }
        }

        /// <summary>
        /// Команда возврата к выбору тем
        /// </summary>
        [RelayCommand]
        private void NavigateBack()
        {
            _main?.NavigateToLearningSwitch();
        }
    }
}