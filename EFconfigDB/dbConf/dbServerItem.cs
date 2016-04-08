using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EFlocalDB
{
    public class dbServerItem : Entity
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        private string _opcHost;
        public string opcHost
        {
            get { return _opcHost; }
            set
            {
                _opcHost = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Host"));
            }
        }

        private string _opcURL;
        public string opcURL
        {
            get { return _opcURL; }
            set
            {
                _opcURL = value;

                OnPropertyChanged(new PropertyChangedEventArgs("opcURL"));
            }
        }

        private bool _opcRecconect;
        public bool opcRecconect
        {
            get { return _opcRecconect; }
            set
            {
                _opcRecconect = value;

                OnPropertyChanged(new PropertyChangedEventArgs("opcRecconect"));
            }
        }


        private string _dbConnString;
        public string dbConnString
        {
            get { return _dbConnString; }
            set
            {
                _dbConnString = value;

                OnPropertyChanged(new PropertyChangedEventArgs("dbConnString"));
            }
        }

        private bool _dbRecconect;
        public bool dbRecconect
        {
            get { return _dbRecconect; }
            set
            {
                _dbRecconect = value;

                OnPropertyChanged(new PropertyChangedEventArgs("dbRecconect"));
            }
        }

        public virtual List<dbTagItem> opcMonitoredTags {get; set;}

        public dbServerItem() 
        {
            opcMonitoredTags = new List<dbTagItem>();
        }

    }
}
