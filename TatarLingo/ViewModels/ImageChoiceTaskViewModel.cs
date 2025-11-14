using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TatarLingo.Models;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// Модель данных для задания с выбором изображения
    /// </summary>
    /// <param name="ImagePath">Путь к изображению</param>
    /// <param name="CorrectWord">Правильное слово, соответствующее изображению</param>
    public record ImageChoiceData(string ImagePath, string CorrectWord);

    /// <summary>
    /// Состояние проверки ответа
    /// </summary>
    public enum ValidationState
    {
        /// <summary>Ответ еще не проверен</summary>
        Unchecked,
        /// <summary>Ответ верный</summary>
        Correct,
        /// <summary>Ответ неверный</summary>
        Incorrect
    }

    /// <summary>
    /// ViewModel для элемента задания с выбором изображения
    /// </summary>
    public partial class ImageChoiceItemViewModel : ObservableObject
    {
        /// <summary>
        /// Текущее состояние проверки ответа
        /// </summary>
        [ObservableProperty]
        private ValidationState _validationState = ValidationState.Unchecked;

        /// <summary>
        /// Флаг, указывающий что ответ верный
        /// </summary>
        [ObservableProperty]
        private bool _isMatched = false;

        /// <summary>
        /// Флаг, указывающий что ответ неверный
        /// </summary>
        [ObservableProperty]
        private bool _isInvalidMatch = false;

        /// <summary>
        /// Флаг, указывающий что дан ответ
        /// </summary>
        [ObservableProperty]
        private bool _isAnswered = false;

        /// <summary>
        /// Выбранное слово
        /// </summary>
        [ObservableProperty]
        private string? _selectedWord;

        /// <summary>
        /// Изображение для задания
        /// </summary>
        public Bitmap? ImagePath { get; }

        /// <summary>
        /// Правильное слово для этого изображения
        /// </summary>
        private string CorrectWord { get; }

        /// <summary>
        /// Доступные варианты ответов
        /// </summary>
        public ObservableCollection<string> Options { get; }

        /// <summary>
        /// Событие, возникающее при ответе
        /// </summary>
        public event Action<bool>? Answered;

        /// <summary>
        /// Конструктор элемента задания
        /// </summary>
        /// <param name="imagePath">Путь к изображению</param>
        /// <param name="correctWord">Правильное слово</param>
        /// <param name="allWords">Все возможные варианты ответов</param>
        public ImageChoiceItemViewModel(string imagePath, string correctWord, IEnumerable<string> allWords)
        {
            try
            {
                var uri = new Uri(imagePath, UriKind.Absolute);
                ImagePath = new Bitmap(AssetLoader.Open(uri));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {imagePath}. Exception: {ex.Message}");
                ImagePath = null;
            }

            CorrectWord = correctWord;
            Options = new ObservableCollection<string>(allWords.OrderBy(x => Guid.NewGuid()));
        }

        /// <summary>
        /// Обработчик изменения состояния проверки
        /// </summary>
        partial void OnValidationStateChanged(ValidationState value)
        {
            IsMatched = (value == ValidationState.Correct);
            IsInvalidMatch = (value == ValidationState.Incorrect);
        }

        /// <summary>
        /// Обработчик изменения выбранного слова
        /// </summary>
        partial void OnSelectedWordChanged(string? value)
        {
            if (value == null || IsAnswered) return;

            bool isCorrect = (value == CorrectWord);
            ValidationState = isCorrect ? ValidationState.Correct : ValidationState.Incorrect;

            if (isCorrect)
            {
                IsAnswered = true;
            }

            Answered?.Invoke(isCorrect);
        }
    }

    /// <summary>
    /// ViewModel для задания с выбором изображений
    /// </summary>
    public partial class ImageChoiceTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;

        /// <summary>
        /// Название текущего модуля
        /// </summary>
        [ObservableProperty] 
        private string _moduleTitle;

        /// <summary>
        /// Коллекция элементов задания
        /// </summary>
        public ObservableCollection<ImageChoiceItemViewModel> Items { get; } = new();

        /// <summary>
        /// Флаг завершения задания
        /// </summary>
        [ObservableProperty] 
        private bool _isCompleted = false;

        /// <summary>
        /// Текст кнопки завершения
        /// </summary>
        [ObservableProperty] 
        private string _buttonText = "Завершить";
        
        private int _correctlyAnsweredCount;
        private int _totalItemsCount;
        private User _user = UserSession.CurrentUser!;

        /// <summary>
        /// Конструктор ViewModel задания
        /// </summary>
        /// <param name="imageData">Данные изображений</param>
        /// <param name="mainWindowViewModel">Главная ViewModel</param>
        public ImageChoiceTaskViewModel(IEnumerable<ImageChoiceData> imageData, MainWindowViewModel? mainWindowViewModel = null)
        {
            _moduleTitle = _user.CurrentTopic.Title;
            _main = mainWindowViewModel;
            InitializeExercise(imageData);
        }

        /// <summary>
        /// Инициализация упражнения
        /// </summary>
        /// <param name="imageData">Данные изображений</param>
        private void InitializeExercise(IEnumerable<ImageChoiceData> imageData)
        {
            var dataList = imageData.ToList();
            if (!dataList.Any()) return;

            _totalItemsCount = dataList.Count;
            var allWords = dataList.Select(d => d.CorrectWord).Distinct().ToList();

            foreach (var data in dataList.OrderBy(x => Guid.NewGuid()))
            {
                var itemVM = new ImageChoiceItemViewModel(data.ImagePath, data.CorrectWord, allWords);
                itemVM.Answered += OnItemAnswered;
                Items.Add(itemVM);
            }
        }

        /// <summary>
        /// Обработчик ответа на элемент задания
        /// </summary>
        /// <param name="isCorrect">Был ли ответ верным</param>
        private void OnItemAnswered(bool isCorrect)
        {
            if (isCorrect) _correctlyAnsweredCount++;
            if (_correctlyAnsweredCount == _totalItemsCount) IsCompleted = true;
        }

        /// <summary>
        /// Команда завершения упражнения
        /// </summary>
        [RelayCommand]
        private void FinishExercise()
        {
            _user.Module5Passed = true;
            var authService = new AuthService();
            authService.MarkModuleAsCompleted(_user.Id, _user.CurrentTopic.Id);
            _main?.NavigateToLearningSwitch();
        }

        /// <summary>
        /// Команда возврата назад
        /// </summary>
        [RelayCommand]
        private void NavigateBack()
        {
            _main?.NavigateToLearningSwitch();
        }
    }
}