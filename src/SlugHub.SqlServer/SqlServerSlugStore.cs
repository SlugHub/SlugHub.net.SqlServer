using Dapper;
using SlugHub.SlugStore;
using System;
using System.Data.SqlClient;

namespace SlugHub.SqlServer
{
    public class SqlServerSlugStore : ISlugStore
    {
        private readonly SqlServerSlugStoreOptions _options;
        private readonly string _connectionString;

        public SqlServerSlugStore(string nameOrConnectionString) 
            : this(nameOrConnectionString, new SqlServerSlugStoreOptions())
        { }

        public SqlServerSlugStore(string connectionString, SqlServerSlugStoreOptions options = null)
        {
            if (options == null)
                options = new SqlServerSlugStoreOptions();

            _options = options;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            Installer.InstallSqlTable(_connectionString, _options);
        }

        public bool Exists(string slug)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                var query = $"select count(slug) from [{_options.TableSchema}].[{_options.TableName}] where [Slug] = @slug";

                var slugResult = sqlConnection.ExecuteScalar<int>(query, new { slug });

                return slugResult > 0;
            }
        }

        public void Store(Slug slug)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                var command = $"insert into [{_options.TableSchema}].[{_options.TableName}]([Slug],[Created]) values (@Slug,@Created)";

                sqlConnection.Execute(command, new
                {
                    Slug = slug.Value,
                    slug.Created
                });

                sqlConnection.Close();
            }
        }
    }
}
