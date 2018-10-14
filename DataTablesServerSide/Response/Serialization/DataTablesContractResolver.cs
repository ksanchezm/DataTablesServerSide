using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Response.Serialization
{
    public class DataTablesResponseContractResolver<T> : DefaultContractResolver
    {
        public bool HasError { get; set; }
        public IEnumerable<string> DataTablesColumnNames { get; set; }

        public DataTablesResponseContractResolver()
        {
        }
        public DataTablesResponseContractResolver(bool hasError, IEnumerable<string> dataTablesColumnNames) : base()
        {
            HasError = hasError;
            DataTablesColumnNames = dataTablesColumnNames;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            IEnumerable<JsonProperty> propQuery = properties.AsEnumerable();

            if (type == typeof(DataTablesResponse<T>))
            {
                propQuery = HasError ? propQuery.Where(p => p.PropertyName == nameof(DataTablesResponse<T>.Draw) || p.PropertyName == nameof(DataTablesResponse<T>.Error))
                                      : propQuery.Where(p => p.PropertyName != nameof(DataTablesResponse<T>.Error));
                foreach (var property in propQuery)
                {
                    //camel case the properties
                    property.PropertyName = property.PropertyName.Substring(0, 1).ToLower() + property.PropertyName.Substring(1);
                }

            }
            else if (type == typeof(T))
            {
                propQuery = propQuery.Where(p => DataTablesColumnNames.Contains(p.PropertyName) || DataTablesColumnNames == null);
            }
            return propQuery.ToList();
        }
    }
}
