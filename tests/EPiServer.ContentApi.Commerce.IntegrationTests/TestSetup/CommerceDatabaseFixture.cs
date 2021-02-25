using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace EPiServer.ContentApi.IntegrationTests.Commerce.TestSetup
{
    public class CommerceDatabaseFixture
    {
        public string DatabaseName => "Commerce_Test";
        protected string MDF_FILE_PATH => @"..\..\..\Resources\db_commerce_template.mdf";
        protected string DESTINATION_MDF_FILE => Path.Combine(DESTINATION_MDF_DIRECTORY, "commerce.mdf");
        protected string DESTINATION_MDF_DIRECTORY => Path.Combine(Environment.CurrentDirectory, "database");

        public CommerceDatabaseFixture()
        {
            CopyDatabaseFiles();
            CreateDatabase();
        }

        public void Dispose()
        {
            TearDownDatabase();
        }

        private void CreateDatabase()
        {
            ExecuteSqlCommand($@"CREATE DATABASE [{DatabaseName}]
                        ON PRIMARY ( FILENAME =  '{Path.GetFullPath(DESTINATION_MDF_FILE)}' )
                        FOR ATTACH");
            ExecuteSqlCommand($@"ALTER DATABASE[{DatabaseName}] SET READ_WRITE WITH NO_WAIT");
        }

        private void CopyDatabaseFiles()
        {
            var dir = Directory.CreateDirectory(DESTINATION_MDF_DIRECTORY);
            var sec = dir.GetAccessControl();
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            dir.SetAccessControl(sec);
            File.Copy(MDF_FILE_PATH, DESTINATION_MDF_FILE, true);
        }

        private void ExecuteSqlCommand(string commandText)
        {
            using (var connection = new SqlConnection(ConnectionStringBuilder.ToString()))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        private List<T> ExecuteSqlQuery<T>(string queryText, Func<SqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(ConnectionStringBuilder.ToString()))
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

        private void TearDownDatabase()
        {
            var fileNames = ExecuteSqlQuery($@"SELECT [physical_name] FROM [sys].[master_files] WHERE [database_id] = DB_ID('{DatabaseName}')", row => (string)row["physical_name"]);

            if (fileNames.Any())
            {
                ExecuteSqlCommand($@"ALTER DATABASE [{DatabaseName}]
                        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        EXEC sp_detach_db '{DatabaseName}'");
                fileNames.ForEach(File.Delete);
            }
        }

        private static SqlConnectionStringBuilder ConnectionStringBuilder => new SqlConnectionStringBuilder
        {
            DataSource = @"(LocalDb)\MSSQLLocalDB",
            InitialCatalog = "master",
            IntegratedSecurity = true
        };
    }
}