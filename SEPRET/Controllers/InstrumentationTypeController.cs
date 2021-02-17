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
    public class InstrumentationTypeController : Controller
    {
        // GET: InstrumentationType
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(InstrumentationTypeVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    InstrumentationType InstrumentationType = DBC.InstrumentationTypes.FirstOrDefault(x => x.Id == modelo.Id);

                    InstrumentationType.Nombre = modelo.Nombre;
                    InstrumentationType.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    InstrumentationType InstrumentationType = new InstrumentationType
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.InstrumentationTypes.Add(InstrumentationType);
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
                InstrumentationTypeVM modelo = new InstrumentationTypeVM();

                if (Id > 0)
                {
                    InstrumentationType InstrumentationType = DBC.InstrumentationTypes.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = InstrumentationType.Id;
                    modelo.Nombre = InstrumentationType.Nombre;
                }

                return PartialView("~/Views/InstrumentationType/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                InstrumentationType InstrumentationType = DBC.InstrumentationTypes.FirstOrDefault(x => x.Id == Id);

                if (InstrumentationType.Active)
                {
                    InstrumentationType.Active = false;
                }
                else
                {
                    InstrumentationType.Active = true;
                }

                InstrumentationType.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<InstrumentationType> InstrumentationType = Enumerable.Empty<InstrumentationType>();

                switch (Filter)
                {
                    case "Active":
                        InstrumentationType = DBC.InstrumentationTypes.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        InstrumentationType = DBC.InstrumentationTypes.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = InstrumentationType.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    InstrumentationType = InstrumentationType.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = InstrumentationType.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<InstrumentationType, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    InstrumentationType = InstrumentationType.OrderBy(orderFunction);
                }
                else
                {
                    InstrumentationType = InstrumentationType.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                InstrumentationType.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<InstrumentationTypeVM> data = InstrumentationType.Select(x => new InstrumentationTypeVM
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