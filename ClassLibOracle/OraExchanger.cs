using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<oraEntity> items;
        private ObservableCollection<FIX_STAN789_T> oraItems;

        public OraExchanger()
        {

            try
            {
                int c = 0;
                context = new OraContext();
                
                c = context.FIX_STAN789_T.Count();
                
                if (c > 0)
                {
                    isConnectionOK = true;
                    OnReportMessage("Oracle Connection success");
                }

                items = new ObservableCollection<oraEntity>();
                oraItems = new ObservableCollection<FIX_STAN789_T>();
                
            }
            catch (Exception ex)
            {
                isConnectionOK = false;
                OnReportMessage("Oracle Connection fail");
                OnReportMessage(ex.ToString());
            }
            

        }

        public ObservableCollection<FIX_STAN789_T> bindContext()
        {
            context.FIX_STAN789_T.Load();
            return context.FIX_STAN789_T.Local;
        }


        /// <summary>
        /// Get list of names of database table fields 
        /// </summary>
        /// <returns></returns>
        public List<string> GetFields()
        {
            FIX_STAN789_T ent = new FIX_STAN789_T();

            List<string> list = new List<string>();

            foreach (var prop in ent.GetType().GetProperties())
            {
                if (prop.PropertyType != typeof(DateTime) & !prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {

                    string s = prop.Name;
                    list.Add(s);
                    
                }
            }

            return list;
        }


        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        public void bindData()
        {
            FIX_STAN789_T ent = new FIX_STAN789_T();

            try
            {
                foreach (var p in ent.GetType().GetProperties())
                {

                    if (items.Any(k => k.Name == p.Name))
                    {
                        oraEntity t = items.First(k => k.Name == p.Name);

                        var targetType = IsNullableType(p.PropertyType) ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;

                        //Returns an System.Object with the specified System.Type and whose value is
                        //equivalent to the specified object.
                        object propertyVal = Convert.ChangeType(t.Value, targetType);

                        //Set the value of the property
                        p.SetValue(ent, propertyVal, null);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            ent.INCOMIN_DATE = DateTime.Now;
            ent.WHEN = DateTime.Now;


            insertData(ent);
        }

        public bool insertData()
        {
            bool result = false;

            try
            {
                using (context = new OraContext())
                {
                    decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;
                    FIX_STAN789_T r = generateRecord(++maxID);
                    context.FIX_STAN789_T.Add(r);
                    context.SaveChanges();
                }

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
         
            return result;
        }

        public bool insertData(FIX_STAN789_T data)
        {
            bool result = false;

            try
            {

                decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;
                data.ID = maxID + 1;
                context.FIX_STAN789_T.Add(data);
                context.SaveChanges();

                OnReportMessage("Inserted with id=" + data.ID.ToString());
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
            oraEventHandler ReportMessageCopy = ReportMessage;
            if (ReportMessageCopy != null)
            {
                ReportMessageCopy(this, new oraEventArgs(message));
            }
        }



    }
}
