using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador, Administrador, Financiero")]
    public class MethodController : Controller
    {
        // GET: Method
        public ActionResult Index()
        {
            return View();
        }


        #region Métodos de pago
        [HttpPost]
        public ActionResult Index(MethodVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    // U P D A T E
                    Method method = DBC.Methods.FirstOrDefault(x => x.Id == modelo.Id);
                    method.Name = modelo.Name.ToTitleCase();
                    method.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    // I N S E R T
                    Method method = new Method
                    {
                        Name = modelo.Name.ToTitleCase(),
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Methods.Add(method);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditMethod(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                MethodVM modelo = new MethodVM();

                if (Id > 0)
                {
                    Method method = DBC.Methods.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = method.Id;
                    modelo.Name = method.Name;
                }

                return PartialView("~/Views/Method/_AddEditMethod.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateMethodStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Method method = DBC.Methods.FirstOrDefault(x => x.Id == Id);

                if (method.Active)
                {
                    method.Active = false;
                }
                else
                {
                    method.Active = true;
                }

                method.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MethodDT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                #region ListaDB
                IEnumerable<Method> methods = Enumerable.Empty<Method>();

                switch (Filter)
                {
                    case "Active":
                        methods = DBC.Methods.Where(x => x.Active == true);
                        break;
                    case "Inactive":
                        methods = DBC.Methods.Where(x => x.Active == false);
                        break;
                    default:
                        break;
                }
                long total = methods.Count();
                #endregion

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    methods = methods.Where(x => x.Name.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }

                long totalFiltrado = methods.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Method, string> orderFunction = (x => columnId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    methods = methods.OrderBy(orderFunction);
                }
                else
                {
                    methods = methods.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                methods = methods.Skip(param.start).Take(param.length);
                #endregion

                #region DT
                List<MethodVM> table = methods.Select(x => new MethodVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Active = x.Active,
                    TimeCreatedFormatted = x.TimeCreated.ToString()
                }).ToList();
                #endregion

                return Json(new
                {
                    aaData = table,
                    param.draw,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalFiltrado
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}