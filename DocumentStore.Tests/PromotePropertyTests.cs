using NUnit.Framework;

namespace DocumentStore.Tests
{
    public class PromotePropertyTests
    {
        private TestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            
            Nifty.PromotedPropertyExpressions.Clear();
        }

        [Test]
        public void PromotePropertyInsertedIntoColumn()
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "CreatedAt datetime not null, " +
                               "UpdatedAt datetime, " +
                               "Data ntext not null," +
                               "ReleaseDate nvarchar(255))";
            _fixture = new TestFixture(sql);
            Nifty.Promote<Album>(a => a.ReleaseDate, "ReleaseDate");
            var album = _fixture.GivenAValidAlbum();

            _fixture.InsertDocument(album);

            var documentData = _fixture.GetDocumentDataById(album.Id);
            Assert.That(documentData["ReleaseDate"], Is.EqualTo(album.ReleaseDate.ToString()));
        }       

        [Test]
        public void PromoteNestedPropertyInsertedIntoColumn()
        {
            const string sql = "create table Documents (" +
                               "Id nvarchar(255) not null primary key, " +
                               "CreatedAt datetime not null, " +
                               "UpdatedAt datetime, " +
                               "Data ntext not null," +
                               "ProducerName nvarchar(255))";
            _fixture = new TestFixture(sql);
            Nifty.Promote<Album>(a => a.Producer.Name, "ProducerName");
            var album = _fixture.GivenAValidAlbum();

            _fixture.InsertDocument(album);

            var documentData = _fixture.GetDocumentDataById(album.Id);
            Assert.That(documentData["ProducerName"], Is.EqualTo(album.Producer.Name));
        }       
    }
}
