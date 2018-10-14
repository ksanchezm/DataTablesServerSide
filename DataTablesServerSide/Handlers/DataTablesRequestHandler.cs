using DataTablesServerSide.Configuration;
using DataTablesServerSide.Request;
using DataTablesServerSide.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesServerSide.Handlers
{
    public abstract class DataTablesRequestHandler<T>
    {
        public DataTablesRequestHandler(string configurationName)
        {
            Configuration = DataTablesConfiguration<T>.Get(configurationName) ?? DataTablesConfiguration<T>.GetDefault();
        }
        public DataTablesRequestHandler(DataTablesConfiguration<T> configuration)
        {
            Configuration = configuration;
        }
        public DataTablesConfiguration<T> Configuration { get; set; }
        public Func<Exception, string> ErrorHandler { get; set; }
        public abstract DataTablesResponse<T> Handle(DataTablesRequest request);
    }
}
