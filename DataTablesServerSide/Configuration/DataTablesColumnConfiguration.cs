using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Configuration
{
    public class DataTablesColumnConfiguration
    {
        /// <summary>
        /// Name of the SQL column in the data source
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// Name of the field or property in the target object
        /// </summary>
        public string Field { get; set; }
        public string Title { get; set; }
        public bool Searchable { get; set; }
        private bool _globallySearchable;
        /// <summary>
        /// True if the column will be taken into consideration for the global search.
        /// Will always return false if the column Type is not string
        /// </summary>
        public bool GloballySearchable
        {
            get { return _globallySearchable && Type == typeof(string); }
            set { _globallySearchable = value; }
        }
        public bool Sortable { get; set; }
        public bool Visible { get; set; }
        public Type Type { get; set; }

        public DataTablesColumnConfiguration Clone()
        {
            return new DataTablesColumnConfiguration()
            {
                Column = this.Column,
                Field = this.Field,
                Title = this.Title,
                Searchable = this.Searchable,
                Sortable = this.Sortable,
                Visible = this.Visible,
                Type = this.Type
            };
        }
    }
}
