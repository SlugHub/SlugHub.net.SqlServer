using System;
using System.Data.SqlClient;
using Dapper;
using SlugHub.SlugStore;

namespace SlugHub.SqlServer
{
    public class SqlServerSlugStore : ISlugStore
    {
        private readonly SqlServerSlugStoreOptions _options;
        private readonly string _connectionString;
        private const string DefaultGroupingKey = "Default";

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

        public bool Exists(string slug, string groupingKey)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                object queryParameters;
                var queryText = $"select count(slug) from [{_options.TableSchema}].[{_options.TableName}] " +
                            $"where [Slug] = @slug";

                if (!string.IsNullOrEmpty(groupingKey))
                {
                    queryText = queryText + " and [GroupingKey] = @groupingKey";
                    queryParameters = new { slug, groupingKey };
                }
                else
                {
                    queryParameters = new { slug };
                }

                var slugResult = sqlConnection.ExecuteScalar<int>(queryText, queryParameters);

                return slugResult > 0;
            }
        }

        public void Store(Slug slug)
        {
            var groupingKey = string.IsNullOrEmpty(slug.GroupingKey)
                ? DefaultGroupingKey
                : slug.GroupingKey;

            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                sqlConnection.Open();

                var command = $"insert into [{_options.TableSchema}].[{_options.TableName}]" +
                    "([Slug],[GroupingKey],[Created]) values (@Slug,@GroupingKey,@Created)";

                sqlConnection.Execute(command, new
                {
                    Slug = slug.Value,
                    GroupingKey = groupingKey,
                    slug.Created
                });

                sqlConnection.Close();
            }
        }
    }
}
