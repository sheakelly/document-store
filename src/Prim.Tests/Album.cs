using System;

namespace Prim.Tests
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Producer Producer { get; set; }        

        protected bool Equals(Album other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Title, other.Title) && string.Equals(Artist, other.Artist) && ReleaseDate.Equals(other.ReleaseDate) && Equals(Producer, other.Producer);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Album) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Artist != null ? Artist.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ ReleaseDate.GetHashCode();
                hashCode = (hashCode*397) ^ (Producer != null ? Producer.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Title: {1}, Artist: {2}, ReleaseDate: {3}, Producer: {4}", Id, Title, Artist, ReleaseDate, Producer);
        }
    }
}