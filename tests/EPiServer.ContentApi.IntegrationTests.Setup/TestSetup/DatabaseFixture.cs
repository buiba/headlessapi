using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class DatabaseFixture : IDisposable
    {
        private readonly string CMS_MDF_FILE_PATH = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\Resources\db_cms_template.mdf");
        private readonly string DESTINATION_MDF_DIRECTORY = Path.Combine(Environment.CurrentDirectory, "database");
        private readonly string DESTINATION_MDF_FILE;

        public DatabaseFixture(string databaseName = null)
        {
            DESTINATION_MDF_FILE = Path.Combine(DESTINATION_MDF_DIRECTORY, "cms.mdf");
            CmsDatabase = databaseName ?? $"CD_Test_{DateTime.Now.Ticks}";
            CopyDatabaseFiles();
            CreateDatabase(DESTINATION_MDF_FILE);
        }

        public void Dispose()
        {
            TearDownDatabase(CmsDatabase);
        }

        public string CmsDatabase { get; }

        private void CopyDatabaseFiles()
        {
            var dir = Directory.CreateDirectory(DESTINATION_MDF_DIRECTORY);
            var sec = dir.GetAccessControl();
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            dir.SetAccessControl(sec);
            File.Copy(CMS_MDF_FILE_PATH, DESTINATION_MDF_FILE, true);
        }

        private void CreateDatabase(string mdfPath)
        {
            ExecuteSqlCommand(ConnectionString, $@"CREATE DATABASE [{CmsDatabase}] 
                        ON PRIMARY ( FILENAME =  '{Path.GetFullPath(mdfPath)}' )
                        FOR ATTACH");
            ExecuteSqlCommand(ConnectionString, $@"ALTER DATABASE[{CmsDatabase}] SET READ_WRITE WITH NO_WAIT");
        }

        private void TearDownDatabase(string databaseName)
        {
            var fileNames = ExecuteSqlQuery(ConnectionString, $@"SELECT [physical_name] FROM [sys].[master_files] WHERE [database_id] = DB_ID('{databaseName}')", row => (string)row["physical_name"]);

            if (fileNames.Any())
            {
                ExecuteSqlCommand(ConnectionString, $@"ALTER DATABASE [{databaseName}] 
                        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        EXEC sp_detach_db '{databaseName}'");
                fileNames.ForEach(File.Delete);
            }
        }

        private static void ExecuteSqlCommand(SqlConnectionStringBuilder stringBuilder, string commandText)
        {
            using (var connection = new SqlConnection(stringBuilder.ToString()))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static List<T> ExecuteSqlQuery<T>(SqlConnectionStringBuilder stringBuilder, string queryText, Func<SqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(stringBuilder.ToString()))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryText;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(read(reader));
                        }
                    }
                }
            }

            return result;
        }

        private static SqlConnectionStringBuilder ConnectionString => new SqlConnectionStringBuilder
        {
            DataSource = @"(LocalDb)\MSSQLLocalDB",
            InitialCatalog = "master",
            IntegratedSecurity = true
        };
    }
}
