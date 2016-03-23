using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimatekExCnahger
{ 

    public static class vmColors
    {

        public static Color backAlarm = Colors.Red;
        public static Color backWarning = Colors.OrangeRed;
        public static Color backInfo = Colors.LightGray;
        public static Color backDefault = Colors.Gray;

        public static Color foreAlarm = Colors.Yellow;
        public static Color foreWarning = Colors.White;
        public static Color foreInfo = Colors.Blue;
        public static Color foreDefault = Colors.Black;


        static vmColors() { }
    }
}
