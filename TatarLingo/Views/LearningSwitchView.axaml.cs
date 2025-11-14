using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using TatarLingo.Models;

namespace TatarLingo.Views
{
    public partial class LearningSwitchView : UserControl
    {
        public LearningSwitchView()
        {
            InitializeComponent();
            PassedModules();
        }
        public void PassedModules()
        {
            var user = UserSession.CurrentUser!;
    
            var modules = new List<(Border border, bool passed)>
            {
                (Module1Grid, user.Module1Passed),
                (Module2Grid, user.Module2Passed),
                (Module3Grid, user.Module3Passed),
                (Module4Grid, user.Module4Passed),
                (Module5Grid, user.Module5Passed),
                (FinalTestGrid, user.FinalTestPassed)
            };

            int firstUnpassedIndex = modules.FindIndex(m => !m.passed);

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i].passed)
                {
                    modules[i].border.BorderBrush = new SolidColorBrush(Color.Parse("#009035")); // зелёный
                }
                else if (i == firstUnpassedIndex)
                {
                    modules[i].border.BorderBrush = new SolidColorBrush(Color.Parse("#DCDCDC")); // серый
                }
                else
                {
                    modules[i].border.BorderBrush = new SolidColorBrush(Color.Parse("#FF0000")); // красный
                }
            }
        }
    }
}