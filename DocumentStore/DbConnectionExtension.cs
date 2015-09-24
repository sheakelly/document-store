using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace DocumentStore
{
    /*
    Expected schema
    create table Document (
      Id varchar(255) not null primary key,
      CreateAt datetime not null,
      UpdatedAt datetime,
      Data varchar(max) not null
    )
    */
    public static class DbConnectionExtension
    {
        public static void InsertDocument<T>(this IDbConnection connection, T document)
        {
            var command = connection.CreateCommand();
            command.CommandText = "insert into Documents (Id, CreatedAt, Data) values (?, ?, ?)";
            AddParameter(command, DbType.String, GetIdPropertyValue(document));
            AddParameter(command, DbType.DateTime, DateTime.Now);
            AddParameter(command, DbType.String, SerialiseDocument(document));
            command.ExecuteNonQuery();
        }

        private static void AddParameter(IDbCommand command, DbType dbType, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = dbType;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static string SerialiseDocument(object document)
        {
            return JsonConvert.SerializeObject(document);
        }

        public static void UpdateDocument<T>(this IDbConnection connection, T document)
        {
            var command = connection.CreateCommand();
            command.CommandText = "update Documents set Data = ?, UpdatedAt = ? where Id = ?";
            command.Parameters.Add(SerialiseDocument(document));
            command.Parameters.Add(GetIdPropertyValue(document));
            command.Parameters.Add(DateTime.Now);
            command.ExecuteNonQuery();
        }

        public static void UpsertDocument<T>(this IDbConnection connection, T document)
        {
            throw new NotImplementedException();
        }

        public static void DeleteDocument<T>(this IDbConnection connection, T document)
        {
          var command = connection.CreateCommand();
          command.CommandText = "delete Documents where Id = ?";
          command.Parameters.Add(GetIdPropertyValue(document));
          command.ExecuteNonQuery();
        }

        public static T GetDocumentById<T, TId>(this IDbConnection connection, TId id)
        {
            var command = connection.CreateCommand();
            command.CommandText = "select data from Documents where id = ?";
            command.Parameters.Add(id);
            using(var reader = command.ExecuteReader())
            {
                return JsonConvert.DeserializeObject<T>(reader.GetString(0));
            }
        }

        private static string GetIdPropertyValue(object document)
        {
            var properties = document.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var idProperty = properties.FirstOrDefault(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
            if (idProperty == null)
            {
                throw new Exception(string.Format("Document of type '{0}' is missing id property", document.GetType().FullName));
            }
            var idValue = idProperty.GetValue(document);
            if (idValue == null)
            {
                throw new Exception(string.Format("Document of type '{0}' must have a id with a value that is not null", document.GetType().FullName));
            }
            return idValue.ToString();
        }
    }
}
