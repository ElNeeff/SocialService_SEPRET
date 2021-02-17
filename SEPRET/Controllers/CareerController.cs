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
    [Authorize(Roles = "Super Administrador, Administrador")]
    public class CareerController : Controller
    {
        // GET: Career
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(CareerVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Career career = DBC.Careers.FirstOrDefault(x => x.Id == modelo.Id);
                    career.Name = modelo.Name.ToTitleCase();
                    career.OfficialKey = modelo.OfficialKey;
                    career.StartDate = modelo.StartDate;
                    career.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Career career = new Career
                    {
                        Name = modelo.Name.ToTitleCase(),
                        OfficialKey = modelo.OfficialKey,
                        StartDate = modelo.StartDate,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Careers.Add(career);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditCareer(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                CareerVM modelo = new CareerVM();

                if (Id > 0)
                {
                    Career career = DBC.Careers.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = career.Id;
                    modelo.Name = career.Name;
                    modelo.OfficialKey= career.OfficialKey;
                    modelo.StartDate = career.StartDate;
                }

                return PartialView("~/Views/Career/_AddEditCareer.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateCareerStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Career career = DBC.Careers.FirstOrDefault(x => x.Id == Id);

                if (career.Active)
                {
                    career.Active = false;
                }
                else
                {
                    career.Active = true;
                }

                career.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        #region DataTable
        [HttpPost]
        public JsonResult CareerDT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Career> careers = Enumerable.Empty<Career>();

                switch (Filter)
                {
                    case "Active":
                        careers = DBC.Careers.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        careers = DBC.Careers.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                int total = careers.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    careers = careers.Where(x => x.Name.Contains(keyword) ||
                    x.OfficialKey.ToString().Contains(keyword) ||
                    x.StartDate.ToString().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }
                int totalFiltrado = careers.Count();
                #endregion

                #region Ordenamiento
                int colId = param.order[0].column;
                Func<Career, string> orderFunction = (x => colId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    careers = careers.OrderBy(orderFunction);
                }
                else
                {
                    careers = careers.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                careers.Skip(param.start).Skip(param.length);
                #endregion

                #region DataTable
                List<CareerVM> data = careers.Select(x => new CareerVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    StartDateFormatted = x.TimeCreated.ToString(),
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
        #endregion
    }
}