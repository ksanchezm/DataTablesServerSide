using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Configuration.Attributes
{
    /// <summary>
    /// Field or properties marked with this attribute will be ignored when generating the DataTableConfiguration for a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class IgnoreDataTablesMappingAttribute : Attribute
    {
        public IgnoreDataTablesMappingAttribute(string configurationName = "", bool ignoreAlways = false)
        {
            ConfigurationName = configurationName;
            IgnoreAlways = ignoreAlways;
        }
        /// <summary>
        /// The name of the DataTablesConfiguration where the field will be ignored from the mapping
        /// </summary>
        public string ConfigurationName { get; protected set; }
        /// <summary>
        /// If this flag is set to true the field will be ignored for all the DataTablesConfiguration mappings
        /// </summary>
        public bool IgnoreAlways { get; protected set; }
    }
}
