using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EFconfigDB
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

        private string _urlString;
        public string urlString
        {
            get { return _urlString; }
            set
            {
                _urlString = value;

                OnPropertyChanged(new PropertyChangedEventArgs("urlString"));
            }
        }


        public virtual List<dbTagItem> monitoredTags {get; set;}


        public dbServerItem() 
        {
            monitoredTags = new List<dbTagItem>();
        }

    }
}
