using System;
using System.ComponentModel;

namespace ClassLibOPC
{
    public abstract class Entity : INotifyPropertyChanged
    {

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set
            {
                _Id = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

    }
}
