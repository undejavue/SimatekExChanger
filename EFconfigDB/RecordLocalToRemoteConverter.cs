using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibOracle;

namespace EFlocalDB
{
    public class EntLocalToRemoteConverter
    {


        public EntLocalToRemoteConverter()
        {

        }

        public oraEntity return_oraRecord(dbLocalRecord rec)
        {
            oraEntity oraEnt = new oraEntity();

            oraEnt.BREAK = rec.BREAK;
            oraEnt.COUNTER = rec.COUNTER;
            oraEnt.ERASE = rec.ERASE;
            oraEnt.G_UCHASTOK = rec.G_UCHASTOK;
            oraEnt.INCOMIN_DATE = rec.INCOMIN_DATE;
            oraEnt.N_STAN = rec.N_STAN;
            oraEnt.REPLAC = rec.REPLAC;
            oraEnt.START_STOP = rec.START_STOP;

            return oraEnt;
        }
        
    }
}
