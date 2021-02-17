using Microsoft.Ajax.Utilities;
using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador, Administrador, Financiero")]
    public class ReasonController : Controller
    {
        // GET: Reason
        public ActionResult Index()
        {
            return View();
        }

        public List<Reason> GetReasons()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Reason> reasons = DBC.Reasons.Where(x => x.Active == true && x.Preset == true).ToList();

                return reasons;
            }
        }

        [HttpPost]
        public ActionResult Index(ReasonVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Reason reason = DBC.Reasons.FirstOrDefault(x => x.Id == modelo.Id);
                    reason.Description = modelo.Description;

                    DBC.SaveChanges();
                }
                else
                {
                    Reason reason = new Reason
                    {
                        Description = modelo.Description,
                        Preset = true,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Reasons.Add(reason);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditReason(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ReasonVM modelo = new ReasonVM();

                if (Id > 0)
                {
                    Reason reason = DBC.Reasons.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = reason.Id;
                    modelo.Description = reason.Description;
                }

                return PartialView("~/Views/Reason/_AddEditReason.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateReasonStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Reason reason = DBC.Reasons.FirstOrDefault(x => x.Id == Id);

                if (reason.Active)
                {
                    reason.Active = false;
                }
                else
                {
                    reason.Active = true;
                }

                reason.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ReasonDT(DataTablesParameters parameter, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Reason> reasons = Enumerable.Empty<Reason>();

                #region Filtro
                switch (Filter)
                {
                    case "Active":
                        reasons = DBC.Reasons.Where(x => x.Active == true && x.Preset == true).ToList();
                        break;
                    case "Inactive":
                        reasons = DBC.Reasons.Where(x => x.Active == false && x.Preset == true).ToList();
                        break;
                    default:
                        break;
                }
                int total = reasons.Count();
                #endregion

                #region Búsqueda
                if (!string.IsNullOrEmpty(parameter.search.value))
                {
                    string keyword = parameter.search.value.ToLower();

                    reasons = reasons.Where(x => x.Description.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }

                int totalFiltrado = reasons.Count();
                #endregion

                #region Ordenamiento
                int colId = parameter.order[0].column;

                Func<Reason, string> orderFunction = (x => colId == 0 ? x.Description : x.TimeCreated.ToString());

                if (parameter.order[0].dir == "asc")
                {
                    reasons = reasons.OrderBy(orderFunction);
                }
                else
                {
                    reasons = reasons.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginación
                reasons = reasons.Skip(parameter.start).Take(parameter.length);
                #endregion

                #region DT
                List<ReasonVM> DT = reasons.Select(x => new ReasonVM
                {
                    Id = x.Id,
                    Description = x.Description,
                    TimeCreatedFormatted = x.TimeCreated.ToString(),
                    Active = x.Active
                }).ToList();
                #endregion

                return Json(new
                {
                    aaData = DT,
                    parameter.draw,
                    iTotalRecords = total,
                    iTotalDisplayRecords = totalFiltrado
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}