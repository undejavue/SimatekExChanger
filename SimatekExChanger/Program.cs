using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFlocalDB;

namespace SimatekExChanger
{
    class Program
    {
        static void Main(string[] args)
        {
            testEFdatabase();
        }


        static void testEFdatabase()
        {
            string dbFile = @"D:\OneDB\sconfig.mdf";


            dbConfManager config = new dbConfManager();

            dbServerItem server = new dbServerItem();

            server.Name = "new server";
            for (int i = 1; i < 10; i++)
            {
                dbTagItem tag = new dbTagItem("name " + i.ToString(), "path " + i.ToString());
                server.opcMonitoredTags.Add(tag);
            }


            config.Save(server, dbFile);

            System.Console.WriteLine("Database is created ... ");
            System.Console.ReadKey();

            dbServerItem s = config.Load(dbFile);

            foreach (dbTagItem tag in s.opcMonitoredTags)
            {
                System.Console.WriteLine("Server name = {0}", tag.Name);
            }


            System.Console.ReadKey();
        }
    }
}
