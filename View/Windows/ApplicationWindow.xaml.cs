using Scheduler.ViewModel;

using System.Windows;

namespace Scheduler.View
{
    public partial class ApplicationWindow : Window
    {
        public ApplicationWindow()
        {
            this.DataContext = new ApplicationWindowViewModel();
            InitializeComponent();
        }
    }
}