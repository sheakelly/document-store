using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Prim.Tests
{
    public class DbConnectionExtensionTests
    {
        private TestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "Data ntext not null)";
            _fixture = new TestFixture(sql);
        }       

        [Test]
        public void InsertDocumentAsJson()
        {
            var album = _fixture.GivenAValidAlbum();

            var wasSuccessful = _fixture.InsertDocument(album);

            var document = _fixture.GetDocumentById(album.Id);
            Assert.That(wasSuccessful, Is.True);
            var insertedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(insertedAlbum, Is.EqualTo(album));
        }        

        [Test]
        public void RequireIdPropertyOnDocumentWhenInserting()
        {
            var album = new AlbumWithoutId();

            Assert.Throws<ArgumentException>(() => _fixture.InsertDocument(album));
        }

        [Test]
        public void RequireNotNullIdPropertyOnDocument()
        {
            var album = _fixture.GivenAValidAlbum();
            album.Id = null;

            Assert.Throws<ArgumentException>(() => _fixture.InsertDocument(album));
        }

        [Test]
        public void UpdateDocumentSaveNewData()
        {
            var album = _fixture.GivenAlbumExists();
            album.Artist = "New artist";

            var result = _fixture.UpdateDocument(album);

            var document = _fixture.GetDocumentById(album.Id);
            var updatedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(updatedAlbum.Artist, Is.EqualTo("New artist"));
            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateDocumentThatDoesNotExistShouldReturnFalse()
        {
            var album = _fixture.GivenAValidAlbum();

            Assert.That(_fixture.UpdateDocument(album), Is.False);
        }

        [Test]
        public void GetDocumentByIdWhenSuccessful()
        {
            var album = _fixture.GivenAlbumExists();

            Album result = null;
            _fixture.WithConnection(connection => result = connection.GetDocumentById<Album>(album.Id));

            Assert.That(result, Is.EqualTo(album));
        }

        [Test]
        public void ThrowExceptionWhenDocumentDoesNotExist()
        {
            var idThatDoesNotExist = Guid.NewGuid().ToString();

            Assert.Throws<Exception>(() => _fixture.WithConnection(con => con.GetDocumentById<Album>(idThatDoesNotExist)));
        }

        [Test]
        public void InsertDocumentWhenUpsertedAndDoesNotExists()
        {
            var album = _fixture.GivenAValidAlbum();

            var wasSuccessful = _fixture.UpsertDocument(album);

            var document = _fixture.GetDocumentById(album.Id);
            Assert.That(wasSuccessful, Is.True);
            var insertedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(insertedAlbum, Is.EqualTo(album));
        }

        [Test]
        public void UpdateDocumentWhenUpsertedAndAlreadyExists()
        {
            var album = _fixture.GivenAlbumExists();
            album.Artist = "New artist";

            var result = _fixture.UpsertDocument(album);

            var document = _fixture.GetDocumentById(album.Id);
            var updatedAlbum = JsonConvert.DeserializeObject<Album>(document.Data);
            Assert.That(updatedAlbum.Artist, Is.EqualTo("New artist"));
            Assert.That(result, Is.True);
        }        
    }
}
