﻿using System;
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
using System.Data.Entity.Core.Objects;

namespace ClassLibOracle
{
    /// <summary>
    /// Remote databse (Oracle) exchange class
    /// </summary>
    public class OraExchanger
    {
        public static string TAG = LogFilter.RemoteDB.ToString(); 
        private OraContext context;
        public bool isDbConnectionOK;
        public bool isInsertOk;

        private OraContext contextSync;


        /// <summary>
        /// Operation with remote Oracle database
        /// </summary>
        public OraExchanger()
        {
            try
            {
                context = new OraContext();
                isDbConnectionOK = context.Database.Exists();
                OnReportMessage("Oracle Connection success");
            }
            catch (Exception ex)
            {
                isDbConnectionOK = false;
                OnReportMessage("Oracle Connection fail");
                OnReportMessage(ex.ToString());
            }           
        }

        /// <summary>
        /// Test connection by checking if database exist in context
        /// </summary>
        /// <returns>True if connected, fail message if not</returns>
        public bool TestConnection()
        {
            if (context != null)
            {
                try
                {
                    isDbConnectionOK = context.Database.Exists();
                }
                catch (Exception ex)
                {
                    isDbConnectionOK = false;
                    OnReportMessage("Oracle connection fail");
                    OnReportMessage(ex.ToString());
                }
            }
            return isDbConnectionOK;
        }

        /// <summary>
        /// Select records from remote table
        /// </summary>
        /// <returns>Binding list as a result of select operation</returns>
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
        /// Get database table field names list
        /// </summary>
        /// <returns>Field names</returns>
        public static List<string> GetFields()
        {
            oraEntity ent = new oraEntity();
            List<string> list = new List<string>();

            foreach (var prop in ent.GetType().GetProperties())
            {
                oraManualFields gSpec = new oraManualFields();                          
                if (prop.PropertyType != typeof(DateTime)  && !prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase) 
                                                            && !prop.Name.Contains("N_STAN")
                                                            && !prop.Name.Contains("G_UCHASTOK") )
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


        /// <summary>
        /// Insert tag values to database
        /// Convert list of tags in db table fields values to insert
        /// </summary>
        /// <param name="items">Tag items</param>
        /// <param name="spec">Manually entered from UI params</param>
        /// <returns>True if insert procedure is ok</returns>
        public bool  insert(List<mTag> items , oraManualFields spec)
        {
            DateTime insertTime = DateTime.Now;
            oraEntity ent = new oraEntity();

            // TODO: use items instead of en.types, because its shorter
            foreach (var p in ent.GetType().GetProperties())
            {
                if (items.Any(k => k.NameInDb == p.Name))
                {
                    mTag t = items.First(k => k.NameInDb == p.Name);
                    var targetType = IsNullableType(p.PropertyType) ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;

                    try
                    {
                        object propertyVal = Convert.ChangeType(t.oValue, targetType, CultureInfo.InvariantCulture);
                        p.SetValue(ent, propertyVal, null);
                    }
                    catch (Exception ex)
                    {
                        OnReportMessage("Tags <=> Fields conversion fail ");
                        OnReportMessage(ex.Message);
                    }
                }
            }

            // Values from UI
            ent.G_UCHASTOK = spec.G_UCHASTOK;
            ent.N_STAN = spec.N_STAN;
            ent.INCOMIN_DATE = insertTime;

            return AddRecord(ent);
        }

        /// <summary>
        /// Insert collection of entities to database
        /// </summary>
        /// <param name="entities">Oracle entity list</param>
        /// <returns>True if insert ok</returns>
        public bool insert(List<oraEntity> entities)
        {
            bool result = false;
            int retVal = 0;
            if (entities.Count > 0)
            {
                try
                {
                    using (contextSync = new OraContext())
                    {

                        //foreach (oraEntity e in entities)
                        //{
                        //    retVal = context.RUN_PROC_GUILD_OPC(e.G_UCHASTOK, 
                        //        e.N_STAN, 
                        //        e.START_STOP, 
                        //        e.ERASE, e.BREAK, 
                        //        e.REPLAC, 
                        //        (int)e.COUNTER, 
                        //        e.INCOMIN_DATE);

                        //    if ( retVal != 1 ) break;                            
                        //}

                        for (int i = 0; i < entities.Count; i++)
                        {
                            oraEntity e = new oraEntity();
                            e = entities[i];
                            retVal = contextSync.RUN_PROC_GUILD_OPC(e.G_UCHASTOK,
                                e.N_STAN,
                                e.START_STOP,
                                e.ERASE, e.BREAK,
                                e.REPLAC,
                                (int)e.COUNTER,
                                e.INCOMIN_DATE);

                            if (retVal != 1) break;
                        }

                        contextSync.SaveChanges();

                        //throw new InvalidOperationException("Storage procedure missing!");
                        if (retVal == 1)
                        {
                            OnReportMessage("Synchronization success");
                            result = true;
                        }
                        else
                        {
                            OnReportMessage("Synchronization fail, return_value = " + retVal.ToString());
                            result = false;
                        }


                    }

                }
                catch (InvalidOperationException ex)
                {
                    result = false;
                    OnReportMessage("Synchronization failed with exception");
                    OnReportMessage(ex.Message);
                }
            }

            isInsertOk = result;
            return result;
        }

       /// <summary>
       /// Insert entity to db table
       /// </summary>
       /// <param name="e">Oracle entity</param>
       /// <returns>True if added and commited</returns>
        private bool AddRecord(oraEntity e)
        {
            bool result = false;

            //OnReportMessage("Remote DB, insert:");
            //OnReportMessage("COUNTER = " + e.COUNTER.ToString());
            //OnReportMessage("NSTAN  = " + e.N_STAN.ToString());
            //OnReportMessage("GUCHASTOK  = " + e.G_UCHASTOK.ToString());
            //OnReportMessage("bool fields  = " 
            //    + e.BREAK.ToString() 
            //    + " & " + e.ERASE.ToString() 
            //    + " & " + e.REPLAC.ToString() 
            //    + " & " + e.START_STOP.ToString() );
            //OnReportMessage("INC_DATE = " + e.INCOMIN_DATE.ToString());

            try
            {
                int ret = 
                context.RUN_PROC_GUILD_OPC(e.G_UCHASTOK,
                            e.N_STAN,
                            e.START_STOP,
                            e.ERASE, e.BREAK,
                            e.REPLAC,
                            (int)e.COUNTER,
                            e.INCOMIN_DATE);

                context.SaveChanges();
                isDbConnectionOK = true;
                

                if (ret == 1)
                {
                    OnReportMessage("Remote DB, insert ok");
                    result = true;
                }
                else
                {
                    OnReportMessage("Remote DB, insert fail, return_value = " + ret.ToString());
                    result = false;
                }
            }
            catch (Exception ex)
            {
                OnReportMessage("Remote DB, insert fail with exception");
                OnReportMessage(ex.Message.ToString());

                isDbConnectionOK = false;

                if (ex.InnerException != null)
                    OnReportMessage(ex.InnerException.Message.ToString());
                result = false;
            }

            isInsertOk = result;
            return result;
        }


        /// <summary>
        /// Remote db testing purpose,
        /// generate random entity and trying to insert
        /// </summary>
        /// <returns>True if insert ok</returns>
        public bool AddTestRecord()
        {
            bool result = false;

            try
            {
                oraEntity e = generateRecord();

                int ret =
                context.RUN_PROC_GUILD_OPC(e.G_UCHASTOK,
                            e.N_STAN,
                            e.START_STOP,
                            e.ERASE, e.BREAK,
                            e.REPLAC,
                            (int)e.COUNTER,
                            e.INCOMIN_DATE);

                context.SaveChanges();
                isDbConnectionOK = true;

                if (ret == 1)
                {
                    OnReportMessage("Remote DB, insert ok");
                    result = true;
                }
                else
                {
                    OnReportMessage("Remote DB, insert fail, return_value = " + ret.ToString());
                    result = false;
                }
            }
            catch (InvalidOperationException ex)
            {
                OnReportMessage("Insert fail for remote DB");
                OnReportMessage(ex.Message.ToString());

                isDbConnectionOK = false;

                if (ex.InnerException != null)
                    OnReportMessage(ex.InnerException.Message.ToString());

                result = false;
            }

            return result;
        }

        /// <summary>
        /// Test entity generator
        /// </summary>
        /// <returns>Oracle entity</returns>
        private oraEntity generateRecord()
        {
            oraEntity r = new oraEntity();
            r.COUNTER = 22;
            r.BREAK = true;
            r.ERASE = true;
            r.INCOMIN_DATE = DateTime.Now;
            r.N_STAN = 33;
            r.REPLAC = false;
            r.START_STOP = false;
            r.G_UCHASTOK = "T";
            return r;
        }



        #region Message handlers

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

        #endregion


    }
}
