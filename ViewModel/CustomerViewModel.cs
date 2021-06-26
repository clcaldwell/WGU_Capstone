﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using GalaSoft.MvvmLight.CommandWpf;

using Microsoft.EntityFrameworkCore;

using Scheduler.Model.DBEntities;

namespace Scheduler.ViewModel
{
    public class CustomerViewModel : ViewModelBase
    {
        private List<Customer> _allcustomersloaded;

        private Customer _selectedcustomer;
        private Address _selectedaddress;
        private City _selectedcity;
        private Country _selectedcountry;
        private Country _selectedCountryItem;
        private City _selectedCityItem;
        private Address _selectedAddressItem;

        private bool _addMode = false;
        private bool _editMode = false;
        private bool _viewMode = true;
        private object _tabControlSelectedItem;
        private bool _customerTabSelected;
        private bool _addressTabSelected;

        enum Mode
        {
            Add,
            Edit,
            View
        }

        private void SetMode(Mode mode)
        {
            if (mode == Mode.Add)
            {
                AddMode = true;
                EditMode = true;
                ViewMode = false;

                if (SelectedCustomer == null)
                {
                    SelectedCustomer = AllCustomers.FirstOrDefault();
                }
            }
            if (mode == Mode.Edit)
            {
                EditMode = true;
                ViewMode = false;
            }
            if (mode == Mode.View)
            {
                AddMode = false;
                EditMode = false;
                ViewMode = true;
            }
        }

        private void OnAddButton(Customer customer)
        {
            SetMode(Mode.Add);
        }

        private void OnEditButton(Customer customer)
        {
            SetMode(Mode.Edit);
        }

        private void OnDeleteButton(Customer customer)
        {
            if (MessageBox.Show("Are you sure you want to delete this customer?" +
                    "\r\n Id:" + customer.CustomerId +
                    "\r\n Name:" + customer.CustomerName,
                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var context = new DBContext();
                context.Remove(customer);
                context.SaveChanges();
                SetMode(Mode.View);
                LoadCustomers();
            }
        }

        private void OnSaveButton(Customer customer)
        {
            var context = new DBContext();
            if (AddMode)
            {
                int NextId = AllCustomers.
                    OrderByDescending(a => a.CustomerId).FirstOrDefault().CustomerId + 1;

                Customer NewCustomer = new Customer
                {
                    CustomerId = NextId,
                    CustomerName = customer.CustomerName,
                    AddressId = customer.AddressId,
                    Active = customer.Active,
                    Address = customer.Address,
                    CreateDate = DateTime.Now.ToUniversalTime(),
                    CreatedBy = customer.CreatedBy,
                    LastUpdate = DateTime.Now.ToUniversalTime(),
                    LastUpdateBy = customer.LastUpdateBy

                }; 
                context.Add(NewCustomer);
            }
            else
            {
                //customer.LastUpdate = DateTime.Now.ToUniversalTime();

                context.Update(customer);
                //context.Update(SelectedAddress);
                //context.Update(SelectedCity);
                //context.Update(SelectedCountry);
            }

            context.SaveChanges();
            LoadCustomers();
            SetMode(Mode.View);
        }

        private void OnCancelButton(Customer customer)
        {
            LoadCustomers();
            SetMode(Mode.View);
        }

        public CustomerViewModel()
        {
            AddCustomerCommand = new RelayCommand<Customer>(OnAddButton);
            EditCustomerCommand = new RelayCommand<Customer>(OnEditButton);
            DeleteCustomerCommand = new RelayCommand<Customer>(OnDeleteButton);

            SaveCustomerCommand = new RelayCommand<Customer>(OnSaveButton);
            CancelCustomerCommand = new RelayCommand<Customer>(OnCancelButton);
        }

        public RelayCommand<string> GetCustomersCommand { get; private set; }
        public RelayCommand<Customer> AddCustomerCommand { get; private set; }
        public RelayCommand<Customer> EditCustomerCommand { get; private set; }
        public RelayCommand<Customer> DeleteCustomerCommand { get; private set; }
        public RelayCommand<Customer> SaveCustomerCommand { get; private set; }
        public RelayCommand<Customer> CancelCustomerCommand { get; private set; }

        public bool AddMode
        {
            get => _addMode;
            set
            {
                if (value != _addMode)
                {
                    SetProperty(ref _addMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool ViewMode
        {
            get => _viewMode;
            set
            {
                if (value != _viewMode)
                {
                    SetProperty(ref _viewMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool EditMode
        {
            get => _editMode;
            set
            {
                if (value != _editMode)
                {
                    SetProperty(ref _editMode, value);
                    OnPropertyChanged();
                }
            }
        }

        public List<Customer> AllCustomers
        {
            get
            {
                var context = new DBContext();
                return context.Customer.ToList();
            }
            set
            {
                var context = new DBContext();
                context.Customer.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public List<Customer> AllCustomersLoaded
        {
            get => _allcustomersloaded;
            set
            {
                if (value != _allcustomersloaded)
                {
                    SetProperty(ref _allcustomersloaded, value);
                    OnPropertyChanged();
                }
            }
        }

        public async void LoadCustomers()
        {
            var context = new DBContext();
            AllCustomersLoaded = await context.Customer.ToListAsync();
        }

        //public async void LoadAddresses()
        //{
            //var context = new DBContext();
            //AllAddresses = await context.Address.ToListAsync();
        //}

        //public async void LoadCities()
        //{
            //var context = new DBContext();
            //AllCities = await context.City.ToListAsync();
        //}

        //public async void LoadCountries()
        //{
            //var context = new DBContext();
            //AllCountries = await (IEnumerable<Country>)context.Country.ToListAsync();
        //}

        public Customer SelectedCustomer
        {
            get => _selectedcustomer;
            set
            {
                if (value != null && value != _selectedcustomer)
                {
                    var context = new DBContext();
                    value.Address = context.Address.Find(value.AddressId);
                    value.Address.City = context.City.Find(value.Address.CityId);
                    value.Address.City.Country = context.Country.Find(value.Address.City.CountryId);

                    SetProperty(ref _selectedcustomer, value);

                    //SelectedAddress = context.Address.Find(value.AddressId);

                    OnPropertyChanged();
                }
            }
        }

        public Address SelectedAddress
        {
            get => _selectedaddress;
            set
            {
                if (value != null && value != _selectedaddress)
                {
                    var context = new DBContext();
                    value.City = context.City.Find(value.CityId);
                    value.City.Country = context.Country.Find(value.City.CountryId);

                    SetProperty(ref _selectedaddress, value);

                    SelectedCity = context.City.Find(value.CityId);

                    OnPropertyChanged();
                }
            }
        }

        public City SelectedCity
        {
            get => _selectedcity;
            set
            {
                if (value != null && value != _selectedcity)
                {
                    SetProperty(ref _selectedcity, value);

                    var context = new DBContext();
                    SelectedCountry = context.Country.Find(value.CountryId);

                    OnPropertyChanged();
                }
            }
        }

        public Country SelectedCountry
        {
            get => _selectedcountry;
            set
            {
                if (value != null && value != _selectedcountry)
                {
                    SetProperty(ref _selectedcountry, value);
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Address> AllAddresses
        {
            get
            {
                var context = new DBContext();
                return new ObservableCollection<Address>(context.Address.ToList());
            }
            set
            {
                var context = new DBContext();
                context.Address.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public ObservableCollection<City> AllCities
        {
            get
            {
                var context = new DBContext();
                return new ObservableCollection<City>(context.City.ToList());
            }
            set
            {
                var context = new DBContext();
                context.City.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public ObservableCollection<Country> AllCountries

        {
            get
            {
                var context = new DBContext();
                return new ObservableCollection<Country>(context.Country.ToList());
            }
            set
            {
                var context = new DBContext();
                context.Country.UpdateRange(value.ToList());
                context.SaveChanges();
            }
        }

        public Country SelectedCountryItem
        {
            get => _selectedCountryItem;
            set
            {
                if (value != null && value != _selectedCountryItem)
                {
                    SetProperty(ref _selectedCountryItem, value);
                    OnPropertyChanged();
                }
            }
        }

        public City SelectedCityItem
        {
            get => _selectedCityItem;
            set
            {
                if (value != null && value != _selectedCityItem)
                {
                    SetProperty(ref _selectedCityItem, value);
                    OnPropertyChanged();
                }
            }
        }

        public Address SelectedAddressItem
        {
            get => _selectedAddressItem;
            set
            {
                if (value != null && value != _selectedAddressItem)
                {
                    SetProperty(ref _selectedAddressItem, value);
                    OnPropertyChanged();
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

        public bool CustomerTabSelected
        {
            get => _customerTabSelected;
            set
            {
                if (value != _customerTabSelected)
                {
                    SetProperty(ref _customerTabSelected, value);
                    OnPropertyChanged();
                }
            }
        }

        public bool AddressTabSelected
        {
            get => _addressTabSelected;
            set
            {
                if (value != _addressTabSelected)
                {
                    SetProperty(ref _addressTabSelected, value);
                    OnPropertyChanged();
                }
            }
        }

    }
}
