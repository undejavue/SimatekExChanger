using System.ComponentModel;


namespace ClassLibOracle
{
    public class oraEntity : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                //OnPropertyChanged(new PropertyChangedEventArgs("Name"));
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

        


        public oraEntity() { }

        public oraEntity(string n, string val)
        {
            Name = n;
            Value = val;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
