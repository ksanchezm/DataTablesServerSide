using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Request
{
    public class DataTablesRequestSort
    {
        //Sets the order on with the Sort will be applied
        public int Order { get; set; }
        //Name of the column used for the sorting 
        public string Column { get; set; }
        //Direction on which to apply the sort
        public SortDirection Direction { get; set; }
    }
    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }
}
