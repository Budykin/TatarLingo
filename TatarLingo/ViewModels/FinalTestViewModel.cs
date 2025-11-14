using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TatarLingo.Models;
using System.Net.Mail;
using TatarLingo.Views;

namespace TatarLingo.ViewModels;

/// <summary>
/// ViewModel для управления финальным тестом приложения TatarLingo.
/// Отвечает за загрузку заданий, навигацию между ними и обработку результатов.
/// </summary>
public partial class FinalTestViewModel : ObservableObject
{
    /// <summary>
    /// Текущая ViewModel активного задания теста.
    /// </summary>
    [ObservableProperty]
    private ObservableObject? currentTaskViewModel;

    /// <summary>
    /// Номер текущего задания в тесте (начиная с 1).
    /// </summary>
    [ObservableProperty]
    private int currentTaskNumber;

    /// <summary>
    /// Флаг, указывающий завершен ли тест.
    /// </summary>
    [ObservableProperty]
    private bool isTestFinished;

    /// <summary>
    /// Строка с итоговыми результатами теста.
    /// </summary>
    [ObservableProperty]
    public string resultSummary = "";

    private readonly MainWindowViewModel _main;
    
    private User _user = UserSession.CurrentUser!;

    /// <summary>
    /// Инициализирует новый экземпляр FinalTestViewModel.
    /// </summary>
    /// <param name="main">Главная ViewModel приложения</param>
    public FinalTestViewModel(MainWindowViewModel main)
    {
        _main = main;
        LoadNextTask();
    }

    /// <summary>
    /// Команда перехода к следующему заданию или завершения теста.
    /// </summary>
    [RelayCommand]
    private void Next()
    {
        SaveCurrentResult();

        if (UserSession.CurrentTestIndex >= UserSession.FinalTestTasks.Count)
        {
            FinishTest();
            return;
        }

        LoadNextTask();
    }

    /// <summary>
    /// Загружает следующее задание теста.
    /// </summary>
    private void LoadNextTask()
    {
        var task = UserSession.GetNextTestTask();
        if (task == null)
        {
            FinishTest();
            return;
        }

        CurrentTaskNumber = UserSession.CurrentTestIndex;

        switch (task.Type)
        {
            case TaskType.FillInBlank:
                CurrentTaskViewModel = new FinalTestFillInBlankTaskViewModel((FillInBlankData)task.Payload, this);
                break;
            case TaskType.MatchTerms:
                CurrentTaskViewModel = new FinalTestMatchingTaskViewModel((IEnumerable<MatchingPair>)task.Payload, this);
                break;
            case TaskType.ImageChoice:
                CurrentTaskViewModel = new FinalTestImageChoiceTaskViewModel((IEnumerable<ImageChoiceData>)task.Payload, this);
                break;
        }
    }

    /// <summary>
    /// Сохраняет результат текущего задания.
    /// </summary>
    private void SaveCurrentResult()
    {
        bool result = false;

        switch (CurrentTaskViewModel)
        {
            case FinalTestFillInBlankTaskViewModel fib:
                result = fib.IsCorrect;
                break;
            case FinalTestMatchingTaskViewModel match:
                result = match.IsCorrect;
                break;
            case FinalTestImageChoiceTaskViewModel img:
                result = img.IsCorrect;
                break;
        }

        // сохраняем результат в текущего пользователя
        if (UserSession.CurrentUser != null)
        {
            UserSession.CurrentUser.TestResults[CurrentTaskNumber] = result;
        }
    }

    /// <summary>
    /// Завершает тест, подсчитывает результаты и отправляет уведомление.
    /// </summary>
    private void FinishTest()
    {
        IsTestFinished = true;

        int correctCount = 0;
        if (UserSession.CurrentUser != null)
        {
            foreach (var result in UserSession.CurrentUser.TestResults)
            {
                if (result)
                    correctCount++;
            }

            ResultSummary = $"Правильных ответов: {correctCount} из 9";
            Console.WriteLine(ResultSummary);
        }

        var resultWindow = new TestResultDialog(ResultSummary);
        resultWindow.Show();
        
        SendEmail(correctCount);
        
        var testdate = DateTime.Today.Date.ToShortDateString();
        var authServise = new AuthService();
        authServise.MarkTestAsCompleted(_user.Id, correctCount, testdate);
        _main.NavigateToLearningSwitch();
        UserSession.ExitTest();
    }

    /// <summary>
    /// Отправляет email с результатами теста на почту пользователя.
    /// </summary>
    /// <param name="correctCount">Количество правильных ответов</param>
    private void SendEmail(int correctCount)
    {
        if (string.IsNullOrEmpty(_user.Email)) return;
        try
        {
            // Настройки SMTP-сервера 
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Для Gmail с TLS
            string smtpUsername = "budykin2007@gmail.com";
            string smtpPassword = "uqlv odor nbsy iblh";

            // Создание письма
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(smtpUsername);
            mail.To.Add(_user.Email);
            mail.Subject = "TatarLingo";
            mail.Body = $"Результат финального теста: {correctCount}/9";
            mail.IsBodyHtml = false;

            // Настройка SMTP-клиента
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            // Отправка письма
            smtpClient.Send(mail);
            Console.WriteLine("Письмо успешно отправлено!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при отправке письма: " + ex.Message);
        }
    }

    /// <summary>
    /// Команда выхода из теста без завершения.
    /// </summary>
    [RelayCommand]
    private void ExitTest()
    {
        _main.NavigateToLearningSwitch();
        UserSession.ExitTest();
    }

    /// <summary>
    /// Команда перехода к определенному заданию теста.
    /// </summary>
    /// <param name="testIndex">Индекс задания (начиная с 0)</param>
    [RelayCommand]
    private void CertainTask(int testIndex)
    {
        CurrentTaskNumber = UserSession.CurrentTestIndex;
    }
}