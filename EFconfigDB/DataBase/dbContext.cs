using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Entity;

namespace EFconfigDB
{
    public class dbContext : DbContext
    {
        public DbSet<dbServerItem> dbServerConfig { get; set; }
        public DbSet<dbTagItem> dbTag { get; set; }


        public dbContext(bool isNew)
            : base(OneConnectionString(""))
        {

            if (isNew)
                Database.SetInitializer<dbContext>(new DropCreateDatabaseAlways<dbContext>());
            else
                Database.SetInitializer<dbContext>(new DropCreateDatabaseIfModelChanges<dbContext>());

            this.Configuration.LazyLoadingEnabled = false; 
               
        }


        public dbContext(string filename): base(OneConnectionString(filename))
        {
            Database.SetInitializer<dbContext>(new DropCreateDatabaseIfModelChanges<dbContext>());

            this.Configuration.LazyLoadingEnabled = false;        
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<dbServerItem>().HasMany<dbTagItem>(k => k.opcMonitoredTags).WithRequired(k => k.srvID);
            //modelBuilder.Entity<dbServerItem>().HasMany<dbTagItem>(k => k.monitoredTags).WithRequired(k => k.srvID).HasForeignKey();

        }

        private static string OneConnectionString(string filepath)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            //string  connectionString="Data Source=(LocalDB)\v11.0;AttachDbFilename=D:\proj_VS\OneCLick_Automation\ClassLibrary\OneClickDB.mdf;Integrated Security=True";

            if (filepath.Equals("")) filepath = @"|DataDirectory|\sconfig.mdf";

            // Set the properties for the data source.
            sqlBuilder.DataSource = "(LocalDB)\\v11.0";
            sqlBuilder.AttachDBFilename = filepath; //"|DataDirectory|\\" + 
            sqlBuilder.IntegratedSecurity = true;
            sqlBuilder.Pooling = false;


            return sqlBuilder.ToString();
        }

    }
}
