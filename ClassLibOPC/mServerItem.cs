using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClassLibOPC
{
    public class mServerItem : Entity
    {

        private bool _isConnected;
        public bool isConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsConnected"));
            }
        }
        
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Description"));
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        private string _UrlString;
        public string UrlString
        {
            get { return _UrlString; }
            set
            {
                _UrlString = value;

                OnPropertyChanged(new PropertyChangedEventArgs("UrlString"));
            }
        }

        private string _ServerState;
        public string ServerState
        {
            get { return _ServerState; }
            set
            {
                _ServerState = value;

                OnPropertyChanged(new PropertyChangedEventArgs("ServerState"));
            }
        }

        private string _StatusInfo;
        public string StatusInfo
        {
            get { return _StatusInfo; }
            set
            {
                _StatusInfo = value;

                OnPropertyChanged(new PropertyChangedEventArgs("StatusInfo"));
            }
        }

        private string _ProductVersion;
        public string ProductVersion
        {
            get { return _ProductVersion; }
            set
            {
                _ProductVersion = value;

                OnPropertyChanged(new PropertyChangedEventArgs("ProductVersion"));
            }
        }

        private string _VendorInfo;
        public string VendorInfo
        {
            get { return _VendorInfo; }
            set
            {
                _VendorInfo = value;

                OnPropertyChanged(new PropertyChangedEventArgs("VendorInfo"));
            }
        }


        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Host"));
            }
        }

        private bool _isReconnect;
        public bool isReconnect
        {
            get { return _isReconnect; }
            set
            {
                _isReconnect = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isReconnect"));
            }
        }

        private double _reconnectInterval;
        public double ReconnectInterval
        {
            get { return _reconnectInterval; }
            set
            {
                _reconnectInterval = value;

                OnPropertyChanged(new PropertyChangedEventArgs("ReconnectInterval"));
            }
        }

        /// <summary>
        /// Server item: name, url, status, state, produc, vendor, isconnected
        /// </summary>
        /// <param name="init">if true, set all fields to default values</param>
        public mServerItem(bool init)
        {
            if (init)
            {
                isConnected = false;
                Description = "unknown";
                Name = "unknown";
                UrlString = "";
                ServerState = "unknown";
                StatusInfo = "unknown";
                ProductVersion = "unknown";
                VendorInfo = "unknown";
                ReconnectInterval = 10000; // milisec
                Host = "localhost";
            }
        }

    }
}
