using Avalonia.Controls;
using System.Linq;

namespace Desktop.Dialogs
{
    public partial class RunHIstoryWindow : Window
    {
        public RunHIstoryWindow()
        {
            InitializeComponent();

            var context = new DesktopContext();

            DataGrid.Items = context.RunResults.ToList();
        }
    }
}
