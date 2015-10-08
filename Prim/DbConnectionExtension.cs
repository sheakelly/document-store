using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Prim
{    
    public static class DbConnectionExtension
    {
        public static bool InsertDocument<T>(this IDbConnection connection, T document)
        {
            var command = connection.CreateCommand();
            command.CommandText = GenerateInsertStatement(typeof(T));
            BindParameter(command, DbType.String, GetIdPropertyValue(document));
            BindParameter(command, DbType.String, SerialiseDocument(document));
            BindPromotedPropertiesIfRequired(command, document);
            return command.ExecuteNonQuery() == 1;            
        }

        private static string GenerateInsertStatement(Type type)
        {
            var targets = Configure.EnumeratePromotedPropertyTargets(type);            
            var builder = new StringBuilder();
            builder.Append("insert into Documents (Id, Data");
            var enumerable = targets as Target[] ?? targets.ToArray();
            foreach (var target in enumerable)
            {
                builder.Append(", ").Append(target.ColumnName);
            }
            builder.Append(") values (?, ?");
            for (var i = 0; i < enumerable.Length; i++)
            {
                builder.Append(",?");
            }
            builder.Append(")");
            return builder.ToString();
        }

        private static void BindPromotedPropertiesIfRequired<T>(IDbCommand command, T document)
        {
            var propertyTargets = Configure.EnumeratePromotedPropertyTargets(typeof(T));            
            foreach (var propertyTarget in propertyTargets)
            {
                var value = propertyTarget.GetValue(document);
                if (value != null)
                {
                    BindParameter(command, DbType.String, value);    
                }                
            }
        }

        private static void BindParameter(IDbCommand command, DbType dbType, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = dbType;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public static bool UpdateDocument<T>(this IDbConnection connection, T document)
        {
            var command = connection.CreateCommand();
            command.CommandText = GenerateUpdateStatement<T>();                        
            BindParameter(command, DbType.String, SerialiseDocument(document));            
            var idValue = GetIdPropertyValue(document);
            BindPromotedPropertiesIfRequired(command, document);
            BindParameter(command, DbType.String, idValue);
            return command.ExecuteNonQuery() == 1;
        }

        private static string GenerateUpdateStatement<T>()
        {
            var builder = new StringBuilder();
            builder.Append("update Documents set Data = ?");
            var targets = Configure.EnumeratePromotedPropertyTargets(typeof(T));
            foreach (var target in targets)
            {
                builder.Append(", ");
                builder.Append(target.ColumnName);
                builder.Append(" = ?");                
            }
            builder.Append(" where Id = ?");
            return builder.ToString();
        }

        private static string SerialiseDocument(object document)
        {
            return JsonConvert.SerializeObject(document);
        }

        /// <summary>
        /// Convenience method that checks for the existence of document first and 
        /// inserts or updates the document appropriately
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static bool UpsertDocument<T>(this IDbConnection connection, T document)
        {
            var command = connection.CreateCommand();
            command.CommandText = "select count(*) from Documents where id = ?";
            BindParameter(command, DbType.String, GetIdPropertyValue(document));
            var count = (int) command.ExecuteScalar();
            return count == 0 ? InsertDocument(connection, document) : UpdateDocument(connection, document);
        }

        public static void DeleteDocument<T>(this IDbConnection connection, T document)
        {
          var command = connection.CreateCommand();
          command.CommandText = "delete Documents where Id = ?";
          command.Parameters.Add(GetIdPropertyValue(document));
          command.ExecuteNonQuery();
        }

        public static T GetDocumentById<T>(this IDbConnection connection, string id)
        {
            var command = connection.CreateCommand();
            command.CommandText = "select Data from Documents where id = ?";            
            BindParameter(command, DbType.String, id);
            using(var reader = command.ExecuteReader())
            {
                if(!reader.Read()) throw new Exception(string.Format("Unable to find document for id '{0}'", id));
                var data = reader.GetString(reader.GetOrdinal("Data"));;
                return JsonConvert.DeserializeObject<T>(data);
            }
        }

        private static string GetIdPropertyValue(object document)
        {
            var properties = document.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var idProperty = properties.FirstOrDefault(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
            if (idProperty == null)
            {
                throw new ArgumentException(string.Format("Document of type '{0}' is missing id property", document.GetType().FullName));
            }
            var idValue = idProperty.GetValue(document);
            if (idValue == null)
            {
                throw new ArgumentException(string.Format("Document of type '{0}' must have a id with a value that is not null", document.GetType().FullName));
            }
            return idValue.ToString();
        }
    }
}
