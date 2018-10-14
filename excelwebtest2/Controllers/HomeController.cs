using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace excelwebtest2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public FileResult ExcelTest2()
        {
            List<User> data = new List<Controllers.User>();
            data.Add(new User() { USUA_APELLIDO = "abc", USUA_NOMBRE = "abc123", USUA_PASSWORD = "pa1243", USUA_ID = 1, USUA_USUARIO = "usua1345" });
            data.Add(new User() { USUA_APELLIDO = "apellido", USUA_NOMBRE = "nombre", USUA_PASSWORD = "pass", USUA_ID = 0, USUA_USUARIO = "usuario" });

            return ExcelDownload(data, "users");
        }

        public FileResult ExcelTest()
        {
            //Get the data from SQL
            DataSet ds = new DataSet("Users Report");
            ds.Tables.Add("Users");
            using (var conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=Consultorio;Integrated Security=true"))
            {
                conn.Open();
                using (var com = new SqlCommand("sp_Test", conn))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@p2", 2);
                    com.Parameters.AddWithValue("@p1", 1);
                    using (var reader = com.ExecuteReader())
                    {
                        ds.Load(reader, LoadOption.OverwriteChanges, "Users");
                    }
                }
            }
            //Write to excel file
            return ExcelDownload(ds);
        }

        public ActionResult DatatablesTest()
        {
            List<User> users = new List<User>();
            return View(users);
        }

        public ActionResult GetData()
        {
            var users = new List<User>();
            using (var conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=Consultorio;Integrated Security=true"))
            {
                conn.Open();
                using (var com = new SqlCommand("sp_TestRows", conn))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    using (var reader = com.ExecuteReader())
                    {
                        users = Controllers.User.GetFromReader(reader);
                    }
                }
            }
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckList()
        {
            var model = CheckListModel.Create();
            return View(model);
        }

        public JsonResult GetPagedData()
        {
            return Json(new { Test = 0}, JsonRequestBehavior.AllowGet);
        }

        private FileStreamResult ExcelDownload(DataSet data)
        {
            var ms = new MemoryStream();
            using (var ep = new ExcelPackage(ms))
            {
                for (int i = 0; i < data.Tables.Count; i++)
                {
                    var t = data.Tables[i];
                    var ws = ep.Workbook.Worksheets.Add(string.IsNullOrWhiteSpace(t.TableName) ? $"Sheet{i}" : t.TableName);
                    ws.Cells["A1"].LoadFromDataTable(t, true, OfficeOpenXml.Table.TableStyles.Light4);
                    ws.Cells.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    ws.Cells.AutoFitColumns();
                }
                ep.Save();
            }
            ms.Seek(0, SeekOrigin.Begin);
            HttpContext.Response.Headers.Add("content-disposition", $"attachment;filename={data.DataSetName}.xlsx");
            FileStreamResult fs = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return fs;
        }

        private FileStreamResult ExcelDownload<T>(List<T> data, string downloadFilename)
        {
            var headers = data.GetType().GetGenericArguments().First().GetProperties().Select(pi => pi.Name);
            var ms = new MemoryStream();
            using (var ep = new ExcelPackage(ms))
            {
                var ws = ep.Workbook.Worksheets.Add("Report");
                ws.Cells[$"A1"].LoadFromCollection(headers);
                ws.Cells["B1"].LoadFromCollection(data);
                ep.Save();
            }
            ms.Seek(0, SeekOrigin.Begin);
            HttpContext.Response.Headers.Add("content-disposition", $"attachment;filename={downloadFilename}.xlsx");
            FileStreamResult fs = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return fs;
        }

        public ActionResult Test()
        {
            return View();
        }
    }

    public class User
    {
        public int? USUA_ID { get; set; }
        public string USUA_NOMBRE { get; set; }
        public string USUA_APELLIDO { get; set; }
        public string USUA_USUARIO { get; set; }
        public string USUA_PASSWORD { get; set; }

        public string USUA_TEST { get; set; }
        public int USUA_TEST_2 { get; set; }

        public static List<User> GetFromReader(SqlDataReader reader)
        {
            List<User> users = new List<User>();
            while (reader.Read())
            {
                users.Add(new User()
                {
                    USUA_ID = reader.GetInt32(0),
                    USUA_NOMBRE = reader.GetString(1),
                    USUA_APELLIDO = reader.GetString(2),
                    USUA_USUARIO = reader.GetString(3),
                    USUA_PASSWORD = reader.GetString(4),
                    USUA_TEST = reader.GetString(5),
                    USUA_TEST_2 = reader.GetInt32(6)
                });
            }

            return users;
        }
    }

    public class FooModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }

        public List<FooModel> SubItems { get; set; }

        public static List<FooModel> GetFooModels()
        {
            return Enumerable.Range(1, 4).Select(x => new FooModel()
            {
                Id = x,
                Description = $"FooModel{x}",
                SubItems = Enumerable.Range(1, 3).Select(y => new FooModel() { Id = (10 * x) + y, Description = $"FooSubItem{(10 * x) + y}" }).ToList()
            }).ToList();
        }
    }

    public class CheckListModel
    {
        public List<FooModel> Foos { get; set; }

        public static CheckListModel Create()
        {
            return new CheckListModel()
            {
                Foos = FooModel.GetFooModels()
            };
        }
    }
}