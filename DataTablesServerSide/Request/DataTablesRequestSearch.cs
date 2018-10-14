using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Request
{
    public class DataTablesRequestSearch
    {
        public string Value { get; set; }
        public string Operator { get; set; }
        public bool IsRegex { get; set; }
        public static bool HasSearch(DataTablesRequestSearch s)
        {
            return s != null && s.Value != null;
        }
    }
}
