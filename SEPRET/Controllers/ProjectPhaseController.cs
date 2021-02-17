using Microsoft.Ajax.Utilities;
using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.Adapters;

namespace SEPRET.Controllers
{
    public class ProjectPhaseController : Controller
    {
        // GET: ProjectPhase
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ProjectPhaseVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    ProjectPhase projectPhase = DBC.ProjectPhases.FirstOrDefault(x => x.Id == modelo.Id);
                    projectPhase.Nombre = modelo.Nombre;
                    projectPhase.TimeUpdaetd = DateTime.Now;
                }
                else
                {
                    ProjectPhase projectPhase = new ProjectPhase
                    {
                        Nombre = modelo.Nombre,
                        TimeCreated = DateTime.Now,
                        Active = true
                    };

                    DBC.ProjectPhases.Add(projectPhase);
                }

                DBC.SaveChanges();
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectPhaseVM modelo = new ProjectPhaseVM();

                if (Id > 0)
                {
                    ProjectPhase projectPhase = DBC.ProjectPhases.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = Id;
                    modelo.Nombre = projectPhase.Nombre;
                }

                return PartialView("~/Views/ProjectPhase/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectPhase projectPhase = DBC.ProjectPhases.FirstOrDefault(x => x.Id == Id);

                if (projectPhase.Active)
                {
                    projectPhase.Active = false;
                }
                else
                {
                    projectPhase.Active = true;
                }

                projectPhase.TimeUpdaetd = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<ProjectPhase> projectPhases = Enumerable.Empty<ProjectPhase>();

                switch (Filter)
                {
                    case "Active":
                        projectPhases = DBC.ProjectPhases.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        projectPhases = DBC.ProjectPhases.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                int total = projectPhases.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    projectPhases = projectPhases.Where(x => x.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }
                int totalFiltrado = projectPhases.Count();
                #endregion

                #region Ordenamiento
                int colId = param.order[0].column;
                Func<ProjectPhase, string> orderFunction = (x => colId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    projectPhases = projectPhases.OrderBy(orderFunction);
                }
                else
                {
                    projectPhases = projectPhases.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                projectPhases.Skip(param.start).Skip(param.length);
                #endregion

                #region DataTable
                List<ProjectPhaseVM> data = projectPhases.Select(x => new ProjectPhaseVM
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
                    iTotalDisplayRecords = totalFiltrado
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}