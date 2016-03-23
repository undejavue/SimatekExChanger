using System.Windows.Media;
using System.ComponentModel;
using ClassLibGlobal;

namespace SimatekExCnahger
{
    public class vmError : gErrorEntity
    {


        private Brush _backColor;
        public Brush backColor
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                OnPropertyChanged(new PropertyChangedEventArgs("backColor"));
            }
        }

        private Color _foreColor;
        public Color foreColor
        {
            get { return _foreColor; }
            set
            {
                _foreColor = value;
                OnPropertyChanged(new PropertyChangedEventArgs("foreColor"));
            }
        }

        
        public vmError(gErrorEntity errEnt)
        {
            setFields(errEnt);
            errEnt.PropertyChanged += ErrEnt_PropertyChanged;  
        }

        private void ErrEnt_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            setFields((gErrorEntity)sender as gErrorEntity);        
            if (e.PropertyName.Equals("isError")) { SetBackColor(); }
            if (e.PropertyName.Equals("code")) { SetForeColor(); }
        }

        private void setFields(gErrorEntity errEnt)
        {
            this.isError = errEnt.isError;
            this.code = errEnt.code;
            this.message = errEnt.message;
        }


        private void SetDefaultColors()
        {
            backColor = new SolidColorBrush(vmColors.backDefault);
            foreColor = vmColors.foreDefault;
        }

        private void SetBackColor()
        {
            if (isError)
            {
                backColor = new SolidColorBrush(vmColors.backAlarm);            
            }
            else
            {
                backColor = new SolidColorBrush(vmColors.backDefault);
            }
        }

        private void SetForeColor()
        {
            switch (code)
            {
                case 0:
                    foreColor = vmColors.foreDefault;
                    backColor = new SolidColorBrush(vmColors.backDefault);
                    break;
                case 500:
                    foreColor = vmColors.foreWarning;
                    backColor = new SolidColorBrush(vmColors.backWarning);
                    break;
                case 999:
                    foreColor = vmColors.foreAlarm;
                    backColor = new SolidColorBrush(vmColors.backAlarm);
                    break;
                default:
                    foreColor = vmColors.foreDefault;
                    backColor = new SolidColorBrush(vmColors.backDefault);
                    break;
            }
        }
    }
}
