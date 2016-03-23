using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;



namespace SimatekExCnahger
{
    public static class Converters
    {

        public static LogEntryCollectionToTextConverter LogEntryCollectionToTextConverter = new LogEntryCollectionToTextConverter();
      
    }
    public class LogEntryCollectionToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> logEntries = values[0] as ObservableCollection<string>;

            if (logEntries != null && logEntries.Count > 0)
                return logEntries.ToString();
            else
                return String.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
