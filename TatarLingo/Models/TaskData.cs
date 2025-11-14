namespace TatarLingo.Models 
{
    /// <summary>
    /// Класс <c>TaskData</c> представляет задание для теста, содержащее тему, тип и данные.
    /// </summary>
    public class TaskData
    {
        /// <summary>
        /// Уникальный идентификатор задания.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Тема задания (например, "food", "family").
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Тип задания (например, вставка слова, выбор изображения и т.д.).
        /// </summary>
        public TaskType Type { get; set; }

        /// <summary>
        /// Данные задания (например, MatchingPair, FillInBlankData, список ImageChoiceData).
        /// </summary>
        public object Payload { get; set; }
    }

    /// <summary>
    /// Перечисление типов заданий, доступных в приложении.
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// Задание с пропущенным словом.
        /// </summary>
        FillInBlank,

        /// <summary>
        /// Задание на сопоставление терминов.
        /// </summary>
        MatchTerms,

        /// <summary>
        /// Задание на выбор правильного слова по изображению.
        /// </summary>
        ImageChoice
    }
}