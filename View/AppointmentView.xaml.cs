using Scheduler.ViewModel;

using System.Windows.Controls;

namespace Scheduler.View
{
    public partial class AppointmentView : UserControl
    {
        public AppointmentView()
        {
            this.DataContext = new AppointmentViewModel();
            InitializeComponent();
        }
    }
}