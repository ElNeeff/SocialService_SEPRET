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
    public class ProjectTypeController : Controller
    {
        // GET: ProjectType
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ProjectTypeVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    ProjectType ProjectType = DBC.ProjectTypes.FirstOrDefault(x => x.Id == modelo.Id);

                    ProjectType.Nombre = modelo.Nombre;
                    ProjectType.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    ProjectType ProjectType = new ProjectType
                    {
                        Nombre = modelo.Nombre,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.ProjectTypes.Add(ProjectType);
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
                ProjectTypeVM modelo = new ProjectTypeVM();

                if (Id > 0)
                {
                    ProjectType ProjectType = DBC.ProjectTypes.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = ProjectType.Id;
                    modelo.Nombre = ProjectType.Nombre;
                }

                return PartialView("~/Views/ProjectType/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectType ProjectType = DBC.ProjectTypes.FirstOrDefault(x => x.Id == Id);

                if (ProjectType.Active)
                {
                    ProjectType.Active = false;
                }
                else
                {
                    ProjectType.Active = true;
                }

                ProjectType.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<ProjectType> ProjectType = Enumerable.Empty<ProjectType>();

                switch (Filter)
                {
                    case "Active":
                        ProjectType = DBC.ProjectTypes.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        ProjectType = DBC.ProjectTypes.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = ProjectType.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    ProjectType = ProjectType.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = ProjectType.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<ProjectType, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    ProjectType = ProjectType.OrderBy(orderFunction);
                }
                else
                {
                    ProjectType = ProjectType.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                ProjectType.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<ProjectTypeVM> data = ProjectType.Select(x => new ProjectTypeVM
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