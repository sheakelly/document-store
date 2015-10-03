using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DocumentStore.Tests
{
    public class TestFixture
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;

        public TestFixture(string createDbItemsSql)
        {
            var databaseFile = Path.GetTempFileName();
            _connectionString = string.Format("Data Source={0};Persist Security Info=False", databaseFile);
            _dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
            WithConnection(connection => CreateSchema(connection, createDbItemsSql));
        }

        private static void CreateSchema(IDbConnection connection, string createDbItemsSql)
        {            
            var command = connection.CreateCommand();
            command.CommandText = createDbItemsSql;
            command.ExecuteNonQuery();
        }

        public bool UpsertDocument<T>(T document)
        {
            var success = false;
            WithConnection(connection => success = connection.UpsertDocument(document));
            return success;
        }

        public bool UpdateDocument<T>(T document)
        {
            var success = false;
            WithConnection(connection => success = connection.UpdateDocument(document));
            return success;
        }

        public Album GivenAlbumExists()
        {
            var album = GivenAValidAlbum();
            InsertDocument(album);
            return album;
        }

        public Document GetDocumentById(string id)
        {
            var document = new Document();
            WithConnection(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "select Id, Data, CreatedAt, UpdatedAt from Documents where Id = ?";
                var parameter = command.CreateParameter();
                parameter.DbType = DbType.String;
                parameter.Value = id;
                command.Parameters.Add(parameter);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Assert.Fail("Document with Id '{0}' does not exist in the database", id);
                    }
                    document.Id = reader.GetString(reader.GetOrdinal("Id"));
                    document.Data = reader.GetString(reader.GetOrdinal("Data"));
                    document.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                    document.UpdatedAt = reader.GetNullableDateTime("UpdatedAt");
                }
            });
            return document;
        }

        public IDictionary<string, object> GetDocumentDataById(string id)
        {
            var documentData = new Dictionary<string, object>();
            WithConnection(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "select * from Documents where Id = ?";
                var parameter = command.CreateParameter();
                parameter.DbType = DbType.String;
                parameter.Value = id;
                command.Parameters.Add(parameter);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Assert.Fail("Document with Id '{0}' does not exist in the database", id);
                    }                    
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        documentData.Add(reader.GetName(i), reader.GetValue(i));
                    }
                }
            });
            return documentData;
        }

        public bool InsertDocument<T>(T document)
        {
            var success = false;
            WithConnection(connection => success = connection.InsertDocument(document));
            return success;
        }

        public void WithConnection(Action<IDbConnection> execute)
        {
            using (var connection = GivenAnOpenConnection())
            {
                execute(connection);
            }
        }

        public Album GivenAValidAlbum()
        {
            return new Album { 
                Id = Guid.NewGuid().ToString(), 
                Artist = "Richard Thompson", 
                Title = "1000 Years od Popular Music", 
                ReleaseDate = new DateTime(2001, 02, 01),
                Producer = new Producer()
                {
                    Name = "Some Old Guy"
                }
            };
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
