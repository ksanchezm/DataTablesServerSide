
using DataTablesServerSide.Configuration.Attributes;
using DataTablesServerSide.Request;
using DataTablesServerSide.Handlers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace excelwebtest2.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Test()
        {
            return View();
        }
        public ActionResult TestFilter()
        {
            Filter fg = new Filter()
            {
                Condition = "AND",
                Predicates = new List<FilterPredicate>()
                {
                    new FilterPredicate() { Column = "Id", Operator ="<", Arguments = new object[] { 50} },
                    new FilterPredicate() { Column = "Age", Operator ="between", Arguments = new object[] {18, 45} }
                },
                NestedFilters = new List<Filter>() {
                    new Filter()
                    {
                        Condition = "OR",
                        Predicates = new List<FilterPredicate>()
                        {
                            new FilterPredicate() { Column = "LastName", Operator= "in", Arguments= new object[] { "Garcia", "Flores", "Hernandez"} },
                            new FilterPredicate() { Column = "DOB", Operator= "dateEquals", Arguments = new object[] { DateTime.Now.AddYears(-10)} },
                            new FilterPredicate() { Column = "ParentName", Operator="isNull", Arguments = new object[0] }
                        }
                    }
                }
            };
            return Content(fg.ToString());
        }
        public class FilterPredicate
        {
            public string Column { get; set; }
            public string Operator { get; set; }
            public object[] Arguments { get; set; }
            public override string ToString()
            {
                return $"Col:{Column} Op:{Operator} Args:{string.Join(", ", Arguments.Select(a => a.ToString()))}";
            }
        }
        public class Filter
        {
            public Filter()
            {
                Predicates = new List<FilterPredicate>();
                NestedFilters = new List<Filter>();
            }
            public string Condition { get; set; }
            public List<FilterPredicate> Predicates { get; set; }
            public List<Filter> NestedFilters { get; set; }
            public override string ToString()
            {

                var prd = Predicates.Where(f => f != null);
                var strPrd = prd.Any() ? string.Join($"\r\n\t{Condition} ", prd) : "";
                var nf = NestedFilters.Where(cg => cg != null);
                var strNf = nf.Any() ? $"(\r\n{string.Join($"\r\n\t{Condition} ", nf.Select(f => f.ToString()))}\r\n)" : "";
                string joinString = !string.IsNullOrWhiteSpace(strPrd) && !string.IsNullOrWhiteSpace(strNf) ? $"\r\n\t{Condition} " : "";
                
                return $"{strPrd}{joinString}{Environment.NewLine}{strNf}";
            }
        }
        public ActionResult HandleDataRequest(DataTablesRequest request)
        {
            using (var connection = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=Consultorio;Integrated Security=true"))
            {
                Func<Exception, string> errorHandler = (ex) =>
                {
                    System.Diagnostics.Debug.WriteLine("Error while processing the DataTables request: \r\n" + ex.ToString());
                    return "Request cound't be completed.";
                };
                var dtHandler = new SqlDataTablesRequestHandler<MyTest>(connection).WithSqlServer()
                                                                                   .OnError(errorHandler);
                var dtResponse = dtHandler.Handle(request);
                return Content(dtResponse.ToJson(), "application/json");
            }
        }
    }

    /** DEMO POCO **/
    [DataTables(table: "vwTest")]
    [DataTables(configurationName: "MyCustomConfig", table: "vwTest_Custom")]
    public class MyTest
    {
        [DataTables(searchable: true)]
        public int? Id;
        [DataTables(column: "Name", sortable: true, searchable: true, globallySearchable: true)]
        public string FirstName { get; set; }
        [DataTables(sortable: true, searchable: true, globallySearchable: true)]
        public string LastName { get; set; }
        [IgnoreDataTablesMapping]
        public int IgnoreMe { get; set; }
        [DataTables(sortable: true, searchable: true)]
        public DateTime? DOB { get; set; }
        [DataTables(sortable: true, searchable: true)]
        public bool? Active { get; set; }
    }
}
    /** END DEMO POCO **/