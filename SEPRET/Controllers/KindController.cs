using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class KindController : Controller
    {
        // GET: Kind
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(KindVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Kind Kind = DBC.Kinds.FirstOrDefault(x => x.Id == modelo.Id);

                    Kind.Nombre = modelo.Nombre;
                    Kind.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Kind Kind = new Kind
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Kinds.Add(Kind);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                KindVM modelo = new KindVM();

                if (Id > 0)
                {
                    Kind Kind = DBC.Kinds.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Kind.Id;
                    modelo.Nombre = Kind.Nombre;
                }

                return PartialView("~/Views/Kind/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Kind Kind = DBC.Kinds.FirstOrDefault(x => x.Id == Id);

                if (Kind.Active)
                {
                    Kind.Active = false;
                }
                else
                {
                    Kind.Active = true;
                }

                Kind.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Kind> Kind = Enumerable.Empty<Kind>();

                switch (Filter)
                {
                    case "Active":
                        Kind = DBC.Kinds.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        Kind = DBC.Kinds.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = Kind.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Kind = Kind.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Kind.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Kind, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Kind = Kind.OrderBy(orderFunction);
                }
                else
                {
                    Kind = Kind.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Kind.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<KindVM> data = Kind.Select(x => new KindVM
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    TimeCreatedFormatted = x.TimeCreated.ToString(),
                    Active = x.Active
                }).ToList();
                #endregion

                return Json(new
                {
                    aaData = data,
                    param.draw,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalFiltered
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}