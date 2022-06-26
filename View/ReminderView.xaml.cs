using Scheduler.ViewModel;

using System.Windows.Controls;

namespace Scheduler.View
{
    public partial class ReminderView : UserControl
    {
        public ReminderView()
        {
            this.DataContext = new ReminderViewModel();
            InitializeComponent();
        }
    }
}