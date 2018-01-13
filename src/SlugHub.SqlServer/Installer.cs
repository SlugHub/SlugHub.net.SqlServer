using Dapper;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace SlugHub.SqlServer
{
    internal class Installer
    {
        public static void InstallSqlTable(string connectionString, SqlServerSlugStoreOptions options)
        {
            var script = GetStringResource(typeof(SqlServerSlugStore).GetTypeInfo().Assembly, "SlugHub.SqlServer.Install.sql");

            //do replaces
            script = script.Replace("$(SCHEMA_NAME)", options.TableSchema);
            script = script.Replace("$(TABLE_NAME)", options.TableName);

            using (var connection = new SqlConnection(connectionString))
            {
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        connection.Execute(script);
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 1205)
                        {
                            //no trace in .net standard
                            //Trace.WriteLine("Deadlock occurred during automatic migration execution. Retrying...");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Requested resource `{resourceName}` was not found in the assembly `{assembly}`.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
