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

namespace EFconfigDB
{
    public class dbConfig
    {
        dbContext context;

        public dbConfig()
        {

 

        }

        public void Save(dbServerItem server, string dbFilePath)
        {
            using (context = new dbContext(dbFilePath))
            {             
                context.dbServerConfig.Add(server);
                context.SaveChanges();
            }
        }

        public dbServerItem Load(string dbFilePath)
        {
            ObservableCollection<dbServerItem> config = new ObservableCollection<dbServerItem>();

            using (context = new dbContext(dbFilePath))
            {
                context.dbServerConfig.Include(s => s.monitoredTags).Load();

                config = context.dbServerConfig.Local;
            }

            return config.LastOrDefault();
        }

    }
}
