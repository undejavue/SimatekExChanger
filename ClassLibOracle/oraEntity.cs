using System;
using System.ComponentModel;


namespace ClassLibOracle
{
    public class oraEntity //: INotifyPropertyChanged
    {
        public string G_UCHASTOK { get; set; }
        public int? N_STAN { get; set; }
        public bool? START_STOP { get; set; }
        public bool? ERASE { get; set; }
        public bool? BREAK { get; set; }
        public bool? REPLAC { get; set; }
        public int? COUNTER { get; set; }
        public DateTime? INCOMIN_DATE { get; set; }


        public oraEntity() { }

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, e);
        //}
    }
}
