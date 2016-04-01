using System.ComponentModel;

namespace ClassLibOracle
{
    public class oraManualFields : INotifyPropertyChanged
    {
        private string _G_UCHASTOK;
        public string G_UCHASTOK
        {
            get
            {
                return _G_UCHASTOK;
            }
            set
            {
                _G_UCHASTOK = value;
                OnPropertyChanged(new PropertyChangedEventArgs("G_UCHASTOK"));
            }
        }

        private int _N_STAN;
        public int N_STAN
        {
            get
            {
                return _N_STAN;
            }
            set
            {
                _N_STAN = value;
                OnPropertyChanged(new PropertyChangedEventArgs("N_STAN"));
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
