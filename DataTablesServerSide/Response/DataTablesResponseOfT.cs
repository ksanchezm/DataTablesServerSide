using DataTablesServerSide.Response.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Response
{
    public class DataTablesResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Error { get; set; }
        [JsonIgnore]
        public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
        [JsonIgnore]
        public IEnumerable<string> DataPropertyNames { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                ContractResolver = new DataTablesResponseContractResolver<T>(HasError, DataPropertyNames)
            });
        }
        public override string ToString()
        {
            return this.ToJson();
        }
    }
}
