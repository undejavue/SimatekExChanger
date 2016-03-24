using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;
using System.Data.Sql;
using System.Data.Entity;
using System.Collections.ObjectModel;

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace EFlocalDB
{
    public class dbConfManager
    {
        private dbConfContext context;
        private string dbPath;

        public dbConfManager()
        {

        }

        public bool Save(dbServerItem server, string dbFilePath)
        {
            dbPath = dbFilePath;

            try {

                using (context = new dbConfContext(dbFilePath))
                {
                    context.dbServerConfig.Add(server);
                    context.SaveChanges();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public dbServerItem Load(string dbFilePath)
        {

            dbPath = dbFilePath;
            ObservableCollection<dbServerItem> config = new ObservableCollection<dbServerItem>();

            try
            {

                using (context = new dbConfContext(dbFilePath))
                {
                    context.dbServerConfig.Include(s => s.opcMonitoredTags).Load();
                    config = context.dbServerConfig.Local;
                }

                return config.LastOrDefault();
            }
            catch
            {
                return new dbServerItem();
            }

            
        }


        public bool SaveLogs(List<dbLogItem> logs)
        {
            if (!string.IsNullOrEmpty(dbPath))
            {
                using (context = new dbConfContext(dbPath))
                {
                    context.dbLog.AddRange(logs);
                    context.SaveChanges();
                    return true;
                }
            }
            return false;
        }

    }
}
