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
    public class AdviserTypeController : Controller
    {
        // GET: AdviserType
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(AdviserTypeVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    AdviserType AdviserType = DBC.AdviserTypes.FirstOrDefault(x => x.Id == modelo.Id);

                    AdviserType.Nombre = modelo.Nombre;
                    AdviserType.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    AdviserType AdviserType = new AdviserType
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.AdviserTypes.Add(AdviserType);
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
                AdviserTypeVM modelo = new AdviserTypeVM();

                if (Id > 0)
                {
                    AdviserType AdviserType = DBC.AdviserTypes.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = AdviserType.Id;
                    modelo.Nombre = AdviserType.Nombre;
                }

                return PartialView("~/Views/AdviserType/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                AdviserType AdviserType = DBC.AdviserTypes.FirstOrDefault(x => x.Id == Id);

                if (AdviserType.Active)
                {
                    AdviserType.Active = false;
                }
                else
                {
                    AdviserType.Active = true;
                }

                AdviserType.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<AdviserType> AdviserType = Enumerable.Empty<AdviserType>();

                switch (Filter)
                {
                    case "Active":
                        AdviserType = DBC.AdviserTypes.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        AdviserType = DBC.AdviserTypes.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = AdviserType.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    AdviserType = AdviserType.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = AdviserType.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<AdviserType, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    AdviserType = AdviserType.OrderBy(orderFunction);
                }
                else
                {
                    AdviserType = AdviserType.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                AdviserType.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<AdviserTypeVM> data = AdviserType.Select(x => new AdviserTypeVM
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