using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Moq;
using NUnit.Framework;

namespace DocumentStore.Tests
{
    public class DbConnectionExtensionShould
    {
        private DbProviderFactory _dbProviderFactory;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            var databaseFile = Path.GetTempFileName();
            _connectionString = string.Format("Data Source={0};Persist Security Info=False", databaseFile);
            _dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
            WithConnection(CreateSchema);
        }

        private static void CreateSchema(IDbConnection connection)
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "CreatedAt datetime not null, " +
                               "UpdatedAt datetime, " +
                               "Data ntext not null)";
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        [Test]
        public void InsertDocumentSuccessfully()
        {
            var album = GivenAValidAlbum();
            WithConnection((connection) => connection.InsertDocument(album));
            ThenIsShouldExistInTheDatabase(album);
        }

        [Test]
        public void RequireIdPropertyOnDocumentObject()
        {

        }

        private void ThenIsShouldExistInTheDatabase(Album album)
        {
            var documentHolder = new DocumentHolder();
            WithConnection(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "select Id, Data from Documents where Id = ?";
                var parameter = command.CreateParameter();
                parameter.DbType = DbType.String;
                parameter.Value = album.Id.ToString();
                command.Parameters.Add(parameter);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    documentHolder.Id = reader.GetString(reader.GetOrdinal("Id"));
                    documentHolder.Data = reader.GetString(reader.GetOrdinal("Data"));
                }
            });
        }

        private void WithConnection(Action<IDbConnection> execute)
        {
            using (var connection = GivenAnOpenConnection())
            {
                execute(connection);
            }
        }

        private static Album GivenAValidAlbum()
        {
            return new Album {Id = 1, Artist = "Richard Thompson", Title = "1000 Years od Popular Music", ReleaseDate = new DateTime(2001, 02, 01)};
        }        

        public DbConnection GivenAnOpenConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            connection.ConnectionString = _connectionString;
            connection.Open();
            return connection;
        }
    }
}
