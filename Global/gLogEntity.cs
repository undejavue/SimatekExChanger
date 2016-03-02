using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibGlobal
{
    public class gLogEntity : Entity
    {
        private string _message;
        public string message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged(new PropertyChangedEventArgs("message"));
            }
        }

        private string _time;
        public string time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                OnPropertyChanged(new PropertyChangedEventArgs("time"));
            }
        }


        private string _entry;
        public string Entry
        {
            get
            {
                return _entry;
            }
            set
            {
                _entry = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Entry"));
            }
        }

        public gLogEntity (string m)
        {
            message = m;
            time = DateTime.Now.ToString("hh:mm:ss");
            Entry = time + ": " + message;
        }

    }
}
