using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EFconfigDB
{
    public class dbTagItem :Entity
    {

        //private string _srvID;
        //public string srvID
        //{
        //    get { return _srvID; }
        //    set
        //    {
        //        _srvID = value;

        //        OnPropertyChanged(new PropertyChangedEventArgs("srvID"));
        //    }
        //}
        
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

        public virtual dbServerItem srvID {get; set; }

        public dbTagItem ()
        {

        }

        public dbTagItem(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public dbTagItem(string name, string path, string desc)
        {
            this.Name = name;
            this.Path = path;
            this.Description = desc;
        }
    }
}
