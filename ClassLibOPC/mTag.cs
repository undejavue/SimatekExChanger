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
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Description"));
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

        public mTag() { }

        public mTag(string tagName, string tagPath)
        {
            Name = tagName;
            Path = tagPath;
        }

    }
}
