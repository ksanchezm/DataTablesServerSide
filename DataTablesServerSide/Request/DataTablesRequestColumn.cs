using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Request
{
    public class DataTablesRequestColumn
    {
        public string Data { get; set; }
        public string Name { get { return Data; } }
        public DataTablesRequestSearch Search { get; set; }
        public bool HasSearch { get { return DataTablesRequestSearch.HasSearch(this.Search); } }
    }
}
