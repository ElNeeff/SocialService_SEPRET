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
    public class AmbitController : Controller
    {
        // GET: Ambit
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(AmbitVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Ambit Ambit = DBC.Ambits.FirstOrDefault(x => x.Id == modelo.Id);

                    Ambit.Nombre = modelo.Nombre;
                    Ambit.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Ambit Ambit = new Ambit
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Ambits.Add(Ambit);
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
                AmbitVM modelo = new AmbitVM();

                if (Id > 0)
                {
                    Ambit Ambit = DBC.Ambits.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Ambit.Id;
                    modelo.Nombre = Ambit.Nombre;
                }

                return PartialView("~/Views/Ambit/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Ambit Ambit = DBC.Ambits.FirstOrDefault(x => x.Id == Id);

                if (Ambit.Active)
                {
                    Ambit.Active = false;
                }
                else
                {
                    Ambit.Active = true;
                }

                Ambit.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Ambit> Ambit = Enumerable.Empty<Ambit>();

                switch (Filter)
                {
                    case "Active":
                        Ambit = DBC.Ambits.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        Ambit = DBC.Ambits.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = Ambit.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Ambit = Ambit.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Ambit.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Ambit, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Ambit = Ambit.OrderBy(orderFunction);
                }
                else
                {
                    Ambit = Ambit.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Ambit.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<AmbitVM> data = Ambit.Select(x => new AmbitVM
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