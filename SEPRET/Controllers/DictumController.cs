using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Web;
using System.Web.Mvc;
using WebGrease.Activities;

namespace SEPRET.Controllers
{
    public class DictumController : Controller
    {
        // GET: Dictum
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(DictumVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Dictum dictum = DBC.Dicta.FirstOrDefault(x => x.Id == modelo.Id);

                    dictum.Nombre = modelo.Nombre;
                    dictum.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Dictum dictum = new Dictum
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Dicta.Add(dictum);
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
                DictumVM modelo = new DictumVM();

                if (Id > 0)
                {
                    Dictum dictum = DBC.Dicta.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = dictum.Id;
                    modelo.Nombre = dictum.Nombre;
                }

                return PartialView("~/Views/Dictum/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Dictum dictum = DBC.Dicta.FirstOrDefault(x => x.Id == Id);

                if (dictum.Active)
                {
                    dictum.Active = false;
                }
                else
                {
                    dictum.Active = true;
                }

                dictum.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Dictum> dictum = Enumerable.Empty<Dictum>();

                switch (Filter)
                {
                    case "Active":
                        dictum = DBC.Dicta.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        dictum = DBC.Dicta.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = dictum.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    dictum = dictum.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = dictum.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Dictum, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    dictum = dictum.OrderBy(orderFunction);
                }
                else
                {
                    dictum = dictum.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                dictum.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<DictumVM> data = dictum.Select(x => new DictumVM
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