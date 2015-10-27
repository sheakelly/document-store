using NUnit.Framework;

namespace Prim.Tests
{
    public class TableNamingStrategyTests
    {
        private TestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            const string sql = "create table MyTable (" +
                               "Id nvarchar(255) not null primary key, " +
                               "Data ntext not null)";
            _fixture = new TestFixture(sql, "MyTable");
        }             

        [Test]
        public void ShouldBeAbleToConfigureTableName()
        {            
            Configure.UseTableNamingStrategy(t => "MyTable");
            var album = _fixture.GivenAlbumExists();

            var result = _fixture.GetDocumentById(album.Id);

            Assert.That(result.Data, Is.Not.Empty);
        }
    }
}
