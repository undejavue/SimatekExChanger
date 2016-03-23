using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibGlobal
{
    public class gSpecialEntity : Entity
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
                OnPropertyChanged(new PropertyChangedEventArgs("isOPCwaiting"));
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

    }
}
