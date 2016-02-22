using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;


namespace ClassLibOracle
{
    public class OraExchanger
    {

        private OraContext context;
        public bool isConnectionOK;

        public OraExchanger()
        {
            using (context = new OraContext())
            {
                try
                {
                    context.FIX_STAN789_T.Count();
                    isConnectionOK = true;
                    OnReportMessage("Oracle Connection success");
                }
                catch (Exception ex)
                {
                    isConnectionOK = false;
                    OnReportMessage("Oracle Connection fail");
                    OnReportMessage(ex.ToString());
                }
            }

        }

        public bool insertData()
        {
            bool result = false;
            
            using (context = new OraContext())
            {

                try
                {
                    decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;
                    FIX_STAN789_T r = generateRecord(++maxID);
                    context.FIX_STAN789_T.Add(r);
                    context.SaveChanges();

                    result = true;
                }
                catch (Exception ex)
                {
                    OnReportMessage("Insert fail");
                    OnReportMessage(ex.Message.ToString());

                    if (ex.InnerException != null)
                        OnReportMessage(ex.InnerException.Message.ToString());

                    result = false;
                }

            }
            return result;
        }

        public bool insertData(FIX_STAN789_T data)
        {
            bool result = false;

            using (context = new OraContext())
            {

                try
                {
                    decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;
                    data.ID = maxID + 1;

                    context.FIX_STAN789_T.Add(data);
                    context.SaveChanges();

                    OnReportMessage("Inserted with id="+ data.ID.ToString());
                    result = true;
                }
                catch (Exception ex)
                {
                    OnReportMessage("Insert fail");
                    OnReportMessage(ex.Message.ToString());

                    if (ex.InnerException != null)
                        OnReportMessage(ex.InnerException.Message.ToString());

                    result = false;
                }

            }
            return result;
        }



        private FIX_STAN789_T generateRecord(decimal id)
        {
            FIX_STAN789_T r = new FIX_STAN789_T();

            int i = Decimal.ToInt32(id);

            r.ID = id;
            r.COUNTER = i * 2;
            r.BREAK = true;
            r.ERASE = true;
            r.INCOMIN_DATE = DateTime.Now;
            r.N_STAN = id + 2;
            r.REPLAC = false;
            r.START_STOP = true;
            r.WHEN = DateTime.Now;
            r.G_UCHASTOK = "s";

            return r;
        }
        


        public delegate void oraEventHandler(object sender, oraEventArgs args);

        public event oraEventHandler ReportMessage;

        protected virtual void OnReportMessage(string message)
        {
            if (ReportMessage != null)
            {
                ReportMessage(this, new oraEventArgs(message));
            }
        }



    }
}
