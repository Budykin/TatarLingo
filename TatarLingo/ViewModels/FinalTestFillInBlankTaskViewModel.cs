using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TatarLingo.Models;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// ViewModel для задания типа "заполни пропуск" в финальном тесте
    /// </summary>
    public partial class FinalTestFillInBlankTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;
        private readonly string _correctWord;
        private readonly User _user = UserSession.CurrentUser!;

        /// <summary>
        /// Шаблон предложения с пропуском для заполнения
        /// </summary>
        [ObservableProperty]
        private string _sentenceTemplate;

        /// <summary>
        /// Флаг, указывающий завершено ли задание
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;

        /// <summary>
        /// Флаг, указывающий правильно ли выполнено задание
        /// </summary>
        [ObservableProperty] 
        private bool _isCorrect = false;

        /// <summary>
        /// Текст на кнопке действия (Далее/Завершить)
        /// </summary>
        [ObservableProperty]
        private string _buttonText = "Далее";
        
        private readonly FinalTestViewModel _parentTestVm;
        
        /// <summary>
        /// Массив выполненных заданий пользователя
        /// </summary>
        [ObservableProperty]
        private bool[] _doneTasks = UserSession.DoneTasks;

        /// <summary>
        /// Коллекция вариантов для заполнения пропуска
        /// </summary>
        public ObservableCollection<FillInBlankOptionViewModel> Options { get; } = new();

        /// <summary>
        /// Конструктор ViewModel задания
        /// </summary>
        /// <param name="data">Данные для задания</param>
        /// <param name="parentTestVm">Родительская ViewModel теста</param>
        public FinalTestFillInBlankTaskViewModel(FillInBlankData data, FinalTestViewModel parentTestVm)
        {
            if (UserSession.CurrentTestIndex == 9) _buttonText = "Завершить";
            _parentTestVm = parentTestVm;
            _correctWord = data.CorrectWord;
            _sentenceTemplate = data.SentenceTemplate;
            InitializeOptions(data.IncorrectOptions);
        }

        /// <summary>
        /// Инициализирует коллекцию вариантов ответа
        /// </summary>
        /// <param name="incorrectOptions">Список некорректных вариантов</param>
        private void InitializeOptions(IEnumerable<string> incorrectOptions)
        {
            var allOptions = new List<string>(incorrectOptions) { _correctWord };
            var random = new Random();
            var shuffledOptions = allOptions.OrderBy(x => random.Next());

            Options.Clear();
            foreach (var optionText in shuffledOptions)
            {
                var optionVM = new FillInBlankOptionViewModel(optionText, optionText == _correctWord, OnOptionSelected);
                Options.Add(optionVM);
            }
        }

        /// <summary>
        /// Обработчик выбора варианта ответа
        /// </summary>
        /// <param name="selectedOption">Выбранный вариант</param>
        private async void OnOptionSelected(FillInBlankOptionViewModel selectedOption)
        {
            if (IsCompleted) return;

            if (selectedOption.IsCorrectOption)
            {
                selectedOption.ValidationState = ValidationState.Correct;
                IsCompleted = true;
                IsCorrect = true;
            }
            else
            {
                selectedOption.ValidationState = ValidationState.Incorrect;
                IsCompleted = true;
                IsCorrect = false;
            }
        }

        /// <summary>
        /// Команда завершения текущего задания
        /// </summary>
        [RelayCommand]
        private void FinishExercise() => _parentTestVm.NextCommand.Execute(null);

        /// <summary>
        /// Команда возврата из теста
        /// </summary>
        [RelayCommand]
        private void NavigateBack() => _parentTestVm.ExitTestCommand.Execute(null);
    }
}