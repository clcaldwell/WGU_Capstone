using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;

using Scheduler.Exceptions;
using Scheduler.Model.DBEntities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Scheduler.ViewModel
{
    public class AppointmentViewModel : ViewModelBase
    {
        private ObservableCollection<Appointment> _allappointmentsloaded;
        private string _customerIdSearchOption;
        private string _customerIdSearchText;
        private int _customerIndex;
        private string _descriptionSearchOption;
        private string _descriptionSearchText;
        private bool _isAddMode = false;
        private bool _isAllCalendar;
        private bool _isCustomerIdSearchEnabled;
        private bool _isDescriptionSearchEnabled;
        private bool _isEditMode = false;
        private bool _isLocationSearchEnabled;
        private bool _isModifyAppointment;
        private bool _isMonthlyCalendar;
        private bool _isSearchCalendar;
        private bool _isTitleSearchEnabled;
        private bool _isTypeSearchEnabled;
        private bool _isViewMode = true;
        private bool _isWeeklyCalendar;
        private string _locationSearchOption;
        private string _locationSearchText;
        private ObservableCollection<Appointment> _monthlyAppointments;
        private ObservableCollection<Appointment> _searchAppointments;
        private Appointment _selectedappointment;
        private Customer _selectedcustomer;
        private string _selectedMonth = DateTime.Today.ToString("MMMM");
        private string _selectedSearchText;
        private string _selectedSearchType;
        private string _selectedYear = DateTime.Today.Year.ToString();
        private object _tabControlSelectedItem;
        private string _titleSearchOption;
        private string _titleSearchText;
        private string _typeSearchOption;
        private string _typeSearchText;
        private ObservableCollection<Appointment> _weeklyAppointments;

        public AppointmentViewModel()
        {
            AddAppointmentCommand = new RelayCommand<Appointment>(OnAddButton);
            EditAppointmentCommand = new RelayCommand<Appointment>(OnEditButton);
            DeleteAppointmentCommand = new RelayCommand<Appointment>(OnDeleteButton);

            SaveAppointmentCommand = new RelayCommand<Appointment>(OnSaveButton);
            CancelAppointmentCommand = new RelayCommand<Appointment>(OnCancelButton);

            ResetSearchFilterCommand = new RelayCommand(async () => await ResetSearchFilter());
        }

        private enum Mode
        {
            Add,
            Edit,
            View
        }

        public static List<Appointment> AllAppointments
        {
            get
            {
                var context = new DBContext();
                List<Appointment> appointments = context.Appointment.ToList();
                foreach (Appointment appointment in appointments)
                {
                    appointment.Start = appointment.Start.ToLocalTime();
                    appointment.End = appointment.End.ToLocalTime();
                }

                return appointments;
            }
            set
            {
                var context = new DBContext();
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
        }

        public static ObservableCollection<string> Months => new()
        {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };

        public static ObservableCollection<string> SearchOptions => new()
        {
            " ",
            "Matches Regex",
            "Does Not Match Regex",
            "Contains",
            "Does Not Contain"
        };

        public static ObservableCollection<string> SearchTypes => new()
        {
            "CustomerId",
            "Appt Type",
            "Title",
            "Location",
            "Description"
        };

        public static ObservableCollection<string> Years
        {
            get
            {
                List<string> years = new();
                for (int i = -3; i < 3; i++)
                {
                    years.Add(
                        DateTime.Today.AddYears(i).Year.ToString()
                    );
                }

                return new ObservableCollection<string>(years);
            }
        }

        public RelayCommand<Appointment> AddAppointmentCommand { get; }

        public ObservableCollection<Appointment> AllAppointmentsLoaded
        {
            get => _allappointmentsloaded;
            set
            {
                if (value != _allappointmentsloaded)
                {
                    SetProperty(ref _allappointmentsloaded, value);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand<Appointment> CancelAppointmentCommand { get; }

        public string CustomerIdSearchOption
        {
            get => _customerIdSearchOption;
            set
            {
                if (value != _customerIdSearchOption)
                {
                    SetProperty(ref _customerIdSearchOption, value);
                    OnPropertyChanged();
                    SearchOptionChange("CustomerId", value);
                }
            }
        }

        public string CustomerIdSearchText
        {
            get => _customerIdSearchText;
            set
            {
                if (value != _customerIdSearchText)
                {
                    SetProperty(ref _customerIdSearchText, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public int CustomerIndex
        {
            get => _customerIndex;
            set
            {
                if (value != _customerIndex)
                {
                    SetProperty(ref _customerIndex, value);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand<Appointment> DeleteAppointmentCommand { get; }

        public string DescriptionSearchOption
        {
            get => _descriptionSearchOption;
            set
            {
                if (value != _descriptionSearchOption)
                {
                    SetProperty(ref _descriptionSearchOption, value);
                    OnPropertyChanged();
                    SearchOptionChange("Description", value);
                }
            }
        }

        public string DescriptionSearchText
        {
            get => _descriptionSearchText;
            set
            {
                if (value != _descriptionSearchText)
                {
                    SetProperty(ref _descriptionSearchText, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public RelayCommand<Appointment> EditAppointmentCommand { get; }

        public RelayCommand<string> GetAppointmentsCommand { get; }

        public bool IsAddMode
        {
            get => _isAddMode;
            set
            {
                if (value != _isAddMode)
                {
                    SetProperty(ref _isAddMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAllCalendar
        {
            get => _isAllCalendar;
            set
            {
                if (value != _isAllCalendar)
                {
                    SetProperty(ref _isAllCalendar, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCustomerIdSearchEnabled
        {
            get => _isCustomerIdSearchEnabled;
            set
            {
                if (value != _isCustomerIdSearchEnabled)
                {
                    SetProperty(ref _isCustomerIdSearchEnabled, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDescriptionSearchEnabled
        {
            get => _isDescriptionSearchEnabled;
            set
            {
                if (value != _isDescriptionSearchEnabled)
                {
                    SetProperty(ref _isDescriptionSearchEnabled, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (value != _isEditMode)
                {
                    SetProperty(ref _isEditMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLocationSearchEnabled
        {
            get => _isLocationSearchEnabled;
            set
            {
                if (value != _isLocationSearchEnabled)
                {
                    SetProperty(ref _isLocationSearchEnabled, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsModifyAppointment
        {
            get => _isModifyAppointment;
            set
            {
                if (value != _isModifyAppointment)
                {
                    SetProperty(ref _isModifyAppointment, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMonthlyCalendar
        {
            get => _isMonthlyCalendar;
            set
            {
                if (value != _isMonthlyCalendar)
                {
                    SetProperty(ref _isMonthlyCalendar, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSearchCalendar
        {
            get => _isSearchCalendar;
            set
            {
                if (value != _isSearchCalendar)
                {
                    SetProperty(ref _isSearchCalendar, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTitleSearchEnabled
        {
            get => _isTitleSearchEnabled;
            set
            {
                if (value != _isTitleSearchEnabled)
                {
                    SetProperty(ref _isTitleSearchEnabled, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTypeSearchEnabled
        {
            get => _isTypeSearchEnabled;
            set
            {
                if (value != _isTypeSearchEnabled)
                {
                    SetProperty(ref _isTypeSearchEnabled, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsViewMode
        {
            get => _isViewMode;
            set
            {
                if (value != _isViewMode)
                {
                    SetProperty(ref _isViewMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWeeklyCalendar
        {
            get => _isWeeklyCalendar;
            set
            {
                if (value != _isWeeklyCalendar)
                {
                    SetProperty(ref _isWeeklyCalendar, value);
                    OnPropertyChanged();
                }
            }
        }

        public string LocationSearchOption
        {
            get => _locationSearchOption;
            set
            {
                if (value != _locationSearchOption)
                {
                    SetProperty(ref _locationSearchOption, value);
                    OnPropertyChanged();
                    SearchOptionChange("Location", value);
                }
            }
        }

        public string LocationSearchText
        {
            get => _locationSearchText;
            set
            {
                if (value != _locationSearchText)
                {
                    SetProperty(ref _locationSearchText, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public ObservableCollection<Appointment> MonthlyAppointments
        {
            get => _monthlyAppointments; set
            {
                if (value != _monthlyAppointments)
                {
                    SetProperty(ref _monthlyAppointments, value);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand ResetSearchFilterCommand { get; }

        public RelayCommand<Appointment> SaveAppointmentCommand { get; }

        public ObservableCollection<Appointment> SearchAppointments
        {
            get => _searchAppointments;
            set
            {
                if (value != _searchAppointments)
                {
                    SetProperty(ref _searchAppointments, value);
                    OnPropertyChanged();
                }
            }
        }

        public Appointment SelectedAppointment
        {
            get => _selectedappointment;
            set
            {
                if (value != null && value != _selectedappointment)
                {
                    value.Start = value.Start.ToLocalTime();
                    value.End = value.End.ToLocalTime();

                    SetProperty(ref _selectedappointment, value);
                    OnPropertyChanged();

                    DBContext context = new();
                    SelectedCustomer = context.Customer.Find(value.CustomerId);
                }
            }
        }

        public Customer SelectedCustomer
        {
            get => _selectedcustomer;
            set
            {
                if (value != null && value != _selectedcustomer)
                {
                    SetProperty(ref _selectedcustomer, value);
                    OnPropertyChanged();

                    if (SelectedAppointment != null && SelectedAppointment.CustomerId != value.CustomerId)
                    {
                        SelectedAppointment.CustomerId = value.CustomerId;
                    }
                }
            }
        }

        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (value != _selectedMonth)
                {
                    SetProperty(ref _selectedMonth, value);
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedSearchText
        {
            get => _selectedSearchText;
            set
            {
                if (value != null && value != _selectedSearchText)
                {
                    SetProperty(ref _selectedSearchText, value);
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                if (value != null && value != _selectedSearchType)
                {
                    SetProperty(ref _selectedSearchType, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public string SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (value != _selectedYear)
                {
                    SetProperty(ref _selectedYear, value);
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand<Appointment> SetCalendarByMonthCommand { get; }

        public RelayCommand SetCalendarBySearchCommand { get; }

        public RelayCommand<Appointment> SetCalendarByWeekCommand { get; }

        public RelayCommand<Appointment> SetGridCommand { get; }

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

        public string TitleSearchOption
        {
            get => _titleSearchOption;
            set
            {
                if (value != _titleSearchOption)
                {
                    SetProperty(ref _titleSearchOption, value);
                    OnPropertyChanged();
                    SearchOptionChange("Title", value);
                }
            }
        }

        public string TitleSearchText
        {
            get => _titleSearchText;
            set
            {
                if (value != _titleSearchText)
                {
                    SetProperty(ref _titleSearchText, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public string TypeSearchOption
        {
            get => _typeSearchOption;
            set
            {
                if (value != _typeSearchOption)
                {
                    SetProperty(ref _typeSearchOption, value);
                    OnPropertyChanged();
                    SearchOptionChange("Type", value);
                }
            }
        }

        public string TypeSearchText
        {
            get => _typeSearchText;
            set
            {
                if (value != _typeSearchText)
                {
                    SetProperty(ref _typeSearchText, value);
                    OnPropertyChanged();
                    InvokeFilter();
                }
            }
        }

        public ObservableCollection<Appointment> WeeklyAppointments
        {
            get => _weeklyAppointments;
            set
            {
                if (value != _weeklyAppointments)
                {
                    SetProperty(ref _weeklyAppointments, value);
                    OnPropertyChanged();
                }
            }
        }

        public static void CheckIfAppointmentOverlapping(Appointment appointment)
        {
            foreach (Appointment existingAppt in AllAppointments.Where(appt => appt.AppointmentId != appointment.AppointmentId))
            {
                string message =
                    $"The start of the appointment conflicts with\r\n" +
                    "an existing appointment. Please correct the time.\r\n" +
                    $"Existing Appointment: {existingAppt.Start.ToLocalTime()} - {existingAppt.End.ToLocalTime()}";

                DateTime newApptStart = appointment.Start.ToUniversalTime();
                DateTime newApptEnd = appointment.End.ToUniversalTime();
                DateTime existingApptStart = existingAppt.Start.ToUniversalTime();
                DateTime existingApptEnd = existingAppt.End.ToUniversalTime();

                if ((existingApptStart < newApptStart) && (existingApptEnd > newApptStart))
                {
                    throw (new OverlappingAppointmentException(message));
                }
                if ((existingApptStart < newApptEnd) && (existingApptEnd > newApptStart))
                {
                    throw (new OverlappingAppointmentException(message));
                }
            }
        }

        public async void LoadAppointments()
        {
            DBContext context = new();
            List<Appointment> appointments = await context.Appointment.ToListAsync();
            foreach (Appointment appointment in appointments)
            {
                appointment.Start = appointment.Start.ToLocalTime();
                appointment.End = appointment.End.ToLocalTime();
            }
            AllAppointmentsLoaded = new ObservableCollection<Appointment>(appointments);
            SearchAppointments = new ObservableCollection<Appointment>(appointments);

            DateTime Now = DateTime.Now.ToLocalTime();

            WeeklyAppointments = new ObservableCollection<Appointment>(
                appointments
                .Where(appt => ISOWeek.GetWeekOfYear(appt.Start) == ISOWeek.GetWeekOfYear(Now) && appt.Start.Year == Now.Year)
            );

            MonthlyAppointments = new ObservableCollection<Appointment>(
                appointments
                .Where(appt => appt.Start.Month == Now.Month && appt.Start.Year == Now.Year)
            );
        }

        private async Task InvokeFilter()
        {
            SetMode(Mode.View);

            List<Appointment> appointments = AllAppointments;

            if (IsCustomerIdSearchEnabled)
            {
                switch (CustomerIdSearchOption)
                {
                    case "Matches Regex":
                        appointments.RemoveAll(appt =>
                            !Regex.IsMatch(appt.CustomerId.ToString(), CustomerIdSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Does Not Match Regex":
                        appointments.RemoveAll(appt =>
                            Regex.IsMatch(appt.CustomerId.ToString(), CustomerIdSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Contains":
                        appointments.RemoveAll(appt =>
                            !appt.CustomerId.ToString().Contains(CustomerIdSearchText, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Does Not Contain":
                        appointments.RemoveAll(appt =>
                            appt.CustomerId.ToString().Contains(CustomerIdSearchText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }
            if (IsTypeSearchEnabled)
            {
                switch (TypeSearchOption)
                {
                    case "Matches Regex":
                        appointments.RemoveAll(appt =>
                            !Regex.IsMatch(appt.Type, TypeSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Does Not Match Regex":
                        appointments.RemoveAll(appt =>
                            Regex.IsMatch(appt.Type, TypeSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Contains":
                        appointments.RemoveAll(appt =>
                            !appt.Type.Contains(TypeSearchText, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Does Not Contain":
                        appointments.RemoveAll(appt =>
                            appt.Type.Contains(TypeSearchText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }
            if (IsTitleSearchEnabled)
            {
                switch (TitleSearchOption)
                {
                    case "Matches Regex":
                        appointments.RemoveAll(appt =>
                            !Regex.IsMatch(appt.Title, TitleSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Does Not Match Regex":
                        appointments.RemoveAll(appt =>
                            Regex.IsMatch(appt.Title, TitleSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Contains":
                        appointments.RemoveAll(appt =>
                            !appt.Title.Contains(TitleSearchText, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Does Not Contain":
                        appointments.RemoveAll(appt =>
                            appt.Title.Contains(TitleSearchText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }
            if (IsLocationSearchEnabled)
            {
                switch (LocationSearchOption)
                {
                    case "Matches Regex":
                        appointments.RemoveAll(appt =>
                            !Regex.IsMatch(appt.Location, LocationSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Does Not Match Regex":
                        appointments.RemoveAll(appt =>
                            Regex.IsMatch(appt.Location, LocationSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Contains":
                        appointments.RemoveAll(appt =>
                            !appt.Location.Contains(LocationSearchText, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Does Not Contain":
                        appointments.RemoveAll(appt =>
                            appt.Location.Contains(LocationSearchText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }
            if (IsDescriptionSearchEnabled)
            {
                switch (DescriptionSearchOption)
                {
                    case "Matches Regex":
                        appointments.RemoveAll(appt =>
                            !Regex.IsMatch(appt.Description, DescriptionSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Does Not Match Regex":
                        appointments.RemoveAll(appt =>
                            Regex.IsMatch(appt.Description, DescriptionSearchText, RegexOptions.IgnoreCase));
                        break;

                    case "Contains":
                        appointments.RemoveAll(appt =>
                            !appt.Description.Contains(DescriptionSearchText, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Does Not Contain":
                        appointments.RemoveAll(appt =>
                            appt.Description.Contains(DescriptionSearchText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }

            SearchAppointments = new ObservableCollection<Appointment>(appointments);

            //if (!string.IsNullOrWhiteSpace(SelectedSearchText) && !string.IsNullOrWhiteSpace(SelectedSearchType))
            //{
            //    switch (SelectedSearchType)
            //    {
            //        case "CustomerId":
            //            appointments.AddRange(
            //                AllAppointments
            //                    .Where(appt => appt.CustomerId.ToString().Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            //            break;

            //        case "Type":
            //            appointments.AddRange(
            //                AllAppointments
            //                    .Where(appt => appt.Type.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            //            break;

            //        case "Title":
            //            appointments.AddRange(
            //                AllAppointments
            //                    .Where(appt => appt.Title.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            //            break;

            //        case "Location":
            //            appointments.AddRange(
            //                AllAppointments
            //                    .Where(appt => appt.Location.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            //            break;

            //        case "Description":
            //            appointments.AddRange(
            //                AllAppointments
            //                    .Where(appt => appt.Description.Contains(SelectedSearchText, StringComparison.OrdinalIgnoreCase)).ToList());
            //            break;
            //    }
            //}

            //SearchAppointments = new ObservableCollection<Appointment>(appointments);
        }

        private void OnAddButton(Appointment appointment)
        {
            SetMode(Mode.Add);
            if (appointment == null)
            {
                SelectedAppointment = AllAppointments.FirstOrDefault();
            }
        }

        private void OnCancelButton(Appointment appointment)
        {
            LoadAppointments();
            SetMode(Mode.View);
        }

        private void OnDeleteButton(Appointment appointment)
        {
            if (MessageBox.Show("Are you sure you want to delete this appointment?" +
                    "\r\n Id:" + appointment.AppointmentId +
                    "\r\n Title:" + appointment.Title +
                    "\r\n Location:" + appointment.Location +
                    "\r\n Contact:" + appointment.Contact +
                    "\r\n Start:" + appointment.Start.ToLocalTime().ToString() +
                    "\r\n End:" + appointment.End.ToLocalTime().ToString(),
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var context = new DBContext();
                context.Remove(appointment);
                context.SaveChanges();
                SetMode(Mode.View);
                SearchAppointments?.Remove(appointment);
                LoadAppointments();
            }
        }

        private void OnEditButton(Appointment appointment)
        {
            SetMode(Mode.Edit);
        }

        private void OnSaveButton(Appointment appointment)
        {
            try
            {
                CheckIfAppointmentOverlapping(appointment);
            }
            catch (OverlappingAppointmentException e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            var context = new DBContext();
            if (IsAddMode)
            {
                int NextId = AllAppointments.OrderByDescending(a => a.AppointmentId).FirstOrDefault().AppointmentId + 1;
                Appointment NewAppointment = new()
                {
                    //AppointmentId = NextId,
                    CustomerId = appointment.CustomerId,
                    UserId = appointment.UserId,
                    Title = appointment.Title,
                    Description = appointment.Description,
                    Location = appointment.Location,
                    Contact = appointment.Contact,
                    Type = appointment.Type,
                    Url = appointment.Url,
                    Start = appointment.Start.ToUniversalTime(),
                    End = appointment.End.ToUniversalTime(),
                    CreateDate = appointment.CreateDate.ToUniversalTime(),
                    CreatedBy = appointment.CreatedBy,
                    LastUpdate = appointment.LastUpdate.ToUniversalTime(),
                    LastUpdateBy = appointment.LastUpdateBy
                };

                // Set any string null properties to empty string
                foreach (var propertyInfo in NewAppointment.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof(string) && propertyInfo.GetValue(NewAppointment, null) == null)
                    {
                        propertyInfo.SetValue(NewAppointment, string.Empty, null);
                    }
                }

                context.Add(NewAppointment);
            }
            else
            {
                appointment.Start = appointment.Start.ToUniversalTime();
                appointment.End = appointment.End.ToUniversalTime();
                context.Update(appointment);
            }

            context.SaveChanges();
            LoadAppointments();
            SetMode(Mode.View);
        }

        private async Task ResetSearchFilter()
        {
            CustomerIdSearchText = null;
            TypeSearchText = null;
            TitleSearchText = null;
            LocationSearchText = null;
            DescriptionSearchText = null;

            CustomerIdSearchOption = null;
            TypeSearchOption = null;
            TitleSearchOption = null;
            LocationSearchOption = null;
            DescriptionSearchOption = null;
        }

        private async Task SearchOptionChange(string field, string value)
        {
            switch (field)
            {
                case "CustomerId":
                    IsCustomerIdSearchEnabled = !string.IsNullOrWhiteSpace(value);
                    if (!IsCustomerIdSearchEnabled) { CustomerIdSearchText = null; }
                    break;

                case "Type":
                    IsTypeSearchEnabled = !string.IsNullOrWhiteSpace(value);
                    if (!IsTypeSearchEnabled) { TypeSearchText = null; }
                    break;

                case "Title":
                    IsTitleSearchEnabled = !string.IsNullOrWhiteSpace(value);
                    if (!IsTitleSearchEnabled) { TitleSearchText = null; }
                    break;

                case "Location":
                    IsLocationSearchEnabled = !string.IsNullOrWhiteSpace(value);
                    if (!IsLocationSearchEnabled) { LocationSearchText = null; }
                    break;

                case "Description":
                    IsDescriptionSearchEnabled = !string.IsNullOrWhiteSpace(value);
                    if (!IsDescriptionSearchEnabled) { DescriptionSearchText = null; }
                    break;
            }

            await InvokeFilter();
        }

        private void SetMode(Mode mode)
        {
            if (mode == Mode.Add)
            {
                IsAddMode = true;
                IsEditMode = true;
                IsViewMode = false;
                SelectedAppointment = null;
                IsModifyAppointment = true;
            }
            if (mode == Mode.Edit)
            {
                IsEditMode = true;
                IsViewMode = false;
                IsModifyAppointment = true;
            }
            if (mode == Mode.View)
            {
                IsEditMode = false;
                IsAddMode = false;
                IsViewMode = true;
            }
        }
    }
}