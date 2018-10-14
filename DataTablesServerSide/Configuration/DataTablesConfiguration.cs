using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTablesServerSide.Configuration.Attributes;

namespace DataTablesServerSide.Configuration
{
    //Models the DataTable.net configuration for a given type. 
    //This configuration is used to build the DataTable grid and also 
    //when processing DataTables.net server-side requests.
    public class DataTablesConfiguration<T>
    {
        /*STATIC CODE*/

        protected static readonly string DEFAULT_CONFIG_CACHE_KEY = "";
        protected static object _configurationCacheLock = new object();
        protected static IDictionary<string, DataTablesConfiguration<T>> _configurationCache = new Dictionary<string, DataTablesConfiguration<T>>();
        /// <summary>
        /// Adds the configuration to the Configuration cache. If there is an existing configuration with the same name it will be overwritten.
        /// </summary>
        /// <param name="configuration"></param>
        public static void Register(DataTablesConfiguration<T> configuration)
        {
            lock (_configurationCacheLock)
            {
                string cacheKey = configuration.Name ?? "";
                _configurationCache[cacheKey] = configuration;
            }
        }

        /// <summary>
        /// Removes the specified configuration from the cache
        /// </summary>
        /// <param name="configuration"></param>
        public static void Remove(DataTablesConfiguration<T> configuration)
        {
            Remove(configuration.Name);
        }

        /// <summary>
        /// Removes the configuration with the specified name from the cache
        /// </summary>
        /// <param name="configurationName"></param>
        public static void Remove(string configurationName)
        {
            lock (_configurationCacheLock)
            {
                if (_configurationCache.ContainsKey(configurationName))
                {
                    _configurationCache.Remove(configurationName);
                }
            }
        }

        /// <summary>
        /// Gets a configuration for the given name from the cache. If the configuration is not found, 
        /// then a configuration is created and added to the cache, then returned to the user.
        /// </summary>
        /// <param name="configurationName"></param>
        /// <returns></returns>
        public static DataTablesConfiguration<T> Get(string configurationName)
        {
            DataTablesConfiguration<T> config = null;
            bool configExists = false;
            lock (_configurationCacheLock)
            {
                configExists = _configurationCache.TryGetValue(configurationName, out config) && config != null;
                if (!configExists)
                {
                    config = Create(configurationName);
                    _configurationCache[configurationName] = config;
                }
            }
            return config;
        }

        public static DataTablesConfiguration<T> GetDefault()
        {
            return Get(DEFAULT_CONFIG_CACHE_KEY);
        }

        /// <summary>
        /// Creates a Configuration by analyzing the generic Type parameter
        /// </summary>
        /// <param name="configurationName"></param>
        /// <returns></returns>
        public static DataTablesConfiguration<T> Create(string configurationName)
        {
            Type baseType = typeof(T);

            //Check if there is any configuration attribute with the specified configuration name
            //if there is not, we return early
            bool isDefined = baseType.CustomAttributes.OfType<DataTablesAttribute>()
                                     .Concat(baseType.GetMembers().SelectMany(m => m.GetCustomAttributes(true)
                                                                                    .OfType<DataTablesAttribute>()))
                                     .Any(attr => attr.ConfigurationName == configurationName);
            if (!isDefined) return null;

            var dtAttribute = baseType.GetCustomAttributes(true).OfType<DataTablesAttribute>()//.FirstOrDefault();
                                                                .FirstOrDefault(attr => attr.ConfigurationName == configurationName);

            DataTablesConfiguration<T> configuration = new DataTablesConfiguration<T>();
            configuration.Name = configurationName;
            configuration.Table = dtAttribute?.Table ?? baseType.Name;

            var propMembers = baseType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                      .Select(p => new { p.Name, Type = p.PropertyType, CustomAttributes = p.GetCustomAttributes(true) });
            var fieldMembers = baseType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                       .Select(f => new { f.Name, Type = f.FieldType, CustomAttributes = f.GetCustomAttributes(true) });
            var propAndFields = propMembers.Concat(fieldMembers);
            foreach (var member in propAndFields)
            {
                //if the member is annotated with the IgnoreDataTablesMapping attribute then we skip it
                if (member.CustomAttributes.OfType<IgnoreDataTablesMappingAttribute>().Any(attr => attr.ConfigurationName == configurationName || attr.IgnoreAlways))
                {
                    continue;
                }

                DataTablesColumnConfiguration colConfiguration = new DataTablesColumnConfiguration();
                var colDtAttribute = member.CustomAttributes.OfType<DataTablesAttribute>().FirstOrDefault(attr => attr.ConfigurationName == configurationName);

                colConfiguration.Field = member.Name;
                colConfiguration.Column = colDtAttribute?.Column ?? member.Name;
                colConfiguration.Title = colDtAttribute?.Title ?? Utils.StringExtensions.SplitUppercase(member.Name);
                colConfiguration.Sortable = colDtAttribute?.Sortable ?? false;
                colConfiguration.Searchable = colDtAttribute?.Searchable ?? false;
                colConfiguration.GloballySearchable = colDtAttribute?.GloballySearchable ?? false;
                colConfiguration.Visible = colDtAttribute?.Visible ?? true;
                colConfiguration.Type = member.Type;

                configuration.Columns.Add(colConfiguration);
            }

            return configuration;

        }

        /*END STATIC CODE*/

        /// <summary>
        /// Name of the configuration. A type can have many different configurations with distinct names. The default configuration has an empty string
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Name of the SQL Table or view mapped by the target Type
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// NAme of the SQL schema for the Table/View specified in the Table property
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Configuration of the columns that make up the datatables configuration
        /// </summary>
        public IList<DataTablesColumnConfiguration> Columns { get; set; }

        public DataTablesColumnConfiguration Column(string fieldName)
        {
            return Columns.FirstOrDefault(cc => cc.Field == fieldName);
        }

        public DataTablesConfiguration()
        {
            Name = "";
            Columns = new List<DataTablesColumnConfiguration>();

        }

        public DataTablesConfiguration<T> Clone()
        {
            return new DataTablesConfiguration<T>()
            {
                Name = this.Name,
                Table = this.Table,
                Columns = this.Columns.Select(c => c.Clone()).ToList()
            };
        }
    }
}
