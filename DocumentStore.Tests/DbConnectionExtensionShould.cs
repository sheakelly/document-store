using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Newtonsoft.Json;
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
        public void InsertDocumentAsJson()
        {
            var album = GivenAValidAlbum();

            var wasSuccessful = InsertDocument(album);
            
            var document = GetDocumentById(album.Id);
            Assert.That(wasSuccessful, Is.True);
            var insertedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(insertedAlbum, Is.EqualTo(album));
        }

        [Test]
        public void SetCreatedAtWhenDocumentIsInserted()
        {
            var album = GivenAValidAlbum();
            var time = DateTime.Now;

            InsertDocument(album);

            var document = GetDocumentById(album.Id);            
            Assert.That(document.CreatedAt, Is.GreaterThan(time));
        }

        [Test]
        public void RequireIdPropertyOnDocumentWhenInserting()
        {
            var album = new AlbumWithoutId();

            Assert.Throws<ArgumentException>(() => InsertDocument(album));
        }

        [Test]
        public void RequireNotNullIdPropertyOnDocument()
        {
            var album = GivenAValidAlbum();
            album.Id = null;

            Assert.Throws<ArgumentException>(() => InsertDocument(album));
        }

        [Test]
        public void UpdateDocument()
        {
            var album = GivenAlbumExists();
            album.Artist = "New artist";

            var result = UpdateDocument(album);

            var document = GetDocumentById(album.Id);
            var updatedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(updatedAlbum.Artist, Is.EqualTo("New artist"));
            Assert.That(result, Is.True);
        }

        [Test]
        public void SetUpdatedAtWhenDocumentIsUpdated()
        {
            var time = DateTime.Now;
            var album = GivenAlbumExists();
            album.Title = "A new title";

            UpdateDocument(album);

            var document = GetDocumentById(album.Id);
            Assert.That(document.UpdatedAt, Is.GreaterThan(time));
        }

        [Test]
        public void UpdateDocumentThatDoesNotExistShouldReturnFalse()
        {
            var album = GivenAValidAlbum();

            Assert.That(UpdateDocument(album), Is.False);
        }

        [Test]
        public void GetDocumentByIdWhenSuccessful()
        {
            var album = GivenAlbumExists();

            Album result = null;
            WithConnection(connection => result = connection.GetDocumentById<Album>(album.Id));

            Assert.That(result, Is.EqualTo(album));
        }

        [Test]
        public void ThrowExceptionWhenDocumentDoesNotExist()
        {
            var idThatDoesNotExist = Guid.NewGuid().ToString();

            Assert.Throws<Exception>(() => WithConnection(con => con.GetDocumentById<Album>(idThatDoesNotExist)));
        }

        [Test]
        public void InsertDocumentWhenUpsertedAndDoesNotExists()
        {
            var album = GivenAValidAlbum();

            var wasSuccessful = UpsertDocument(album);

            var document = GetDocumentById(album.Id);
            Assert.That(wasSuccessful, Is.True);
            var insertedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(insertedAlbum, Is.EqualTo(album));
        }

        [Test]
        public void UpdateDocumentWhenUpsertedAndAlreadyExists()
        {
            var album = GivenAlbumExists();
            album.Artist = "New artist";

            var result = UpsertDocument(album);

            var document = GetDocumentById(album.Id);
            var updatedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(updatedAlbum.Artist, Is.EqualTo("New artist"));
            Assert.That(result, Is.True);
        }

        private bool UpsertDocument(Album album)
        {
            var success = false;
            WithConnection(connection => success = connection.UpsertDocument(album));
            return success;
        }

        private bool UpdateDocument(Album album)
        {
            var success = false;
            WithConnection(connection => success = connection.UpdateDocument(album));
            return success;
        }

        private Album GivenAlbumExists()
        {
            var album = GivenAValidAlbum();
            InsertDocument(album);
            return album;
        }

        private Document GetDocumentById(string id)
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

        private bool InsertDocument(object document)
        {
            var success = false;
            WithConnection(connection => success = connection.InsertDocument(document));
            return success;
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
            return new Album {Id = Guid.NewGuid().ToString(), Artist = "Richard Thompson", Title = "1000 Years od Popular Music", ReleaseDate = new DateTime(2001, 02, 01)};
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
