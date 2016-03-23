using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using ClassLibGlobal;
using ClassLibOracle;
using System.Linq;
using ClassLibOPC;
using System.ComponentModel;
using System.Collections.Generic;

namespace EFlocalDB
{
    public class dbLocalManager
    {
        dbLocalContext context;
        public ObservableCollection<gLogEntity> messageLog;
        private ObservableCollection<dbLocalRecord> records;

        public dbLocalManager(string filename)
        {
            messageLog = new ObservableCollection<gLogEntity>();

            try {
                context = new dbLocalContext(filename);
                logMessage("Local database created, stored in " + filename);
            }
            catch (Exception ex)
            {
                logMessage("Fail to create local databse");
                logMessage(ex.Message);
            }
        }



        public void insert(ObservableCollection<mTag> tags, bool flag)
        {
            dbLocalRecord record = new dbLocalRecord();
            record = TagsToRecordEntity(tags);
            record.flagIsSent = flag;

            try
            {
                context.dbRecords.Add(record);
                context.SaveChanges();

                logMessage("Local database record inserted");
            }
            catch (Exception ex)
            {
                logMessage("Local database record insert error");
                logMessage(ex.Message);
            }

        }

        private dbLocalRecord TagsToRecordEntity(ObservableCollection<mTag> items)
        {
            dbLocalRecord ent = new dbLocalRecord();

            if (items.Count > 0)
            {

                try
                {
                    foreach (var p in ent.GetType().GetProperties())
                    {

                        if (items.Any(k => k.NameInDb == p.Name))
                        {
                            mTag t = items.First(k => k.NameInDb == p.Name);

                            var targetType = G.IsNullableType(p.PropertyType) ? Nullable.GetUnderlyingType(p.PropertyType) : p.PropertyType;

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
                ent.WHEN_DATE = DateTime.Now;
                ent.flagIsSent = false;

            }

            return ent;
        }


        public bool updateFlags(List<int> ids)
        {
            bool result = false;
            try {
                foreach (int id in ids)
                {
                    context.dbRecords.Find(id).flagIsSent = true;
                }

                context.SaveChanges();
                logMessage(ids.Count().ToString() + " records are successfully updated in local db");
                result = true;
            }
            catch(Exception ex)
            {
                result = false;
                logMessage("Fail to update record flags in local db");
                logMessage(ex.Message);
            }

            return result;
        }


        public BindingList<dbLocalRecord> getAllRecords()
        {
            try {
                context.dbRecords.Load();
                return context.dbRecords.Local.ToBindingList();
            }
            catch (Exception ex)
            {
                logMessage("LocalDb load context failed");
                logMessage(ex.Message);
            }

            return new BindingList<dbLocalRecord>();
        }

        public List<dbLocalRecord> getNotSyncRecords()
        {
            try {
                List<dbLocalRecord> records =
                    new List<dbLocalRecord>(context.dbRecords.Where(r => r.flagIsSent == false).ToList());

                return records;
            }
            catch (Exception ex)
            {
                logMessage("LocalDb getNotSyncREcords() failed");
                logMessage(ex.Message);
            }

            return new List<dbLocalRecord>();
        }


        #region Message log system

        public delegate void dbLocalEvent(object sender, gEventArgs args);
        public event dbLocalEvent ReportMessage;

        protected virtual void OnReportMessage(string message)
        {
            dbLocalEvent ReportMessageCopy = ReportMessage;

            if (ReportMessageCopy != null)
            {
                ReportMessageCopy(this, new gEventArgs(message));
            }
        }


        private void logMessage(string message)
        {
            messageLog.Add(new gLogEntity(message));
            OnReportMessage(message);
        }

        #endregion
    }
}
