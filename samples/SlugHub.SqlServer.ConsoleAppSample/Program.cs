using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using SlugHub.SqlServer;

namespace SlugHub.SqlServer.ConsoleAppSample
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("This sample will create a slug for a piece of text, with 1000 iterations");
            Console.WriteLine("Please press any key to continue.");
            Console.WriteLine("");

            Console.ReadKey();

            await RunSqlServerSlugStore();
        }

        private static async Task RunSqlServerSlugStore()
        {
            Console.Clear();

            var conn = @"Server=(localdb)\mssqllocaldb;Database=SlugHub_Test;Trusted_Connection=True;Integrated Security=True";

            SetupLocalDbIfRequired(conn);

            var slugGenerator = new SlugGenerator(
                new SlugGeneratorOptions { IterationSeedValue = 1000 },
                new SqlServerSlugStore(conn));

            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("");
            Console.WriteLine("Creating slugs without grouping keys...");

            for (var i = 1; i <= 1000; i++)
            {
                var individualStopWatch = Stopwatch.StartNew();
                var slug = await slugGenerator.GenerateSlugAsync("Some text that needs slugging " + i);
                individualStopWatch.Stop();

                Console.Write($"\r{i} slugs created ({individualStopWatch.ElapsedMilliseconds}ms)");
            }

            stopwatch.Stop();

            Console.WriteLine("");
            Console.WriteLine("Took " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine("");

            Console.WriteLine("Creating slugs with grouping keys...");

            for (var i = 1; i <= 1000; i++)
            {
                var slug = await slugGenerator.GenerateSlugAsync("Some text that needs slugging " + i, "group1");
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

        private static void SetupLocalDbIfRequired(string connectionString)
        {
            Console.WriteLine("Setting up SQL Server DB");

            if (!connectionString.Contains("localdb"))
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