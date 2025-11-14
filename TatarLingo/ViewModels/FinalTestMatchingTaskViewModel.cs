using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using TatarLingo.Models;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для задания на сопоставление в финальном тесте.
    /// Позволяет пользователю сопоставлять слова из двух колонок.
    /// </summary>
    public partial class FinalTestMatchingTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;

        /// <summary>
        /// Коллекция исходных слов для сопоставления (левая колонка).
        /// </summary>
        public ObservableCollection<MatchItemViewModel> SourceWords { get; } = new();

        /// <summary>
        /// Коллекция целевых слов для сопоставления (правая колонка).
        /// </summary>
        public ObservableCollection<MatchItemViewModel> TargetWords { get; } = new();

        /// <summary>
        /// Выбранное слово в левой колонке.
        /// </summary>
        [ObservableProperty]
        private MatchItemViewModel? _selectedSourceWord;

        /// <summary>
        /// Выбранное слово в правой колонке.
        /// </summary>
        [ObservableProperty]
        private MatchItemViewModel? _selectedTargetWord;

        /// <summary>
        /// Количество правильно сопоставленных пар.
        /// </summary>
        [ObservableProperty]
        private int _correctlyMatchedCount;
        
        private int _incorrectlyMatchedCount;
        
        /// <summary>
        /// Флаг, указывающий завершено ли задание.
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted = false;

        /// <summary>
        /// Флаг, указывающий правильно ли выполнено задание.
        /// </summary>
        [ObservableProperty] 
        private bool _isCorrect = false;

        /// <summary>
        /// Текст на кнопке действия (Далее/Завершить).
        /// </summary>
        [ObservableProperty] 
        private string _buttonText = "Далее";
        
        private User _user = UserSession.CurrentUser!;

        /// <summary>
        /// Общее количество пар для сопоставления.
        /// </summary>
        public int TotalPairsCount { get; private set; }
        
        private readonly FinalTestViewModel _parentTestVm;
        
        /// <summary>
        /// Массив выполненных заданий пользователя.
        /// </summary>
        [ObservableProperty]
        private bool[] _doneTasks = UserSession.DoneTasks;

        /// <summary>
        /// Инициализирует новый экземпляр ViewModel задания на сопоставление.
        /// </summary>
        /// <param name="wordPairs">Коллекция пар слов для сопоставления</param>
        /// <param name="parentTestVm">Родительская ViewModel финального теста</param>
        public FinalTestMatchingTaskViewModel(IEnumerable<Models.MatchingPair> wordPairs, FinalTestViewModel parentTestVm)
        {
            if (UserSession.CurrentTestIndex == 9) _buttonText = "Завершить";
            _parentTestVm = parentTestVm;
            InitializeExercise(wordPairs);
        }

        /// <summary>
        /// Инициализирует упражнение, создавая элементы для сопоставления.
        /// </summary>
        /// <param name="wordPairs">Коллекция пар слов для сопоставления</param>
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
        /// Обрабатывает изменение выбранного слова в левой колонке.
        /// </summary>
        /// <param name="value">Выбранный элемент или null</param>
        async partial void OnSelectedSourceWordChanged(MatchItemViewModel? value)
        {
            if (value != null) await CheckForMatchAsync();
        }

        /// <summary>
        /// Обрабатывает изменение выбранного слова в правой колонке.
        /// </summary>
        /// <param name="value">Выбранный элемент или null</param>
        async partial void OnSelectedTargetWordChanged(MatchItemViewModel? value)
        {
            if (value != null) await CheckForMatchAsync();
        }

        /// <summary>
        /// Проверяет, являются ли выбранные слова правильной парой.
        /// Обновляет состояние элементов и счетчики прогресса.
        /// </summary>
        private async Task CheckForMatchAsync()
        {
            if (SelectedSourceWord == null || SelectedTargetWord == null)
                return;

            // Копируем ссылки, чтобы избежать их сброса в null до завершения анимации
            var source = SelectedSourceWord;
            var target = SelectedTargetWord;

            if (source.Id == target.Id)
            {
                source.IsMatched = true;
                target.IsMatched = true;
                CorrectlyMatchedCount++;
                if (CorrectlyMatchedCount == TotalPairsCount)
                {
                    IsCompleted = true;
                    IsCorrect = true;
                }
            }
            else
            {
                _incorrectlyMatchedCount++;
                source.IsInvalidMatch = true;
                target.IsInvalidMatch = true;
                IsCorrect = false;
            }

            if (CorrectlyMatchedCount < TotalPairsCount && 
                _incorrectlyMatchedCount + CorrectlyMatchedCount == TotalPairsCount)
            {
                IsCompleted = true;
                IsCorrect = false;
            }

            SelectedSourceWord = null;
            SelectedTargetWord = null;
        }

        /// <summary>
        /// Команда завершения текущего задания.
        /// Передает управление родительской ViewModel теста.
        /// </summary>
        [RelayCommand]
        private void FinishExercise() => _parentTestVm.NextCommand.Execute(null);

        /// <summary>
        /// Команда возврата из теста.
        /// Передает управление родительской ViewModel теста.
        /// </summary>
        [RelayCommand]
        private void NavigateBack() => _parentTestVm.ExitTestCommand.Execute(null);
    }
}