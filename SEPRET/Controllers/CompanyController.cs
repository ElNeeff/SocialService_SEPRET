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

        [HttpGet]
        public JsonResult GetCompanies()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                DBC.Configuration.LazyLoadingEnabled = false;
                List<Company> companies = DBC.Companies.Where(x => x.Active && x.Id_Dictum == 3).ToList();

                return Json(companies, JsonRequestBehavior.AllowGet);
            }
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
        public JsonResult Index(CompanyVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                string message = "La empresa se guardó correctamente";
                bool success = false;

                if (User.IsInRole("Gestión Tecnológica Y Vinculación"))
                {
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
                        Company.Telefono = new String(modelo.Telefono.Where(Char.IsDigit).ToArray());
                        Company.TimeUpdated = DateTime.Now;

                        DBC.SaveChanges();
                    }
                    else
                    {
                        Company Company = new Company
                        {
                            Id_Person = UserId,
                            Id_Dictum = 3,
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
                            Telefono = new String(modelo.Telefono.Where(Char.IsDigit).ToArray()),
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Companies.Add(Company);
                        DBC.SaveChanges();
                    }
                    success = true;
                }
                else
                {
                    Company companyExists = DBC.Companies.FirstOrDefault(x => x.RFC.ToLower() == modelo.RFC.ToLower().Trim() && x.Active);

                    if (companyExists == null)
                    {
                        Company Company = new Company
                        {
                            Id_Person = UserId,
                            Id_Dictum = 2,
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
                            Telefono = new String(modelo.Telefono.Where(Char.IsDigit).ToArray()),
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Companies.Add(Company);
                        DBC.SaveChanges();
                        success = true;
                    }
                    else
                    {
                        message = "La empresa que intentas registrar ya existe";
                    }
                }

                return Json(new { message, success }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id, bool ReadOnly)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                CompanyVM modelo = new CompanyVM();

                List<Sector> sectors = DBC.Sectors.Where(x => x.Active).ToList();
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
        public JsonResult ProjectDT(DataTablesParameters param)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<ProjectPerson> projectPeople = DBC.ProjectPersons.Where(x => x.Active && x.Project.Active && x.Owner).ToList();

                long total = projectPeople.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    projectPeople = projectPeople.Where(x => x.Project.Titulo.ToLower().Contains(keyword) ||
                    x.Project.Company.Nombre.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = projectPeople.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<ProjectPerson, string> orderFunction = (x => columnId == 0 ? x.Project.Titulo : columnId == 1 ? x.Project.Company.Nombre : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    projectPeople = projectPeople.OrderBy(orderFunction);
                }
                else
                {
                    projectPeople = projectPeople.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                projectPeople.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<ProjectPersonVM> data = projectPeople.Select(x => new ProjectPersonVM
                {
                    Id = x.Project.Id,
                    ProjectName = x.Project.Titulo,
                    Company = x.Project.Company.Nombre,
                    TimeCreatedFormatted = x.TimeCreated.ToString()
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

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Company> Company = Enumerable.Empty<Company>();

                long UserId = (long)Session["Id"];

                bool admin = false;
                if (User.IsInRole("Super Administrador") || User.IsInRole("Administrador") || User.IsInRole("Gestión Tecnológica Y Vinculación"))
                {
                    admin = true;
                }

                switch (Filter)
                {
                    case "Active":
                        Company = admin is true ? DBC.Companies.Where(x => x.Active).ToList() : DBC.Companies.Where(x => x.Active && x.Id_Person == UserId).ToList();
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

                    Company = Company.Where(x => x.Nombre.ToLower().Contains(keyword) ||
                    x.RFC.ToLower().Contains(keyword) || x.Telefono.ToLower().Contains(keyword) ||
                    string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName).ToLower().Contains(keyword) ||
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
                    IsAdmin = admin,
                    CreatedBy = new PersonVM
                    {
                        Id = x.Person.Id,
                        UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        Email = x.Person.Email
                    }
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