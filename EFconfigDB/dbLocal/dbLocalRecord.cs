using System.ComponentModel;
using ClassLibOracle;

namespace EFlocalDB
{
    public class dbLocalRecord : FIX_STAN789_T, INotifyPropertyChanged
    {
        private bool _flagIsSent;
        public bool flagIsSent
        {
            get { return _flagIsSent; }
            set
            {
                _flagIsSent = value;

                OnPropertyChanged(new PropertyChangedEventArgs("flagIsSent"));
            }
        }

        private int _id;
        public int pid
        {
            get { return _id; }
            set
            {
                _id = value;

                OnPropertyChanged(new PropertyChangedEventArgs("pid"));
            }
        }


        public dbLocalRecord() { }

        public dbLocalRecord(FIX_STAN789_T ent)
        {
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }


    public class converter
    {
        private void test()
        {
            dbLocalRecord rec = new dbLocalRecord();

            FIX_STAN789_T ent = new FIX_STAN789_T();

            ent = rec;

            
            

        }
    }

}
