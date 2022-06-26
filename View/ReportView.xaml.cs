using Scheduler.ViewModel;

using System.Windows.Controls;

namespace Scheduler.View
{
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            this.DataContext = new ReportViewModel();
            InitializeComponent();
        }
    }
}