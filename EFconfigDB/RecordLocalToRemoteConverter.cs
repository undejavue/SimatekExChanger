using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibOracle;

namespace EFlocalDB
{
    public class RecordLocalToRemoteConverter
    {
        private FIX_STAN789_T oraEnt;

        public RecordLocalToRemoteConverter()
        {
            oraEnt = new FIX_STAN789_T();
        }

        public FIX_STAN789_T return_oraRecord(dbLocalRecord rec)
        {
            oraEnt.BREAK = rec.BREAK;
            oraEnt.COUNTER = rec.COUNTER;
            oraEnt.ERASE = rec.ERASE;
            oraEnt.G_UCHASTOK = rec.G_UCHASTOK;
            oraEnt.INCOMIN_DATE = rec.INCOMIN_DATE;
            oraEnt.N_STAN = rec.N_STAN;
            oraEnt.REPLAC = rec.REPLAC;
            oraEnt.START_STOP = rec.START_STOP;
            oraEnt.WHEN = rec.WHEN;

            return oraEnt;
        }
        
    }
}
