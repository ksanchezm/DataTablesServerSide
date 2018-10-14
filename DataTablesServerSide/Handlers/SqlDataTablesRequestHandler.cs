using DataTablesServerSide.Configuration;
using DataTablesServerSide.Request;
using DataTablesServerSide.Response;
using SqlKata.Compilers;
using SqlKata.Execution;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTablesServerSide.Handlers
{
    public class SqlDataTablesRequestHandler<T> : DataTablesRequestHandler<T>
    {
        protected IDbConnection Connection { get; set; }
        protected Compiler QueryCompiler { get; set; }
        public SqlDataTablesRequestHandler(IDbConnection connection) : base("")
        {
            Connection = connection;
        }
        public SqlDataTablesRequestHandler(IDbConnection connection, DataTablesConfiguration<T> configuration) : base(configuration)
        {
            Connection = connection;
        }
        public SqlDataTablesRequestHandler(IDbConnection connection, string configurationName) : base(configurationName)
        {
            Connection = connection;
        }
        public override DataTablesResponse<T> Handle(DataTablesRequest request)
        {
            try
            {
                var db = new QueryFactory(Connection, QueryCompiler)
                {
                    Logger = (s) => System.Diagnostics.Debug.WriteLine(s)
                };
                var query = CreateBaseQuery(request, db);
                var queryTotalCount = CreateTotalCountQuery(request, query);
                ProcessGlobalSearch(request, query);
                ProcessColumnSearch(request, query);
                var queryFilteredCount = CreateFilteredCountQuery(request, query);
                ProcessSort(request, query);
                ProcessPagination(request, query);
                var queryResult = query.Get<T>().ToList();
                var totalCount = queryTotalCount.Get<int>().FirstOrDefault();
                //Only query for the filtered count if the request included a filter;
                //Otherwise, use the total count.
                var filteredCount = (request.HasGlobalSearch || request.HasSearch)
                                        ? queryFilteredCount.Get<int>().FirstOrDefault()
                                        : totalCount;
                return new DataTablesResponse<T>()
                {
                    Draw = request.Draw,
                    Data = queryResult,
                    RecordsTotal = totalCount,
                    RecordsFiltered = filteredCount,
                    DataPropertyNames = Configuration.Columns.Select(c => c.Field)
                };
            }
            catch (Exception ex)
            {
                return new DataTablesResponse<T>()
                {
                    Draw = request.Draw,
                    Error = ErrorHandler != null ? ErrorHandler.Invoke(ex) : "Error while processing your request."
                };
            }
        }
        protected Query CreateBaseQuery(DataTablesRequest request, QueryFactory db)
        {

            return db.Query(Configuration.Table)
                     .Select(Configuration.Columns.Select(c => $"{c.Column} AS {c.Field}").ToArray());
        }
        protected Query CreateTotalCountQuery(DataTablesRequest request, Query baseQuery)
        {
            return baseQuery.Clone().AsCount();
        }
        protected Query ProcessGlobalSearch(DataTablesRequest request, Query baseQuery)
        {
            if (request.HasGlobalSearch)
            {
                var globalSearchColumns = Configuration.Columns.Where(cc => cc.GloballySearchable);
                if (globalSearchColumns.Any())
                {
                    baseQuery.Where(q =>
                    {
                        foreach (var c in globalSearchColumns)
                        {
                            string val = request.Search.Value;
                            //we'll use a exact match when the value is enclosed in double quotes -> "exactMatch"
                            bool exactMatch = val != null && val.Length > 1 && val.StartsWith("\"") && val.EndsWith("\"");
                            //we'll use a starts-with match when the value is NOT enclosed in double quotes and doesn't have any wildcards i.e.: '%', '_'
                            bool startsWith = !exactMatch && val.IndexOfAny(new char[] { '%', '_' }) < 0;

                            val = exactMatch
                                //strip quotes and perform a literal comparison
                                ? val = val.Substring(1, val.Length - 2)
                                //since we will only support % and _ placeholders for the global search we must scape any [ 
                                //so that the character grouping functionality of MS-SQL LIKE operator is disabled.
                                : Regex.Replace(val, @"(?<=(?<!\\)(\\\\)*)[\[]", @"\[");
                            //for every other case, perform a LIKE comparison. Only 
                            if (exactMatch)
                            {
                                q.OrWhere(c.Column, val);
                            }
                            else if (startsWith)
                            {
                                q.OrWhereStarts(c.Column, val);
                            }
                            else
                            {
                                q.OrWhereRaw($"[{c.Column}] LIKE ? ESCAPE '\\'", val);
                            }
                        }
                        return q;
                    });
                }
            }
            return baseQuery;
        }
        protected Query ProcessColumnSearch(DataTablesRequest request, Query baseQuery)
        {
            //Apply column search. Always check the column configuration 
            //to validate that the column is actually searchable
            var searchColumns = request.Columns.Where(sc => sc.HasSearch && (Configuration.Column(sc.Name)?.Searchable ?? false));
            if (searchColumns.Any())
            {
                foreach (var col in searchColumns)
                {
                    var colConfig = Configuration.Column(col.Name);
                    var convertToType = Nullable.GetUnderlyingType(colConfig.Type) ?? colConfig.Type;
                    var searchValue = Convert.ChangeType(col.Search.Value, convertToType);
                    //TODO use operators to generate the WHERE clause
                    if (colConfig.Type == typeof(string))
                    {
                        baseQuery.WhereStarts(colConfig.Column, searchValue.ToString());
                    }
                    else
                    {
                        baseQuery.Where(col.Name, searchValue);
                    }
                }
            }
            return baseQuery;
        }
        protected Query CreateFilteredCountQuery(DataTablesRequest request, Query baseQuery)
        {
            return baseQuery.Clone().AsCount();
        }
        protected Query ProcessSort(DataTablesRequest request, Query baseQuery)
        {
            //Apply the sort criteria respecting the sort order
            //Always check the server side configuration to validate that the column is actually sortable
            var sortColumns = request.Sort.OrderBy(s => s.Order)
                                     .Where(s => (Configuration.Column(s.Column)?.Sortable ?? false));
            foreach (var sort in sortColumns)
            {
                if (sort.Direction == SortDirection.Ascending)
                {
                    baseQuery.OrderBy(sort.Column);
                }
                else
                {
                    baseQuery.OrderByDesc(sort.Column);
                }
            }
            return baseQuery;
        }
        protected Query ProcessPagination(DataTablesRequest request, Query baseQuery)
        {
            //Apply pagination if needed
            if (request.HasPagination)
            {
                baseQuery.Limit(request.Length).Offset(request.Start);
            }
            return baseQuery;
        }
        public SqlDataTablesRequestHandler<T> WithSqlServer(bool useLegacyPagination = false)
        {
            QueryCompiler = new SqlServerCompiler() { UseLegacyPagination = useLegacyPagination };
            return this;
        }
        public SqlDataTablesRequestHandler<T> WithMySql()
        {
            QueryCompiler = new MySqlCompiler();
            return this;
        }
        public SqlDataTablesRequestHandler<T> WithPostgreSql()
        {
            QueryCompiler = new PostgresCompiler();
            return this;
        }
        public SqlDataTablesRequestHandler<T> OnError(Func<Exception, string> errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }
    }
}
