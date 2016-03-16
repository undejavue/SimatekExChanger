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
        dbConfContext context;

        public dbConfManager()
        {

 

        }

        public void Save(dbServerItem server, string dbFilePath)
        {
            using (context = new dbConfContext(dbFilePath))
            {             
                context.dbServerConfig.Add(server);
                context.SaveChanges();
            }
        }

        public dbServerItem Load(string dbFilePath)
        {
            ObservableCollection<dbServerItem> config = new ObservableCollection<dbServerItem>();

            using (context = new dbConfContext(dbFilePath))
            {
                context.dbServerConfig.Include(s => s.opcMonitoredTags).Load();

                config = context.dbServerConfig.Local;
            }

            return config.LastOrDefault();
        }

    }
}
