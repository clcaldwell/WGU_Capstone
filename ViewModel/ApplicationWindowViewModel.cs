using CommunityToolkit.Mvvm.Input;

namespace Scheduler.ViewModel
{
    public class ApplicationWindowViewModel : ViewModelBase
    {
        private readonly AppointmentViewModel _appointmentViewModel = new();
        private readonly CustomerViewModel _customerViewModel = new();
        private readonly ReminderViewModel _reminderViewModel = new();
        private readonly ReportViewModel _reportViewModel = new();
        private ViewModelBase _currentViewModel;

        public ApplicationWindowViewModel()
        {
            NavCommand = new RelayCommand<string>(OnNav);
            CurrentViewModel = _reminderViewModel;
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (value != _currentViewModel)
                {
                    SetProperty(ref _currentViewModel, value);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand<string> NavCommand { get; }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "Appointment":
                    CurrentViewModel = _appointmentViewModel;
                    break;

                case "Customer":
                    CurrentViewModel = _customerViewModel;
                    break;

                case "Report":
                    CurrentViewModel = _reportViewModel;
                    break;
            }
        }
    }
}