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
    /// Модель данных для задания типа "заполни пропуск" (вставь слово).
    /// Содержит шаблон предложения, правильный вариант и список неправильных вариантов.
    /// </summary>
    /// <param name="SentenceTemplate">Шаблон предложения с пропуском</param>
    /// <param name="CorrectWord">Правильное слово для вставки</param>
    /// <param name="IncorrectOptions">Коллекция неправильных вариантов</param>
    public record FillInBlankData(string SentenceTemplate, string CorrectWord, IEnumerable<string> IncorrectOptions);

    /// <summary>
    /// ViewModel для отдельного варианта ответа в задании "вставь слово".
    /// Отвечает за состояние и поведение одного варианта ответа.
    /// </summary>
    public partial class FillInBlankOptionViewModel : ObservableObject
    {
        /// <summary>
        /// Текущее состояние проверки варианта ответа.
        /// Может быть: Непроверен, Правильный, Неправильный.
        /// </summary>
        [ObservableProperty]
        private ValidationState _validationState = ValidationState.Unchecked;

        /// <summary>
        /// Указывает, совпадает ли текущий вариант с правильным ответом.
        /// </summary>
        [ObservableProperty]
        private bool _isMatched = false;

        /// <summary>
        /// Указывает, был ли вариант выбран ошибочно.
        /// </summary>
        [ObservableProperty]
        private bool _isInvalidMatch = false;

        /// <summary>
        /// Обрабатывает изменение состояния проверки варианта.
        /// Автоматически обновляет флаги IsMatched и IsInvalidMatch.
        /// </summary>
        /// <param name="value">Новое состояние проверки</param>
        partial void OnValidationStateChanged(ValidationState value)
        {
            IsMatched = (value == ValidationState.Correct);
            IsInvalidMatch = (value == ValidationState.Incorrect);
        }

        /// <summary>
        /// Текст варианта ответа.
        /// </summary>
        public string OptionText { get; }

        /// <summary>
        /// Флаг, указывающий является ли этот вариант правильным ответом.
        /// </summary>
        public bool IsCorrectOption { get; }

        /// <summary>
        /// Определяет, можно ли выбрать данный вариант.
        /// </summary>
        [ObservableProperty]
        private bool _canSelect = true;

        /// <summary>
        /// Событие, возникающее при выборе варианта.
        /// </summary>
        public event Action<FillInBlankOptionViewModel>? OptionSelected;

        /// <summary>
        /// Инициализирует новый экземпляр ViewModel варианта ответа.
        /// </summary>
        /// <param name="optionText">Текст варианта</param>
        /// <param name="isCorrect">Является ли вариант правильным</param>
        /// <param name="onSelectedCallback">Обратный вызов при выборе</param>
        public FillInBlankOptionViewModel(string optionText, bool isCorrect, Action<FillInBlankOptionViewModel> onSelectedCallback)
        {
            OptionText = optionText;
            IsCorrectOption = isCorrect;
            OptionSelected += onSelectedCallback;
        }

        /// <summary>
        /// Временно помечает вариант как неправильный с задержкой в 1 секунду.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        public async Task MarkIncorrectTemporarilyAsync()
        {
            ValidationState = ValidationState.Incorrect;
            await Task.Delay(1000);
            ValidationState = ValidationState.Unchecked;
        }

        /// <summary>
        /// Команда для выбора текущего варианта ответа.
        /// Вызывает событие OptionSelected, если выбор разрешен.
        /// </summary>
        [RelayCommand]
        private void SelectOption()
        {
            if (CanSelect)
                OptionSelected?.Invoke(this);
        }
    }

    /// <summary>
    /// ViewModel для задания типа "вставь слово".
    /// Управляет логикой выполнения задания и взаимодействием с пользователем.
    /// </summary>
    public partial class FillInBlankTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;
        private readonly string _correctWord;
        private readonly User _user = UserSession.CurrentUser!;

        /// <summary>
        /// Название текущего модуля обучения.
        /// </summary>
        [ObservableProperty]
        private string _moduleTitle;

        /// <summary>
        /// Шаблон предложения с пропуском для заполнения.
        /// </summary>
        [ObservableProperty]
        private string _sentenceTemplate;

        /// <summary>
        /// Флаг, указывающий завершено ли задание.
        /// </summary>
        [ObservableProperty]
        private bool _isCompleted;

        /// <summary>
        /// Текст на кнопке завершения задания.
        /// </summary>
        [ObservableProperty]
        private string _buttonText = "Завершить";

        /// <summary>
        /// Коллекция вариантов для заполнения пропуска.
        /// </summary>
        public ObservableCollection<FillInBlankOptionViewModel> Options { get; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр ViewModel задания.
        /// </summary>
        /// <param name="data">Данные для задания</param>
        /// <param name="mainWindowViewModel">Главная ViewModel приложения (опционально)</param>
        public FillInBlankTaskViewModel(FillInBlankData data, MainWindowViewModel? mainWindowViewModel = null)
        {
            _main = mainWindowViewModel;
            _correctWord = data.CorrectWord;
            _moduleTitle = _user.CurrentTopic.Title;
            _sentenceTemplate = data.SentenceTemplate;
            InitializeOptions(data.IncorrectOptions);
        }

        /// <summary>
        /// Инициализирует и перемешивает варианты ответов.
        /// </summary>
        /// <param name="incorrectOptions">Список неправильных вариантов</param>
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
        /// Обрабатывает выбор варианта пользователем.
        /// </summary>
        /// <param name="selectedOption">Выбранный вариант</param>
        private async void OnOptionSelected(FillInBlankOptionViewModel selectedOption)
        {
            if (IsCompleted) return;

            if (selectedOption.IsCorrectOption)
            {
                selectedOption.ValidationState = ValidationState.Correct;
                IsCompleted = true;
            }
            else
            {
                await selectedOption.MarkIncorrectTemporarilyAsync();
            }
        }

        /// <summary>
        /// Команда завершения упражнения.
        /// Помечает модуль как пройденный и сохраняет результат.
        /// </summary>
        [RelayCommand]
        private void FinishExercise()
        {
            _user.Module2Passed = true;
            var authService = new AuthService();
            authService.MarkModuleAsCompleted(_user.Id, _user.CurrentTopic.Id);
            _main?.NavigateToLearningSwitch();
        }

        /// <summary>
        /// Команда возврата к предыдущему экрану.
        /// </summary>
        [RelayCommand]
        private void NavigateBack() => _main?.NavigateToLearningSwitch();
    }
}