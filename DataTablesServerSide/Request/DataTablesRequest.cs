using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Request
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DataTablesRequestSearch Search { get; set; }
        public List<DataTablesRequestSort> Sort { get; set; }
        public List<DataTablesRequestColumn> Columns { get; set; }
        public bool HasSort
        {
            get { return this.Sort?.Any() ?? false; }
        }
        public bool HasPagination
        {
            //If datatables request length parameter is -1 pagination is disabled
            get { return this.Length > -1; }
        }
        public bool HasSearch
        {
            get { return Columns.Any(c => DataTablesRequestSearch.HasSearch(c.Search)); }
        }
        public bool HasGlobalSearch
        {
            get { return DataTablesRequestSearch.HasSearch(this.Search); }
        }
        public DataTablesRequest()
        {
            Sort = new List<DataTablesRequestSort>();
            Columns = new List<DataTablesRequestColumn>();
        }
    }
}
