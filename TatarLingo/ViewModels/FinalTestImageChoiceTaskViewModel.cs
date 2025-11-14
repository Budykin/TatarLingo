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
    /// ViewModel для задания с выбором изображений в финальном тесте.
    /// Управляет логикой задания, где пользователь должен сопоставить изображения с правильными словами.
    /// </summary>
    public partial class FinalTestImageChoiceTaskViewModel : ObservableObject
    {
        private readonly MainWindowViewModel? _main;
        
        /// <summary>
        /// Коллекция элементов задания с изображениями и вариантами выбора.
        /// </summary>
        public ObservableCollection<ImageChoiceItemViewModel> Items { get; } = new();
        
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
        
        private int _correctlyAnsweredCount;
        private int _incorrectlyAnsweredCount;
        private int _AnsweredCount;
        private int _totalItemsCount;
        private User _user = UserSession.CurrentUser!;
        
        private readonly FinalTestViewModel _parentTestVm;
        
        /// <summary>
        /// Массив выполненных заданий пользователя.
        /// </summary>
        [ObservableProperty]
        private bool[] _doneTasks = UserSession.DoneTasks;

        /// <summary>
        /// Инициализирует новый экземпляр ViewModel задания с выбором изображений.
        /// </summary>
        /// <param name="imageData">Коллекция данных для задания (изображения и правильные ответы)</param>
        /// <param name="parentTestVm">Родительская ViewModel финального теста</param>
        public FinalTestImageChoiceTaskViewModel(IEnumerable<ImageChoiceData> imageData, FinalTestViewModel parentTestVm)
        {
            if (UserSession.CurrentTestIndex == 9) _buttonText = "Завершить";
            _parentTestVm = parentTestVm;
            InitializeExercise(imageData);
        }

        /// <summary>
        /// Инициализирует упражнение, создавая элементы задания на основе переданных данных.
        /// </summary>
        /// <param name="imageData">Коллекция данных для задания</param>
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
        /// Обрабатывает событие ответа на элемент задания.
        /// Обновляет счетчики правильных/неправильных ответов и состояние задания.
        /// </summary>
        /// <param name="isCorrect">Был ли ответ правильным</param>
        private void OnItemAnswered(bool isCorrect)
        {
            if (isCorrect)
            {
                _correctlyAnsweredCount++;
                _AnsweredCount++;
            }
            else
            {
                _incorrectlyAnsweredCount++;
                _AnsweredCount++;
            }
            
            if (_correctlyAnsweredCount == _totalItemsCount)
            {
                IsCompleted = true;
                IsCorrect = true;
            } 
            else if (_correctlyAnsweredCount < _totalItemsCount &&
                     _AnsweredCount + _incorrectlyAnsweredCount + _correctlyAnsweredCount >= _totalItemsCount)
            {
                IsCompleted = true;
                IsCorrect = false;
            }
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