using System.Data.Entity;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFlocalDB
{
    public class dbLocalContext : DbContext
    {
        public DbSet<dbLocalRecord> dbRecords { get; set; }



        public dbLocalContext(bool isNew)
            : base(OneConnectionString(""))
        {

            if (isNew)
                Database.SetInitializer(new DropCreateDatabaseAlways<dbLocalContext>());
            else
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<dbLocalContext>());

            this.Configuration.LazyLoadingEnabled = false;

        }


        public dbLocalContext(string filename, bool isNew): base(OneConnectionString(filename))
        {
            if (isNew)
                Database.SetInitializer(new DropCreateDatabaseAlways<dbLocalContext>());
            else
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<dbLocalContext>());

            this.Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            // modelBuilder.Entity<dbServerItem>().HasMany<dbTagItem>(k => k.opcMonitoredTags).WithRequired(k => k.srvID);
            //modelBuilder.Entity<dbServerItem>().HasMany<dbTagItem>(k => k.monitoredTags).WithRequired(k => k.srvID).HasForeignKey();

            modelBuilder.Entity<dbLocalRecord>()
                .HasKey(k => k.pid);
            modelBuilder.Entity<dbLocalRecord>()
                .Property(k => k.pid)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("key");

        }

        private static string OneConnectionString(string filepath)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();


            if (filepath.Equals("")) filepath = @"|DataDirectory|\efLocalDb.mdf";

            // Set the properties for the data source.
            sqlBuilder.DataSource = "(LocalDB)\\v11.0";
            sqlBuilder.AttachDBFilename = filepath; //"|DataDirectory|\\" 
            sqlBuilder.IntegratedSecurity = true;
            sqlBuilder.Pooling = false;

            return sqlBuilder.ToString();
        }

    }
}
