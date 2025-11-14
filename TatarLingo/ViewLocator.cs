using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace TatarLingo
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param == null)
                return null;

            // Получаем тип ViewModel-а, например: TatarLingo.ViewModels.LoginViewModel
            var viewModelType = param.GetType();
            var fullName = viewModelType.FullName;
            if (fullName == null)
                return null;

            // Шаг 1: Заменяем namespace "TatarLingo.ViewModels" → "TatarLingo.Views"
            // Шаг 2: Заменяем окончание "ViewModel" → "View"
            //
            // Например:
            //   fullName = "TatarLingo.ViewModels.LoginViewModel"
            //   viewName = "TatarLingo.Views.LoginView"
            var viewName = fullName
                .Replace(".ViewModels.", ".Views.")
                .Replace("ViewModel", "View");

            // Шаг 3: Ищем этот тип **в той же сборке**, где лежит ViewModel
            var asm = viewModelType.Assembly;
            var viewType = asm.GetType(viewName);

            if (viewType == null)
            {
                // Если всё ещё не нашли, выводим "Not Found" с именем, которое искали
                return new TextBlock
                {
                    Text = $"Not Found: {viewName}"
                };
            }

            // Шаг 4: Создаём экземпляр найденного класса (он должен наследовать Avalonia.Controls.Control)
            return Activator.CreateInstance(viewType) as Control;
        }

        public bool Match(object? data)
        {
            if (data == null)
                return false;

            // Возвращаем true только для тех объектов, у которых имя типа оканчивается на "ViewModel"
            // Таким образом этот IDataTemplate будет применяться к любому ViewModel.
            return data.GetType().Name.EndsWith("ViewModel", StringComparison.Ordinal);
        }
    }
}
