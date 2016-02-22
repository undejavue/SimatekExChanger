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
        public bool IsConnected
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



    }
}
