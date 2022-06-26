using Scheduler.ViewModel;

using System.Windows.Controls;

namespace Scheduler.View
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            this.DataContext = new CustomerViewModel();
            InitializeComponent();
        }
    }
}