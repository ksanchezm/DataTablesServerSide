using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Configuration.Attributes
{
    /// <summary>
    /// Enables Atribute based configuration of the DataTables.net on the target Type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DataTablesAttribute : Attribute
    {
        public DataTablesAttribute(string configurationName = "", string table = null, string column = null, string title = null, bool sortable = false, bool searchable = false, bool visible = true, bool globallySearchable = false)
        {
            ConfigurationName = configurationName;
            Table = table;
            Column = column;
            Title = title;
            Sortable = sortable;
            Searchable = searchable;
            GloballySearchable = globallySearchable;
            Visible = visible;
        }

        public string ConfigurationName { get; protected set; }
        /// <summary>
        /// Name of the SQL Table or View used as source
        /// </summary>
        public string Table { get; protected set; }
        /// <summary>
        /// Name of the SQL schema where the table/view belongs
        /// </summary>
        public string Schema { get; protected set; }
        /// <summary>
        /// Name of the SQL column used as source for the datatable column
        /// </summary>
        public string Column { get; protected set; }
        /// <summary>
        /// Value used to display as column header
        /// </summary>
        public string Title { get; protected set; }
        /// <summary>
        /// Value that indicates whether the column is sortable
        /// </summary>
        public bool Sortable { get; protected set; }
        /// <summary>
        /// Value that indicates whether the column is searchable
        /// </summary>
        public bool Searchable { get; protected set; }
        /// <summary>
        /// Value that indicates whether the column should be incluided in datatable's global search.
        /// Should only be applied to String columns
        /// </summary>
        public bool GloballySearchable { get; protected set; }
        /// <summary>
        /// Value that indicates whether the column should be visible
        /// </summary>
        public bool Visible { get; protected set; }
    }
}
