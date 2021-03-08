using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class FileDictumController : Controller
    {
        // GET: FileDictum
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(FileDictumVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    FileDictum FileDictum = DBC.FileDictums.FirstOrDefault(x => x.Id == modelo.Id);

                    FileDictum.Nombre = modelo.Nombre;
                    FileDictum.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    FileDictum FileDictum = new FileDictum
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.FileDictums.Add(FileDictum);
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
                FileDictumVM modelo = new FileDictumVM();

                if (Id > 0)
                {
                    FileDictum FileDictum = DBC.FileDictums.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = FileDictum.Id;
                    modelo.Nombre = FileDictum.Nombre;
                }

                return PartialView("~/Views/FileDictum/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                FileDictum FileDictum = DBC.FileDictums.FirstOrDefault(x => x.Id == Id);

                if (FileDictum.Active)
                {
                    FileDictum.Active = false;
                }
                else
                {
                    FileDictum.Active = true;
                }

                FileDictum.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<FileDictum> FileDictum = Enumerable.Empty<FileDictum>();

                switch (Filter)
                {
                    case "Active":
                        FileDictum = DBC.FileDictums.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        FileDictum = DBC.FileDictums.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = FileDictum.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    FileDictum = FileDictum.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = FileDictum.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<FileDictum, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    FileDictum = FileDictum.OrderBy(orderFunction);
                }
                else
                {
                    FileDictum = FileDictum.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                FileDictum.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<FileDictumVM> data = FileDictum.Select(x => new FileDictumVM
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