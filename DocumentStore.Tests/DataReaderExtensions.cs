using System;
using System.Data;

namespace DocumentStore.Tests
{
    public static class DataReaderExtensions
    {
        public static DateTime? GetNullableDateTime(this IDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) ? reader.GetDateTime(ordinal) : (DateTime?) null;
        }
    }
}
