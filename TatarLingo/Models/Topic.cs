namespace TatarLingo.Models 
{
    /// <summary>
    /// Класс <c>Topic</c> представляет данные для лекции, содержащее идентификатор, заголовок и имя md-ресурса.
    /// </summary>
    public class Topic
    {
        /// <summary>
        /// Уникальный идентификатор темы, например "alphabet"
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Отображаемое название темы, например "Алфавит и произношение"
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Полное имя внедренного ресурса .md файла для этой темы
        /// </summary>
        public required string MarkdownResourceName { get; set; }
    }
}