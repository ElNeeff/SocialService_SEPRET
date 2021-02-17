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
    public class CommentTypeController : Controller
    {
        // GET: CommentType
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(CommentTypeVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    CommentType CommentType = DBC.CommentTypes.FirstOrDefault(x => x.Id == modelo.Id);

                    CommentType.Nombre = modelo.Nombre;
                    CommentType.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    CommentType CommentType = new CommentType
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.CommentTypes.Add(CommentType);
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
                CommentTypeVM modelo = new CommentTypeVM();

                if (Id > 0)
                {
                    CommentType CommentType = DBC.CommentTypes.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = CommentType.Id;
                    modelo.Nombre = CommentType.Nombre;
                }

                return PartialView("~/Views/CommentType/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                CommentType CommentType = DBC.CommentTypes.FirstOrDefault(x => x.Id == Id);

                if (CommentType.Active)
                {
                    CommentType.Active = false;
                }
                else
                {
                    CommentType.Active = true;
                }

                CommentType.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<CommentType> CommentType = Enumerable.Empty<CommentType>();

                switch (Filter)
                {
                    case "Active":
                        CommentType = DBC.CommentTypes.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        CommentType = DBC.CommentTypes.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = CommentType.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    CommentType = CommentType.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = CommentType.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<CommentType, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    CommentType = CommentType.OrderBy(orderFunction);
                }
                else
                {
                    CommentType = CommentType.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                CommentType.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<CommentTypeVM> data = CommentType.Select(x => new CommentTypeVM
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