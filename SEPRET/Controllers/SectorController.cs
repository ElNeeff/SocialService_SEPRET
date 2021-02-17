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
    public class SectorController : Controller
    {
        // GET: Sector
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(SectorVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Sector Sector = DBC.Sectors.FirstOrDefault(x => x.Id == modelo.Id);

                    Sector.Nombre = modelo.Nombre;
                    Sector.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Sector Sector = new Sector
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Sectors.Add(Sector);
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
                SectorVM modelo = new SectorVM();

                if (Id > 0)
                {
                    Sector Sector = DBC.Sectors.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Sector.Id;
                    modelo.Nombre = Sector.Nombre;
                }

                return PartialView("~/Views/Sector/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Sector Sector = DBC.Sectors.FirstOrDefault(x => x.Id == Id);

                if (Sector.Active)
                {
                    Sector.Active = false;
                }
                else
                {
                    Sector.Active = true;
                }

                Sector.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Sector> Sector = Enumerable.Empty<Sector>();

                switch (Filter)
                {
                    case "Active":
                        Sector = DBC.Sectors.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        Sector = DBC.Sectors.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = Sector.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Sector = Sector.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Sector.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Sector, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Sector = Sector.OrderBy(orderFunction);
                }
                else
                {
                    Sector = Sector.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Sector.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<SectorVM> data = Sector.Select(x => new SectorVM
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