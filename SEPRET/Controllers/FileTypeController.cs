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
    public class FileTypeController : Controller
    {
        // GET: FileType
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FileTypeVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    FileType FileType = DBC.FileTypes.FirstOrDefault(x => x.Id == modelo.Id);

                    FileType.Nombre = modelo.Nombre;
                    FileType.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    FileType FileType = new FileType
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.FileTypes.Add(FileType);
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
                FileTypeVM modelo = new FileTypeVM();

                if (Id > 0)
                {
                    FileType FileType = DBC.FileTypes.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = FileType.Id;
                    modelo.Nombre = FileType.Nombre;
                }

                return PartialView("~/Views/FileType/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                FileType FileType = DBC.FileTypes.FirstOrDefault(x => x.Id == Id);

                if (FileType.Active)
                {
                    FileType.Active = false;
                }
                else
                {
                    FileType.Active = true;
                }

                FileType.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<FileType> FileType = Enumerable.Empty<FileType>();

                switch (Filter)
                {
                    case "Active":
                        FileType = DBC.FileTypes.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        FileType = DBC.FileTypes.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = FileType.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    FileType = FileType.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = FileType.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<FileType, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    FileType = FileType.OrderBy(orderFunction);
                }
                else
                {
                    FileType = FileType.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                FileType.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<FileTypeVM> data = FileType.Select(x => new FileTypeVM
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