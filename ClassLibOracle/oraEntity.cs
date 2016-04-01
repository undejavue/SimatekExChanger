using System;
using System.ComponentModel;


namespace ClassLibOracle
{
    public class oraEntity //: INotifyPropertyChanged
    {
        public string G_UCHASTOK { get; set; }
        public decimal? N_STAN { get; set; }
        public bool? START_STOP { get; set; }
        public decimal? ERASE { get; set; }
        public decimal? BREAK { get; set; }
        public decimal? REPLAC { get; set; }
        public decimal? COUNTER { get; set; }
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
