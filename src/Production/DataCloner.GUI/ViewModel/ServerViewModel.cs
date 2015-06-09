﻿using System;
using DataCloner.DataClasse.Configuration;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        private Int16 _id;
        private string _name;
        private string _providerName;
        private string _connectionString;

        public Int16 Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public string ProviderName
        {
            get { return _providerName; }
            set { Set("ProviderName", ref _providerName, value); }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { Set("Server", ref _connectionString, value); }
        }

        [PreferredConstructor]
        public ServerViewModel()
        {
            if (IsInDesignMode)
            {
                Id = 1;
                Name = "UNI";
                ProviderName = "System.Data.SqlClient";
                ConnectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=northwind;";
            }
        }

        public ServerViewModel(Connection connection)
        {
            Id = connection.Id;
            Name = connection.Name;
            ProviderName = connection.ProviderName;
            ConnectionString = connection.ConnectionString;
        }
    }
}
