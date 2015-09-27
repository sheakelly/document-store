using System;

namespace DocumentStore.Tests
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime ReleaseDate { get; set; }

        protected bool Equals(Album other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Title, other.Title) && string.Equals(Artist, other.Artist) && ReleaseDate.Equals(other.ReleaseDate);
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
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Title: {1}, Artist: {2}, ReleaseDate: {3}", Id, Title, Artist, ReleaseDate);
        }
    }
}