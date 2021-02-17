using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador")]
    public class PhaseController : Controller
    {
        // GET: Phase
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(PhaseVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Phase phase = DBC.Phases.FirstOrDefault(x => x.Id == modelo.Id);
                    phase.Name = modelo.Name.ToTitleCase();

                    DBC.SaveChanges();
                }
                else
                {
                    Phase phase = new Phase
                    {
                        Name = modelo.Name.ToTitleCase(),
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Phases.Add(phase);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditPhase(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                PhaseVM modelo = new PhaseVM();
                if (Id > 0)
                {
                    Phase phase = DBC.Phases.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = phase.Id;
                    modelo.Name = phase.Name;
                }

                return PartialView("~/Views/Phase/_AddEditPhase.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdatePhaseStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Phase phase = DBC.Phases.FirstOrDefault(x => x.Id == Id);

                if (phase.Active)
                {
                    phase.Active = false;
                }
                else
                {
                    phase.Active = true;
                }

                phase.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PhaseDT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Phase> phases = Enumerable.Empty<Phase>();

                #region Filter
                switch (Filter)
                {
                    case "Active":
                        phases = DBC.Phases.Where(x => x.Active == true);
                        break;
                    case "Inactive":
                        phases = DBC.Phases.Where(x => x.Active == false);
                        break;
                    default:
                        break;
                }
                long total = phases.Count();
                #endregion

                #region Búsqueda
                string keyword = param.search.value;
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    phases = phases.Where(x => x.Name.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().Contains(keyword)
                    );
                }

                long totalFiltrado = phases.Count();
                #endregion

                #region Orden
                int colId = param.order[0].column;
                Func<Phase, string> orderFunction = (x => colId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    phases = phases.OrderBy(orderFunction);
                }
                else
                {
                    phases = phases.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginación
                phases = phases.Skip(param.start).Take(param.length);
                #endregion

                #region DT
                List<PhaseVM> dt = phases.Select(x => new PhaseVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Active = x.Active,
                    TimeCreatedFormatted = x.TimeCreated.ToString()
                }).ToList();
                #endregion

                return Json(new
                {
                    aaData = dt,
                    param.draw,
                    iTotalRecords= total,
                    iTotalDisplayRecords = totalFiltrado
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}