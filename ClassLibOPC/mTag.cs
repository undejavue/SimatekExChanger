using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace ClassLibOPC
{
    public class mTag : Entity
    {
        private string _nameInDb;
        public string NameInDb
        {
            get { return _nameInDb; }
            set
            {
                _nameInDb = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NameInDb"));
            }
        }


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

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Path"));
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Value"));
            }
        }

        private object _ovalue;
        public object oValue
        {
            get { return _ovalue; }
            set
            {
                _ovalue = value;

                OnPropertyChanged(new PropertyChangedEventArgs("oValue"));
            }
        }

        private string _quality;
        public string Quality
        {
            get { return _quality; }
            set
            {
                _quality = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Quality"));
            }
        }

        private bool _onChange;
        public bool onChange
        {
            get { return _onChange; }
            set
            {
                _onChange = value;

                OnPropertyChanged(new PropertyChangedEventArgs("onChange"));
            }
        }

        public mTag() { }

        public mTag(string tagName, string tagPath)
        {
            Name = tagName;
            Path = tagPath;
        }

    }
}
