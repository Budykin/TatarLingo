using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using TatarLingo.ViewModels;


namespace TatarLingo.Models;

/// <summary>
/// Сервис аутентификации и работы с базой данных TatarLingo.
/// </summary>
public class AuthService
{
    private readonly string _connectionString =
        $"Data Source={Path.Combine(AppContext.BaseDirectory, "TatarlingoDataBase.db")}";

    /// <summary>
    /// Проверяет правильность введённых логина и пароля.
    /// </summary>
    /// <param name="username">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <returns>True, если пользователь найден и данные корректны; иначе — false.</returns>
    public bool CheckCredentials(string username, string password)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string query = @"
        SELECT u.Id, u.UserName, m.Module1, m.Module2, m.Module3, m.Module4, m.Module5, m.Test, u.Email
        FROM Users u
        LEFT JOIN Modules m ON u.Id = m.UserId
        WHERE u.UserName = @username AND u.Password = @password";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            UserSession.CurrentUser = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Module1Passed = reader.GetInt32(2) == 1,
                Module2Passed = reader.GetInt32(3) == 1,
                Module3Passed = reader.GetInt32(4) == 1,
                Module4Passed = reader.GetInt32(5) == 1,
                Module5Passed = reader.GetInt32(6) == 1,
                FinalTestPassed = reader.GetInt32(7) == 1,
                Email = reader.GetString(8),
            };
            return true;
        }

        return false;
    }

    /// <summary>
    /// Получает пары слов (например, для задания "Соедини пары") из указанной таблицы.
    /// </summary>
    /// <param name="table">Имя таблицы в базе данных.</param>
    /// <returns>Список пар слов.</returns>
    public IEnumerable<MatchingPair> GetWordPairs(string table)
    {
        var wordPairs = new List<MatchingPair>();
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string query = $"SELECT SourceWord, TargetWord FROM {table} ORDER BY RANDOM() LIMIT 6";
        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string sourceWord = reader.GetString(0);
            string targetWord = reader.GetString(1);
            wordPairs.Add(new MatchingPair(sourceWord, targetWord));
        }
        
        return wordPairs;
    }
    
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="username">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="email">Адрес электронной почты.</param>
    /// <returns>True, если регистрация прошла успешно; иначе — false (например, пользователь уже существует).</returns>
    public bool RegisterUser(string username, string password, string email)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Проверка: нет ли уже такого пользователя
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT COUNT(*) FROM Users WHERE UserName = @username";
        checkCommand.Parameters.AddWithValue("@username", username);

        long count = (long)checkCommand.ExecuteScalar();
        if (count > 0)
        {
            // Пользователь с таким именем уже существует
            return false;
        }

        // Добавление нового пользователя
        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
        INSERT INTO Users (UserName, Password, Email)
        VALUES (@username, @password, @email)";
        
        insertCommand.Parameters.AddWithValue("@username", username);
        insertCommand.Parameters.AddWithValue("@password", password); // Лучше хешировать!
        insertCommand.Parameters.AddWithValue("@email", email);
        insertCommand.ExecuteNonQuery();
        
        insertCommand.CommandText = @"
        INSERT INTO Modules (UserId)
        VALUES (
            (SELECT Id FROM Users ORDER BY Id DESC LIMIT 1)
        )";
        insertCommand.ExecuteNonQuery();
        return true;
    }
    
    /// <summary>
    /// Отмечает прохождение модуля пользователем.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="moduleId">Идентификатор модуля (например, "alphabet").</param>
    /// <returns>True, если модуль отмечен как завершён; иначе — false.</returns>
    public bool MarkModuleAsCompleted(int userId, string moduleId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var modules = new Dictionary<string, int>
        {
            ["alphabet"] = 1,
            ["phrases"] = 2,
            ["numbers"] = 3,
            ["family"] = 4,
            ["food"] = 5
        };
        if (!modules.TryGetValue(moduleId, out int moduleNumber))
        {
            return false;
        }
        string columnName = $"Module{moduleNumber}";
        string query = $"UPDATE Modules SET {columnName} = 1 WHERE UserId = @userId";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.ExecuteNonQuery();
        return true;
    }
    
    /// <summary>
    /// Отмечает прохождение финального теста пользователем.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="result">Результат теста.</param>
    /// <param name="testDate">Дата прохождения теста.</param>
    /// <returns>True, если обновление прошло успешно.</returns>
    public bool MarkTestAsCompleted(int userId, int result, string testDate)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
    
        var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = @"
        UPDATE Users 
        SET TestResult = @testresult,
            TestDate = @testdate
        WHERE Id = @userId";
    
        updateCommand.Parameters.AddWithValue("@testresult", result);
        updateCommand.Parameters.AddWithValue("@userId", userId);
        updateCommand.Parameters.AddWithValue("@testdate", testDate);
        updateCommand.ExecuteNonQuery();
        return true;
    }
    
    /// <summary>
    /// Получает список заданий с выбором по изображению.
    /// </summary>
    /// <returns>Список объектов <see cref="ImageChoiceData"/> с изображениями и словами.</returns>
    public IEnumerable<ImageChoiceData> GetImageChoiceData()
    {
        var imageDataList = new List<ImageChoiceData>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string query = "SELECT Image AS ImagePath, Word AS CorrectWord FROM ImageChoiceFood ORDER BY RANDOM() LIMIT 3";
        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string imagePath = $"avares://TatarLingo/Assets/ImageChoiceFood/{reader.GetString(0)}.png";
            string correctWord = reader.GetString(1);
            imageDataList.Add(new ImageChoiceData(imagePath, correctWord));
        }

        return imageDataList;
    }

    /// <summary>
    /// Получает одно задание с пропущенным словом и вариантами ответа.
    /// </summary>
    /// <param name="table">Имя таблицы с заданиями.</param>
    /// <returns>Объект <see cref="FillInBlankData"/> или null, если запись не найдена.</returns>
    public FillInBlankData? GetFillInBlankData(string table)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. Получаем одну случайную запись
        string queryCorrect = $"SELECT Id, SentenceTemplate, CorrectWord FROM {table} ORDER BY RANDOM() LIMIT 1;";
        int correctId = -1;
        string sentenceTemplate = string.Empty;
        string correctWord = string.Empty;

        using (var command = new SqliteCommand(queryCorrect, connection))
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                correctId = reader.GetInt32(0);
                sentenceTemplate = reader.GetString(1);
                correctWord = reader.GetString(2);
            }
        }

        if (correctId == -1)
            return null; // ничего не найдено

        // 2. Получаем три других варианта слова, исключая правильный
        string queryIncorrect = $"SELECT CorrectWord  FROM {table} WHERE Id != @correctId ORDER BY RANDOM() LIMIT 3;";

        var incorrectOptions = new List<string>();

        using (var command = new SqliteCommand(queryIncorrect, connection))
        {
            command.Parameters.AddWithValue("@correctId", correctId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                incorrectOptions.Add(reader.GetString(0));
            }
        }

        // 3. Формируем результат
        return new FillInBlankData(sentenceTemplate, correctWord, incorrectOptions);
    }

    /// <summary>
    /// Формирует финальный тест, объединяя задания разных типов.
    /// </summary>
    /// <returns>Список заданий для финального теста.</returns>
    public List<TaskData> GetFinalTestTasks()
    {
        var tasks = new List<TaskData>();
        
        var matchTables = new[] { "FamilyMatch", "FoodMatch", "AlphabetMatch", "NumbersMatch", "PhrasesMatch" }; // добавь свои таблицы
        foreach (var table in matchTables)
        {
            var matchPairs = GetWordPairs(table);
            tasks.Add(new TaskData
            {
                Id = Guid.NewGuid().ToString(),
                Topic = table,
                Type = TaskType.MatchTerms,
                Payload = matchPairs
            });
        }
        
        var blankTables = new[] { "FrasesFillInBlank", "NumbersFillInBlank", "FoodFillInBlank" }; // добавь свои таблицы
        foreach (var table in blankTables)
        {
            var blankData = GetFillInBlankData(table);
            if (blankData != null)
            {
                tasks.Add(new TaskData
                {
                    Id = Guid.NewGuid().ToString(),
                    Topic = table,
                    Type = TaskType.FillInBlank,
                    Payload = blankData
                });
            }
        }
        
        var imageTasks = GetImageChoiceData();
        tasks.Add(new TaskData
        {
            Id = Guid.NewGuid().ToString(),
            Topic = "ImageChoiceFood",
            Type = TaskType.ImageChoice,
            Payload = imageTasks
        });

        // Перемешиваем и берём 10
        return tasks
            .OrderBy(_ => Guid.NewGuid())
            .Take(9)
            .ToList();
    }
}