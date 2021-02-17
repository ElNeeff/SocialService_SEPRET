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
    public class NatureController : Controller
    {
        // GET: Nature
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(NatureVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Nature Nature = DBC.Natures.FirstOrDefault(x => x.Id == modelo.Id);

                    Nature.Nombre = modelo.Nombre;
                    Nature.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Nature Nature = new Nature
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Natures.Add(Nature);
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
                NatureVM modelo = new NatureVM();

                if (Id > 0)
                {
                    Nature Nature = DBC.Natures.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Nature.Id;
                    modelo.Nombre = Nature.Nombre;
                }

                return PartialView("~/Views/Nature/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Nature Nature = DBC.Natures.FirstOrDefault(x => x.Id == Id);

                if (Nature.Active)
                {
                    Nature.Active = false;
                }
                else
                {
                    Nature.Active = true;
                }

                Nature.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Nature> Nature = Enumerable.Empty<Nature>();

                switch (Filter)
                {
                    case "Active":
                        Nature = DBC.Natures.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        Nature = DBC.Natures.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = Nature.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Nature = Nature.Where(x => x.Nombre.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Nature.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Nature, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Nature = Nature.OrderBy(orderFunction);
                }
                else
                {
                    Nature = Nature.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Nature.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<NatureVM> data = Nature.Select(x => new NatureVM
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