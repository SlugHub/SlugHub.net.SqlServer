using SlugHub.SqlServer;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace SlugHub.SqlServer.ConsoleAppSample
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("This sample will create a slug for a piece of text, with 1000 iterations");
            Console.WriteLine("Please press any key to continue.");
            Console.WriteLine("");

            Console.ReadKey();

            RunSqlServerSlugStore();
        }

        private static void RunSqlServerSlugStore()
        {
            Console.Clear();

            SetupLocalDbIfRequired();

            var slugGenerator = new SlugGenerator(
                new SlugGeneratorOptions { IterationSeedValue = 1000 },
                new SqlServerSlugStore(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString));

            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("");
            Console.WriteLine("Creating slugs...");

            for (var i = 1; i <= 1000; i++)
            {
                var slug = slugGenerator.GenerateSlug("Some text that needs slugging " + i);
                Console.Write("\r{0} slugs created", i);
            }

            stopwatch.Stop();

            Console.WriteLine("");
            Console.WriteLine("Took " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine("");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            Environment.Exit(0);
        }

        private static void SetupLocalDbIfRequired()
        {
            Console.WriteLine("Setting up SQL Server DB");

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"];

            if (!connectionString.ConnectionString.Contains("localdb"))
                return;

            var dbFileName = AppDomain.CurrentDomain.BaseDirectory + "SlugHub.mdf";

            var connection = new SqlConnection("server=(localdb)\\mssqllocaldb");

            using (connection)
            {
                connection.Open();

                var dropDbSql = @"USE master
                    IF EXISTS(select * from sys.databases where name = 'SlugHub_Test')
                    DROP DATABASE SlugHub_Test";

                new SqlCommand(dropDbSql, connection).ExecuteNonQuery();

                var createDbSql = @"
                    CREATE DATABASE SlugHub_Test
                    ON PRIMARY (NAME=SlugHub_Test, FILENAME = '" + dbFileName + "')";

                new SqlCommand(createDbSql, connection).ExecuteNonQuery();
                Console.WriteLine("SQL Server DB Created");
            }
        }
    }
}