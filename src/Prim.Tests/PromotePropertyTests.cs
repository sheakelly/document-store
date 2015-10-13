using System;
using NUnit.Framework;

namespace Prim.Tests
{
    public class PromotePropertyTests
    {
        private TestFixture _fixture;

        [SetUp]
        public void SetUp()
        {            
            Configure.PromotedPropertyExpressions.Clear();
        }

        [Test]
        public void PromotePropertyInsertedIntoColumn()
        {
            var album = GivenAnInsertedAlbumWithPromotedReleaseDate();

            var documentData = _fixture.GetDocumentDataById(album.Id);
            Assert.That(documentData["ReleaseDate"], Is.EqualTo(album.ReleaseDate.ToString()));
        }       

        [Test]
        public void PromoteNestedPropertyInsertedIntoColumn()
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "Data ntext not null," +
                               "ProducerName nvarchar(255))";
            _fixture = new TestFixture(sql);
            Configure.Promote<Album>(a => a.Producer.Name, "ProducerName");
            var album = _fixture.GivenAValidAlbum();

            _fixture.InsertDocument(album);

            var documentData = _fixture.GetDocumentDataById(album.Id);
            Assert.That(documentData["ProducerName"], Is.EqualTo(album.Producer.Name));
        }

        [Test]
        public void PromotedPropertyStoreOnUpdate()
        {
            var album = GivenAnInsertedAlbumWithPromotedReleaseDate();
            var newReleaseDate = new DateTime(2013, 9, 12);
            album.ReleaseDate = newReleaseDate;

            _fixture.UpdateDocument(album);

            Assert.That(_fixture.GetDocumentDataById(album.Id)["ReleaseDate"], Is.EqualTo(newReleaseDate.ToString()));
        }

        private Album GivenAnInsertedAlbumWithPromotedReleaseDate()
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "Data ntext not null," +
                               "ReleaseDate nvarchar(255))";
            _fixture = new TestFixture(sql);
            Configure.Promote<Album>(a => a.ReleaseDate, "ReleaseDate");
            var album = _fixture.GivenAValidAlbum();

            _fixture.InsertDocument(album);

            return album;
        }

        [Test]
        public void MultiplePromotedProperties()
        {
            Configure.Promote<Album>(a => a.Producer.Name, "ProducerName");
            Configure.Promote<Album>(a => a.Artist);
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "Data ntext not null," +
                               "ProducerName nvarchar(255)," +
                               "Artist nvarchar(255))";
            _fixture = new TestFixture(sql);
            var album = _fixture.GivenAValidAlbum();

            _fixture.InsertDocument(album);

            Assert.That(_fixture.GetDocumentDataById(album.Id)["ProducerName"], Is.EqualTo(album.Producer.Name));
            Assert.That(_fixture.GetDocumentDataById(album.Id)["Artist"], Is.EqualTo(album.Artist));
        }

//        [Test]
//        public void QueryByPromotedProperties()
//        {
//            Configure.Promote<Album>(a => a.Artist);
//            const string sql = "create table Documents (" +
//                               "Id nvarchar(255) not null primary key, " +
//                               "Data ntext not null," +
//                               "ProducerName nvarchar(255)," +
//                               "Artist nvarchar(255))";
//            _fixture = new TestFixture(sql);
//            var album = _fixture.GivenAlbumExists();
//
//            _fixture.FindDocuments(album);
//
//            Assert.That(_fixture.GetDocumentDataById(album.Id)["ProducerName"], Is.EqualTo(album.Producer.Name));
//            Assert.That(_fixture.GetDocumentDataById(album.Id)["Artist"], Is.EqualTo(album.Artist));
//        }
    }
}
