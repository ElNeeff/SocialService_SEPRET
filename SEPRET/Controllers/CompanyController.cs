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
    public class CompanyController : Controller
    {
        // GET: Company
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Action(string Action, long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Company company = DBC.Companies.FirstOrDefault(x => x.Id == Id);

                switch (Action)
                {
                    case "Accept":
                        company.Id_Dictum = 3;
                        break;
                    case "Rejected":
                        company.Id_Dictum = 1;
                        break;
                    default:
                        break;
                }
                company.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Index(CompanyVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                if (modelo.Id > 0)
                {
                    Company Company = DBC.Companies.FirstOrDefault(x => x.Id == modelo.Id);

                    Company.Id_Sector = modelo.Id_Sector;
                    Company.Nombre = modelo.Nombre;
                    Company.RFC = modelo.RFC;
                    Company.Lema = modelo.Lema;
                    Company.Mision = modelo.Mision;
                    Company.Valores = modelo.Valores;
                    Company.Calle = modelo.Calle;
                    Company.Colonia = modelo.Colonia;
                    Company.CP = modelo.CP;
                    Company.Ciudad = modelo.Ciudad;
                    Company.Estado = modelo.Estado;
                    Company.Telefono = modelo.Telefono;
                    Company.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Company Company = new Company
                    {
                        Id_Person = UserId,
                        Id_Dictum = User.IsInRole("Gestión Tecnológica y Vinculación") ? 3 : 2,
                        Id_Sector = modelo.Id_Sector,
                        Nombre = modelo.Nombre,
                        RFC = modelo.RFC,
                        Lema = modelo.Lema,
                        Mision = modelo.Mision,
                        Valores = modelo.Valores,
                        Calle = modelo.Calle,
                        Colonia = modelo.Colonia,
                        CP = modelo.CP,
                        Ciudad = modelo.Ciudad,
                        Estado = modelo.Estado,
                        Telefono = modelo.Telefono,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Companies.Add(Company);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id, bool ReadOnly)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                CompanyVM modelo = new CompanyVM();

                List<Sector> sectors = DBC.Sectors.Where(x => x.Active == true).ToList();
                ViewBag.SectorList = new SelectList(sectors, "Id", "Nombre");

                if (Id > 0)
                {
                    Company Company = DBC.Companies.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Company.Id;
                    modelo.Id_Sector = Company.Id_Sector;
                    modelo.Nombre = Company.Nombre;
                    modelo.RFC = Company.RFC;
                    modelo.Lema = Company.Lema;
                    modelo.Mision = Company.Mision;
                    modelo.Valores = Company.Valores;
                    modelo.Calle = Company.Calle;
                    modelo.Colonia = Company.Colonia;
                    modelo.CP = Company.CP;
                    modelo.Ciudad = Company.Ciudad;
                    modelo.Estado = Company.Estado;
                    modelo.Telefono = Company.Telefono;

                }

                if (ReadOnly)
                {

                    return PartialView("~/Views/Company/_Detail.cshtml", modelo);
                }
                else
                {
                    return PartialView("~/Views/Company/_AddEdit.cshtml", modelo);

                }
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Company Company = DBC.Companies.FirstOrDefault(x => x.Id == Id);

                if (Company.Active)
                {
                    Company.Active = false;
                }
                else
                {
                    Company.Active = true;
                }

                Company.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Company> Company = Enumerable.Empty<Company>();

                long UserId = (long)Session["Id"];

                bool admin = false;
                if (User.IsInRole("Super Administrador") || User.IsInRole("Administrador") || User.IsInRole("Gestión Tecnológica y Vinculación"))
                {
                    admin = true;
                }

                switch (Filter)
                {
                    case "Active":
                        Company = admin is true ? DBC.Companies.Where(x => x.Active == true).ToList() : DBC.Companies.Where(x => x.Active == true && x.Id_Person == UserId).ToList();
                        break;
                    case "Inactive":
                        Company = admin is true ? DBC.Companies.Where(x => x.Active == false).ToList() : DBC.Companies.Where(x => x.Active == false && x.Id_Person == UserId).ToList();
                        break;
                    default:
                        break;
                }

                long total = Company.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Company = Company.Where(x => x.Nombre.Contains(keyword) ||
                    x.RFC.Contains(keyword) || x.Telefono.Contains(keyword) ||
                    x.Dictum.Nombre.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Company.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Company, string> orderFunction = (x => columnId == 0 ? x.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Company = Company.OrderBy(orderFunction);
                }
                else
                {
                    Company = Company.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Company.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<CompanyVM> data = Company.Select(x => new CompanyVM
                {
                    Id = x.Id,
                    Id_Dictum = x.Dictum.Id,
                    Nombre = x.Nombre,
                    RFC = x.RFC,
                    Telefono = x.Telefono,
                    TimeCreatedFormatted = x.TimeCreated.ToString(),
                    Dictamen = x.Dictum.Nombre,
                    Active = x.Active,
                    IsAdmin = admin
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