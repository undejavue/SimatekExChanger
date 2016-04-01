using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel;
using ClassLibOPC;
using ClassLibGlobal;
using System.Globalization;

namespace ClassLibOracle
{
    public class OraExchanger
    {
        public static string TAG = LogFilter.RemoteDB.ToString(); 

        private OraContext context;
        public bool isConnectionOK;

        //public ObservableCollection<oraEntity> items;
        //private ObservableCollection<ORA_TABLE> oraItems;

        public OraExchanger()
        {
            try
            {
                context = new OraContext();
                isConnectionOK = context.Database.Exists();
                OnReportMessage("Oracle Connection success");
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
                    isConnectionOK = context.Database.Exists();
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


        public BindingList<oraEntity> GetRecords()
        {
            try
            {
                //context.ORA_TABLE.Load();
                //return context.ORA_TABLE.Local.ToBindingList();
                throw new NotImplementedException("Select procedure not defined");
            }
            catch (Exception ex)
            {
                return new BindingList<oraEntity>();
            }
        }



        /// <summary>
        /// Get list of names of database table fields 
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFields()
        {
            oraEntity ent = new oraEntity();
            List<string> list = new List<string>();

            foreach (var prop in ent.GetType().GetProperties())
            {
                oraManualFields gSpec = new oraManualFields();
                
                

                if (prop.PropertyType != typeof(DateTime?)  & !prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase) 
                                                            & !prop.Name.Contains("N_STAN")
                                                            & !prop.Name.Contains("G_UCHASTOK") )
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

        public bool  insert(List<mTag> items , oraManualFields spec)
        {
            DateTime insertTime = DateTime.Now;
            oraEntity ent = new oraEntity();

            foreach (var p in ent.GetType().GetProperties())
            {

                if (items.Any(k => k.NameInDb == p.Name))
                {
                    mTag t = items.First(k => k.NameInDb == p.Name);

                    var targetType = IsNullableType(p.PropertyType) ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;

                    try
                    {
                        //Returns an System.Object with the specified System.Type and whose value is
                        //equivalent to the specified object.
                        object propertyVal = Convert.ChangeType(t.Value, targetType, CultureInfo.InvariantCulture);

                        //Set the value of the property
                        p.SetValue(ent, propertyVal, null);
                    }
                    catch (Exception ex) { string s = ex.Message; }
                }
            }


            ent.G_UCHASTOK = spec.G_UCHASTOK;
            ent.N_STAN = spec.N_STAN;
            ent.INCOMIN_DATE = insertTime;


            return AddRecord(ent);
        }


        public bool insert(List<oraEntity> entities)
        {
            bool result = false;
            int retVal = 0;
            if (entities.Count > 0)
            {
                try
                {

                    foreach (oraEntity e in entities)
                    {
                        retVal = context.SP_INSERT_STAN(e.G_UCHASTOK,
                        e.N_STAN,
                       null,// e.START_STOP,
                        e.ERASE,
                        e.BREAK,
                        e.REPLAC,
                        e.COUNTER,
                        e.INCOMIN_DATE);
                    }

                    //throw new InvalidOperationException("Storage procedure missing!");
                    OnReportMessage("Synchronization success, retVal = " + retVal.ToString());
                    context.SaveChanges();
                    result = true;
                }

                catch (InvalidOperationException ex)
                {
                    result = false;
                    OnReportMessage("Synchronization failed");
                    OnReportMessage(ex.Message);
                }
            }

            return result;
        }

       
        public bool AddRecord(oraEntity e)
        {
            bool result = false;
            try
            {
                int retVal =
                    context.SP_INSERT_STAN(e.G_UCHASTOK,
                                            e.N_STAN,
                                            null, //e.START_STOP,
                                            e.ERASE,
                                            e.BREAK,
                                            e.REPLAC,
                                            e.COUNTER,
                                            e.INCOMIN_DATE);
                   
                context.SaveChanges();
                isConnectionOK = true;
                OnReportMessage("Remote DB, inserted with retVal = " + retVal.ToString());

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


        public bool AddTestRecord()
        {
            bool result = false;

            try
            {
                oraEntity e = generateRecord();

                int retVal = context.SP_INSERT_STAN(e.G_UCHASTOK,
                            e.N_STAN,
                            null, //e.START_STOP,
                            e.ERASE,
                            e.BREAK,
                            e.REPLAC,
                            e.COUNTER,
                            e.INCOMIN_DATE);

                context.SaveChanges();
                isConnectionOK = true;
                OnReportMessage("Remote DB, inserted with retVal = " + retVal.ToString());
                result = true;
            }
            catch (InvalidOperationException ex)
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


        private oraEntity generateRecord()
        {
            oraEntity r = new oraEntity();
            r.COUNTER = 22;
            r.BREAK = 1;
            r.ERASE = 1;
            r.INCOMIN_DATE = DateTime.Now;
            r.N_STAN = 0;
            r.REPLAC = 0;
            //r.START_STOP = 1;
            r.G_UCHASTOK = "T";
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
