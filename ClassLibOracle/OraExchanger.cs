using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel;
using ClassLibOPC;

namespace ClassLibOracle
{
    public class OraExchanger
    {

        private OraContext context;
        public bool isConnectionOK;

        //public ObservableCollection<oraEntity> items;
        //private ObservableCollection<FIX_STAN789_T> oraItems;

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

                
            }
            catch (Exception ex)
            {
                isConnectionOK = false;
                OnReportMessage("Oracle Connection fail");
                OnReportMessage(ex.ToString());
            }
            

        }


        public bool TestConnection()
        {
            if (context != null)
            {
                try
                {
                    if (context.FIX_STAN789_T.Count() > 0)
                    {
                        isConnectionOK = true;
                    }
                }
                catch (Exception ex)
                {
                    isConnectionOK = false;
                    OnReportMessage("Oracle connection fail");
                    OnReportMessage(ex.ToString());
                }

            }
            return isConnectionOK;
        }


        public BindingList<FIX_STAN789_T> GetRecords()
        {
            context.FIX_STAN789_T.Load();
            return context.FIX_STAN789_T.Local.ToBindingList();
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

        public bool  insert(List<mTag> items)
        {
            FIX_STAN789_T ent = new FIX_STAN789_T();

            try
            {
                foreach (var p in ent.GetType().GetProperties())
                {

                    if (items.Any(k => k.NameInDb == p.Name))
                    {
                        mTag t = items.First(k => k.NameInDb == p.Name);

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

            ent.INCOMIN_DATE = DateTime.Now; // ! change!!!
            //ent. = DateTime.Now;


            return AddRecord(ent);
        }


        public bool insert(List<FIX_STAN789_T> entities)
        {
            bool result = false;
            if (entities.Count > 0)
            {
                try
                {
                    decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;

                    foreach (FIX_STAN789_T ent in entities)
                    {
                        ent.ID = maxID + 1;
                        //ent.WHEN = DateTime.Now;
                        context.FIX_STAN789_T.Add(ent);
                    }

                    
                    context.SaveChanges();

                    result = true;
                }

                catch (Exception ex)
                {
                    result = false;
                    OnReportMessage("Synchronization failed");
                    OnReportMessage(ex.Message);
                }
            }

            return result;
        }

       
        public bool AddRecord(FIX_STAN789_T data)
        {
            bool result = false;

            try
            {

                decimal maxID = context.FIX_STAN789_T.First(x => x.ID == context.FIX_STAN789_T.Max(i => i.ID)).ID;
                data.ID = maxID + 1;
                context.FIX_STAN789_T.Add(data);
                context.SaveChanges();

                isConnectionOK = true;
                OnReportMessage("Remote DB, inserted with id=" + data.ID.ToString());

                result = true;
            }
            catch (Exception ex)
            {
                OnReportMessage("Insert fail for remote DB");
                OnReportMessage(ex.Message.ToString());

                isConnectionOK = false;

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
            //r.WHEN = DateTime.Now;
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
