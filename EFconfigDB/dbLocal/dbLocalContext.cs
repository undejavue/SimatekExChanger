﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Database.SetInitializer<dbConfContext>(new DropCreateDatabaseAlways<dbConfContext>());
            else
                Database.SetInitializer<dbConfContext>(new DropCreateDatabaseIfModelChanges<dbConfContext>());

            this.Configuration.LazyLoadingEnabled = false;

        }


        public dbLocalContext(string filename): base(OneConnectionString(filename))
        {
            Database.SetInitializer<dbConfContext>(new DropCreateDatabaseIfModelChanges<dbConfContext>());

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