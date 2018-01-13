namespace SlugHub.SqlServer
{
    public class SqlServerSlugStoreOptions
    {
        public SqlServerSlugStoreOptions()
        {
            //defaults
            TableSchema = "SlugHub";
            TableName = "Slugs";
        }

        public string TableName { get; set; }

        public string TableSchema { get; set; }
    }
}