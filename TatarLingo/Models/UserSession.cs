using System;
using System.Collections.Generic;

namespace TatarLingo.Models;

/// <summary>
/// Статический класс для хранения информации о текущем сеансе пользователя.
/// </summary>
public static class UserSession
{
    /// <summary>
    /// Текущий авторизованный пользователь.
    /// </summary>
    public static User? CurrentUser { get; set; }

    /// <summary>
    /// Список заданий итогового теста.
    /// </summary>
    public static List<TaskData> FinalTestTasks { get; set; } = new();

    /// <summary>
    /// Индекс текущего задания теста.
    /// </summary>
    public static int CurrentTestIndex { get; set; } = 0;

    /// <summary>
    /// Массив флагов, указывающих на выполненные задания теста.
    /// </summary>
    public static bool[] DoneTasks { get; set; } = new bool[10];

    /// <summary>
    /// Инициализирует итоговый тест списком заданий и сбрасывает индекс.
    /// </summary>
    /// <param name="tasks">Список заданий для теста.</param>
    public static void InitializeFinalTest(List<TaskData> tasks)
    {
        FinalTestTasks = tasks;
        CurrentTestIndex = 0;
    }

    /// <summary>
    /// Возвращает следующее задание теста, если оно доступно.
    /// </summary>
    /// <returns>Следующее задание или null, если тест завершён.</returns>
    public static TaskData? GetNextTestTask()
    {
        if (CurrentTestIndex < FinalTestTasks.Count)
        {
            DoneTasks[CurrentTestIndex] = true;
            return FinalTestTasks[CurrentTestIndex++];
        }
        return null;
    }

    /// <summary>
    /// Завершает текущий тест и сбрасывает данные.
    /// </summary>
    public static void ExitTest()
    {
        Array.Fill(DoneTasks, false);
        FinalTestTasks.Clear();
        CurrentTestIndex = 0;
    }
}

/// <summary>
/// Модель пользователя приложения.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Пройден ли первый модуль.
    /// </summary>
    public bool Module1Passed { get; set; }

    /// <summary>
    /// Пройден ли второй модуль.
    /// </summary>
    public bool Module2Passed { get; set; }

    /// <summary>
    /// Пройден ли третий модуль.
    /// </summary>
    public bool Module3Passed { get; set; }

    /// <summary>
    /// Пройден ли четвёртый модуль.
    /// </summary>
    public bool Module4Passed { get; set; }

    /// <summary>
    /// Пройден ли пятый модуль.
    /// </summary>
    public bool Module5Passed { get; set; }

    /// <summary>
    /// Пройден ли итоговый тест.
    /// </summary>
    public bool FinalTestPassed { get; set; }

    /// <summary>
    /// Email пользователя.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Результаты выполнения заданий теста.
    /// </summary>
    public bool[] TestResults { get; set; } = new bool[10];

    /// <summary>
    /// Текущая выбранная тема.
    /// </summary>
    public Topic CurrentTopic { get; set; }
}
