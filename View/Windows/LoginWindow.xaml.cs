using Scheduler.ViewModel;

using System.Windows;

namespace Scheduler.View
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            this.DataContext = new LoginWindowViewModel();
            InitializeComponent();
        }
    }
}