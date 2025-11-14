using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using TatarLingo.Models;
using TatarLingo.Views;

namespace TatarLingo.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения, управляющая навигацией между View
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Текущее отображаемое View
        /// </summary>
        [ObservableProperty] 
        private object? currentView;

        /// <summary>
        /// Инициализирует новый экземпляр MainWindowViewModel
        /// </summary>
        /// <remarks>
        /// По умолчанию устанавливает экран входа в систему как начальный View
        /// </remarks>
        public MainWindowViewModel()
        {
            CurrentView = new LoginViewModel(this);
        }

        /// <summary>
        /// Переход к экрану регистрации нового пользователя
        /// </summary>
        public void NavigateToRegister()
        {
            CurrentView = new RegisterViewModel(this);
        }

        /// <summary>
        /// Возврат к экрану входа в систему
        /// </summary>
        public void NavigateToLogin()
        {
            CurrentView = new LoginViewModel(this);
        }

        /// <summary>
        /// Переход к экрану выбора тем обучения
        /// </summary>
        public void NavigateToLearningSwitch()
        {
            CurrentView = new LearningSwitchViewModel(this);
        }

        /// <summary>
        /// Переход к экрану лекции по выбранной теме
        /// </summary>
        /// <param name="topic">Тема лекции</param>
        public void NavigateToLecture(Topic topic)
        {
            CurrentView = new LectureViewModel(topic, this);
        }

        /// <summary>
        /// Переход к заданию на сопоставление слов
        /// </summary>
        /// <param name="wordPairs">Коллекция пар слов для сопоставления</param>
        public void NavigateToTask(IEnumerable<Models.MatchingPair> wordPairs)
        {
            CurrentView = new MatchingTaskViewModel(wordPairs, this);
        }

        /// <summary>
        /// Переход к заданию с выбором изображений
        /// </summary>
        /// <param name="imageData">Данные изображений для задания</param>
        public void NavigateToImageTask(IEnumerable<ImageChoiceData> imageData)
        {
            CurrentView = new ImageChoiceTaskViewModel(imageData, this);
        }

        /// <summary>
        /// Переход к заданию на заполнение пропусков
        /// </summary>
        /// <param name="fillInBlankData">Данные для задания</param>
        public void NavigateToFillInBlankTask(FillInBlankData fillInBlankData)
        {
            CurrentView = new FillInBlankTaskViewModel(fillInBlankData, this);
        }

        /// <summary>
        /// Переход к финальному тесту
        /// </summary>
        public void NavigateToFinalTest()
        {
            CurrentView = new FinalTestViewModel(this);
        }
    }
}