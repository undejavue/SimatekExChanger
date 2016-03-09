using System.Windows.Media;
using System.ComponentModel;
using ClassLibGlobal;

namespace WPFinterface
{
    public class vmError : gErrorEntity
    {

        public new int code
        {
            get
            {
                return base.code;
            }
            set
            {
                base.code = value;
                SetColor();
                OnPropertyChanged(new PropertyChangedEventArgs("code"));
            }
        }



        private Brush _errColor;
        public Brush errColor
        {
            get
            {
                return _errColor;
            }
            set
            {
                _errColor = value;
                OnPropertyChanged(new PropertyChangedEventArgs("errColor"));
            }
        }


        private static Color errorColor = Colors.Red;
        private static Color normColor = Colors.Gray;



        public vmError()
        {

        }


        private void SetColor()
        {
            if (base.isError)
            {
                errColor = new SolidColorBrush(errorColor);
            }
            else
            {
                errColor = new SolidColorBrush(normColor);
            }
        }


    }
}
