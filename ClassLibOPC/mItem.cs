using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace ClassLibOPC
{
    public class mItem : Entity
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


        public mItem() { }

        public mItem(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

    }
}
