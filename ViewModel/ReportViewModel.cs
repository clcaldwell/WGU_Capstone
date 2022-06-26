using Scheduler.Model;
using Scheduler.Model.DBEntities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.ViewModel
{
    public class ReportViewModel : ViewModelBase
    {
        private ObservableCollection<ConsultantReportModel> _consultantReport;
        private bool _consultantReportSelected;
        private bool _customReportSelected;
        private string _fraudReport;
        private bool _fraudReportSelected;
        private bool _isDateFilter;
        private ObservableCollection<MonthlyReportModel> _monthlyReport;
        private bool _monthlyReportSelected;
        private object _tabControlSelectedItem;

        public static ObservableCollection<Appointment> AllAppointments
        {
            get
            {
                DBContext context = new();
                List<Appointment> appointments = context.Appointment.ToList();
                foreach (Appointment appointment in appointments)
                {
                    appointment.Start = appointment.Start.ToLocalTime();
                    appointment.End = appointment.End.ToLocalTime();
                }

                return new ObservableCollection<Appointment>(appointments);
            }
            set
            {
                DBContext context = new();
                context.Appointment.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public static ObservableCollection<Customer> AllCustomers
        {
            get
            {
                DBContext context = new();
                return new ObservableCollection<Customer>(context.Customer.ToList());
            }
            set
            {
                DBContext context = new();
                context.Customer.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public static ObservableCollection<User> AllUsers
        {
            get
            {
                DBContext context = new();
                return new ObservableCollection<User>(context.User.ToList());
            }
            set
            {
                DBContext context = new();
                context.User.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public ObservableCollection<ConsultantReportModel> ConsultantReport
        {
            get { return _consultantReport; }
            set
            {
                SetProperty(ref _consultantReport, value);
                OnPropertyChanged();
            }
        }

        public bool ConsultantReportSelected
        {
            get => _consultantReportSelected;
            set
            {
                if (value != _consultantReportSelected)
                {
                    SetProperty(ref _consultantReportSelected, value);
                    OnPropertyChanged();
                    GenerateConsultantSchedule();
                }
            }
        }

        public ObservableCollection<string> Consultants
        {
            get
            {
                ObservableCollection<string> consultants = new();
                foreach (var consultant in AllUsers)
                {
                    consultants.Add(consultant.UserName);
                }
                return consultants;
            }
        }

        public bool CustomReportSelected
        {
            get => _customReportSelected;
            set
            {
                if (value != _customReportSelected)
                {
                    SetProperty(ref _customReportSelected, value);
                    OnPropertyChanged();
                }
            }
        }

        public string FraudReport
        {
            get => _fraudReport;
            set
            {
                if (value != _fraudReport)
                {
                    SetProperty(ref _fraudReport, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool FraudReportSelected
        {
            get => _fraudReportSelected;
            set
            {
                if (value != _fraudReportSelected)
                {
                    SetProperty(ref _fraudReportSelected, value);
                    OnPropertyChanged();
                    GenerateFraudReport();
                }
            }
        }

        public ObservableCollection<MonthlyReportModel> MonthlyReport
        {
            get { return _monthlyReport; }
            set
            {
                SetProperty(ref _monthlyReport, value);
                OnPropertyChanged();
            }
        }

        public bool MonthlyReportSelected
        {
            get => _monthlyReportSelected;
            set
            {
                if (value != _monthlyReportSelected)
                {
                    SetProperty(ref _monthlyReportSelected, value);
                    OnPropertyChanged();
                    GenerateMonthlyReport();
                }
            }
        }

        public object TabControlSelectedItem
        {
            get => _tabControlSelectedItem;
            set
            {
                if (value != _tabControlSelectedItem)
                {
                    SetProperty(ref _tabControlSelectedItem, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDateFilter
        {
            get => _isDateFilter;
            set
            {
                if (value != _isDateFilter)
                {
                    SetProperty(ref _isDateFilter, value);
                    OnPropertyChanged();
                    GenerateMonthlyReport();
                }
            }
        }

        private async Task GenerateConsultantSchedule()
        {
            List<ConsultantReportModel> consultantReport = new();

            foreach (User consultant in AllUsers)
            {
                AllAppointments.Where(appt => appt.UserId == consultant.UserId)
                    .OrderBy(appt => appt.Start).ToList()
                    .ForEach(
                        appt => consultantReport.Add(
                            new ConsultantReportModel()
                            {
                                Consultant = consultant.UserName,
                                Appointment = appt.Start,
                                AppointmentType = appt.Type,
                                CustomerName =
                                    AllCustomers.FirstOrDefault(cust => appt.CustomerId == cust.CustomerId).CustomerName
                            }
                        )
                    );
                ConsultantReport = new ObservableCollection<ConsultantReportModel>(consultantReport);
            }
        }

        private async Task GenerateCustomReport()
        {
        }

        private async Task GenerateFraudReport()
        {
            StringBuilder text = new();
            text.AppendLine("Fraud Detection: Customers with Most Lunch appointments (All Time)");
            text.AppendLine("");

            int counter = 0;
            Customer frequentCustomer = null;
            foreach (Customer customer in AllCustomers)
            {
                int currentCount = AllAppointments.Count(appt => appt.CustomerId == customer.CustomerId);
                if (currentCount > counter)
                {
                    counter = currentCount;
                    frequentCustomer = customer;
                }
            }

            text.Append("Number of Lunches:\t").Append(counter).AppendLine();
            text.Append("Frequent Customer:\t").AppendLine(frequentCustomer.CustomerName);

            IEnumerable<Appointment> listOfFrequentLunches = AllAppointments
                .Where(appt => appt.CustomerId == frequentCustomer.CustomerId)
                .OrderBy(appt => appt.Start.Date);

            foreach (Appointment appt in listOfFrequentLunches)
            {
                text.Append("Date:\t").AppendFormat("{0:MM/dd/yyyy}", appt.Start.Date).AppendLine();
            }

            FraudReport = text.ToString();
        }

        private async Task GenerateMonthlyReport()
        {
            DateTime thisMonth = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime previousMonth = thisMonth.AddMonths(-1);
            DateTime nextMonth = thisMonth.AddMonths(2).AddMilliseconds(-1);
            List<int> months = new()
            {
                previousMonth.Month,
                thisMonth.Month,
                nextMonth.Month
            };

            List<MonthlyReportModel> monthlyReport = new();

            List<Appointment> currentAppointments = AllAppointments.Where(appt =>
                appt.Start.Month >= previousMonth.Month && appt.Start.Month <= nextMonth.Month)
                .OrderBy(appt => appt.Start).ToList();

            foreach (int month in months)
            {
                // Lambda: This lambda lets me do this logic concisely, instead of having
                // to do the extended version of the logic over a dozen lines.
                // This is much more readable and concise with the lambda.
                var counts = currentAppointments
                    .Where(appt => appt.Start.Month == month)
                    .GroupBy(appt => appt.Type)
                    .Select(appt => new { Value = appt.Key, Count = appt.Count() });

                foreach (var currentCount in counts)
                {
                    monthlyReport.Add(
                        new MonthlyReportModel()
                        {
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                            AppointmentType = currentCount.Value,
                            AppointmentTypeCount = currentCount.Count
                        }
                );
                }
            }

            MonthlyReport = new ObservableCollection<MonthlyReportModel>(monthlyReport);
        }
    }
}