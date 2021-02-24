using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using SEPRET.CustomClasses;

namespace SEPRET.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult Index()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                IEnumerable<ProjectPerson> projects = User.IsInRole("Alumno") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId && x.Project.Id_ProjectType == 1 || x.Project.Active == true && x.Project.Id_ProjectType == 2 && x.Id_Dictum == 3 && x.Id_Person == UserId).ToList() : User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase < 5 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase < 5 && x.Project.Id_ProjectType == 2 && x.Project.Active).ToList();
                long total = projects.Count();
                IEnumerable<ProjectVM> projectVMs = Enumerable.Empty<ProjectVM>();

                if (total > 0)
                {
                    List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                    {
                        Id = x.Project.Id,
                        TipoDeProyecto = x.Project.ProjectType.Nombre,
                        Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                        Caracter = x.Project.Nature.Nombre,
                        Ambito = x.Project.Ambit.Nombre,
                        Tipo = x.Project.ProjectType.Nombre,
                        Id_ProjectPhase = x.Project.Id_ProjectPhase,
                        Etapa = x.Project.ProjectPhase.Nombre,
                        Titulo = x.Project.Titulo,
                        ObjetivoGeneral = x.Project.ObjetivoGeneral,
                        ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                        Justificacion = x.Project.Justificacion,
                        Actividades = x.Project.Actividades,
                        Comentarios = x.Project.Comentarios,
                        CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                        Active = x.Project.Active,
                        TimeCreated = x.Project.TimeCreated,
                        Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Career.Name).ToList()),
                        Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_Dictum == 3).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email,
                            ProjectOwner = y.Owner
                        }).ToList(),
                        Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Mensaje).LastOrDefault(),
                        PDFExists = x.Project.ProjectFiles.Any(y => y.Id_Project == x.Project.Id && y.Proyecto && y.Active)
                    }).ToList();

                    ViewBag.ProjectList = projectList;
                    projectVMs = projectList;
                }
                else
                {
                    ViewBag.ProjectList = null;
                }

                ViewBag.Total = total;

                return User.IsInRole("Alumno") ? View("Student") : View(projectVMs);
            }
        }

        [HttpPost]
        public JsonResult ViewPDF(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectFile projectFile = DBC.ProjectFiles.OrderByDescending(x => x.Id).FirstOrDefault(x => x.Id_Project == Id && x.Proyecto && x.Active);
                var file = Server.MapPath(projectFile.Ruta);

                byte[] bytes = System.IO.File.ReadAllBytes(file);
                string base64 = string.Concat("data:application/pdf;base64,", Convert.ToBase64String(bytes));

                bool success = string.IsNullOrEmpty(base64) ? false : true;

                var response = new
                {
                    success,
                    base64
                };

                return new JsonResult()
                {
                    Data = response,
                    MaxJsonLength = 86753090,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpPost]
        public JsonResult AddReview(long Id, string Review)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id);

                Comment comment = new Comment
                {
                    Id_Project = Id,
                    Id_CommentType = 4,
                    Id_Person = UserId,
                    Mensaje = Review,
                    Active = true,
                    TimeCreated = DateTime.Now
                };

                DBC.Comments.Add(comment);
                project.Id_ProjectPhase = project.Id_ProjectPhase is 10 ? 9 : project.Id_ProjectPhase;
                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Teacher()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];

                IEnumerable<Adviser> projects = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active && (x.Project.Id_ProjectPhase == 9 || x.Project.Id_ProjectPhase == 10)).Take(12).ToList();

                List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                {
                    Id = x.Project.Id,
                    Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                    Caracter = x.Project.Nature.Nombre,
                    Ambito = x.Project.Ambit.Nombre,
                    Tipo = x.Project.ProjectType.Nombre,
                    Id_ProjectPhase = x.Project.Id_ProjectPhase,
                    Etapa = x.Project.ProjectPhase.Id is 5 && x.Project.Id_ProjectType is 2 && User.IsInRole("Alumno") ? "En espera de envio de anteproyecto" : x.Project.ProjectPhase.Nombre,
                    Titulo = x.Project.Titulo,
                    ObjetivoGeneral = x.Project.ObjetivoGeneral,
                    ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                    Justificacion = x.Project.Justificacion,
                    Actividades = x.Project.Actividades,
                    Comentarios = x.Project.Comentarios,
                    TimeCreated = x.Project.TimeCreated,
                    //Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                    //EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                    Active = x.Active,
                    Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Career.Name).ToList()),
                    //Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Owner != true && y.Id_Dictum == 3).Select(y => new PersonVM
                    //{
                    //    UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                    //    Enrollment = y.Person.Enrollment,
                    //    Email = y.Person.Email
                    //}).ToList(),
                    Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_Dictum == 3).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email,
                        ProjectOwner = y.Owner
                    }).ToList(),
                    //Miembros = string.Join(", ", x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Owner != true && y.Id_Dictum == 3).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName, " (", y.Person.Enrollment, ")")).ToList()),
                    LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                }).ToList();

                return View(projectList);
            }
        }

        public ActionResult Administrative()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];

                IEnumerable<ProjectCareer> projects = Enumerable.Empty<ProjectCareer>();
                string Role = System.Web.Security.Roles.GetRolesForUser().Single();

                switch (Role)
                {
                    case "Coordinador de carrera":
                        projects = DBC.ProjectCareers.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 7 && x.Id_Career == CareerId).Take(12).ToList();
                        break;
                    case "Jefe departamental":
                        projects = DBC.ProjectCareers.Where(x => x.Project.Active && x.Id_Project == x.Project.Id && x.Id_Career == CareerId && x.Project.Id_ProjectPhase == 8 && x.Project.ProjectPersons.Where(r => r.Owner).Select(t => t.Owner).FirstOrDefault()).Take(12).ToList();
                        break;
                    default:
                        break;
                }

                long total = projects.Count();

                List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                {
                    Id = x.Project.Id,
                    Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                    Caracter = x.Project.Nature.Nombre,
                    Ambito = x.Project.Ambit.Nombre,
                    Tipo = x.Project.ProjectType.Nombre,
                    Id_ProjectPhase = x.Project.Id_ProjectPhase,
                    Etapa = x.Project.ProjectPhase.Id is 5 && x.Project.Id_ProjectType is 2 && User.IsInRole("Alumno") ? "En espera de envio de anteproyecto" : x.Project.ProjectPhase.Nombre,
                    Titulo = x.Project.Titulo,
                    ObjetivoGeneral = x.Project.ObjetivoGeneral,
                    ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                    Justificacion = x.Project.Justificacion,
                    Actividades = x.Project.Actividades,
                    Comentarios = x.Project.Comentarios,
                    TimeCreated = x.Project.TimeCreated,
                    //Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                    //EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                    Active = x.Active,
                    Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Career.Name).ToList()),
                    //Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Owner != true && y.Id_Dictum == 3).Select(y => new PersonVM
                    //{
                    //    UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                    //    Enrollment = y.Person.Enrollment,
                    //    Email = y.Person.Email
                    //}).ToList(),
                    Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_Dictum == 3).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email,
                        ProjectOwner = y.Owner
                    }).ToList(),
                    //Miembros = string.Join(", ", x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Owner != true && y.Id_Dictum == 3).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName, " (", y.Person.Enrollment, ")")).ToList()),
                    LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                }).ToList();

                ViewBag.Total = total;

                return User.IsInRole("Coordinador de carrera") ? View("Coordinador", projectList) : View("JefeDepartamental", projectList);
            }
        }

        [HttpPost]
        public JsonResult Apply(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                bool applyExists = DBC.ProjectPersons.Any(x => x.Id_Dictum == 3 && x.Id_Person == UserId && x.Active && x.Owner);
                bool success = false;

                if (!applyExists)
                {
                    ProjectPerson projectPerson = new ProjectPerson
                    {
                        Id_Project = Id,
                        Id_Person = UserId,
                        Id_Dictum = 2,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };
                    DBC.ProjectPersons.Add(projectPerson);
                    DBC.SaveChanges();
                    success = true;
                }

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateAdviser(long Id_Person, long Id_Project)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id_Project);

                Adviser adviser = project.Id_ProjectPhase >= 11 ? DBC.Advisers.FirstOrDefault(x => x.Id_Project == Id_Project && x.Id_Person == Id_Person && x.Id_AdviserType == 2) : DBC.Advisers.FirstOrDefault(x => x.Id_Project == Id_Project && x.Id_Person == Id_Person && x.Id_AdviserType == 1);

                if (adviser != null)
                {
                    adviser.Active = adviser.Active is false;
                }
                else
                {
                    adviser = new Adviser
                    {
                        Id_Project = Id_Project,
                        Id_Person = Id_Person,
                        Id_AdviserType = project.Id_ProjectPhase >= 11 ? 2 : 1,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Advisers.Add(adviser);
                }

                project.Id_ProjectPhase = project.Id_ProjectPhase is 8 || project.Id_ProjectPhase is 10 ? 10 : 12;
                project.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        #region DataTable Docentes
        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter, long Id_Project)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Person> people = Enumerable.Empty<Person>();
                long DepartmentId = (long)Session["DepartmentId"];
                long UserId = (long)Session["Id"];

                switch (Filter)
                {
                    case "MyDepartment":
                        people = DBC.People.Where(x => x.Career.Department.Id == DepartmentId && x.Active && x.PersonPermissions.Where(y => y.Permission.Name.ToLower() == "docente").Select(y => y.Permission.Name).FirstOrDefault() == "docente").ToList();
                        break;
                    case "Others":
                        people = DBC.People.Where(x => x.Active && x.PersonPermissions.Where(y => y.Permission.Name.ToLower() == "docente").Select(y => y.Permission.Name).FirstOrDefault() == "docente").ToList();
                        break;
                    default:
                        break;
                }

                int total = people.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    people = people.Where(x => x.Name.ToLower().Contains(keyword) ||
                    x.MiddleName.ToLower().Contains(keyword) ||
                    x.LastName.ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }
                int totalFiltrado = people.Count();
                #endregion

                #region Ordenamiento
                int colId = param.order[0].column;
                Func<Person, string> orderFunction = (x => colId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    people = people.OrderBy(orderFunction);
                }
                else
                {
                    people = people.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                people.Skip(param.start).Skip(param.length);
                #endregion

                #region DataTable
                Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id_Project);
                List<PersonVM> data = project is null ? null : project.Id_ProjectPhase >= 11 ?
                people.Select(x => new PersonVM
                {
                    Id = x.Id,
                    UserFullName = string.Concat(x.Name, " ", x.MiddleName, " ", x.LastName),
                    Active = x.Advisers.Where(y => y.Project.Id_ProjectPhase >= 11 && y.Project.Active && y.Id_Person == y.Person.Id && y.Id_Project == Id_Project && y.Id_AdviserType == 2).Select(z => z.Active).FirstOrDefault(),
                    Projects = x.Advisers.Where(y => y.Project.Id_ProjectPhase >= 11 && y.Project.Active && y.Active && y.Id_Person == x.Id && y.Id_AdviserType == 2).Select(z => new ProjectVM
                    {
                        Id = z.Project.Id,
                        Titulo = z.Project.Titulo
                    }).ToList(),
                    TimeCreatedFormatted = x.TimeCreated.ToString()
                }).ToList() :
                people.Select(x => new PersonVM
                {
                    Id = x.Id,
                    UserFullName = string.Concat(x.Name, " ", x.MiddleName, " ", x.LastName),
                    Active = x.Advisers.Where(y => y.Project.Id_ProjectPhase == 9 || y.Project.Id_ProjectPhase == 10 && y.Project.Active && y.Id_Person == y.Person.Id && y.Id_Project == Id_Project).Select(z => z.Active).FirstOrDefault(),
                    Projects = x.Advisers.Where(y => y.Project.Id_ProjectPhase == 9 || y.Project.Id_ProjectPhase == 10 && y.Project.Active && y.Active && y.Id_Person == x.Id).Select(z => new ProjectVM
                    {
                        Id = z.Project.Id,
                        Titulo = z.Project.Titulo
                    }).ToList(),
                    TimeCreatedFormatted = x.TimeCreated.ToString()
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

        [HttpPost]
        public ActionResult ListMember(long Id, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<ProjectPerson> projectPeople = Enumerable.Empty<ProjectPerson>();

                switch (Filter)
                {
                    case "Pending":
                        projectPeople = DBC.ProjectPersons.Where(x => x.Id_Dictum == 2 && x.Id_Project == Id && !x.Owner).ToList();
                        break;
                    case "Accepted":
                        projectPeople = DBC.ProjectPersons.Where(x => x.Id_Dictum == 3 && x.Id_Project == Id && !x.Owner).ToList();
                        break;
                    case "Rejected":
                        projectPeople = DBC.ProjectPersons.Where(x => x.Id_Dictum == 1 && x.Id_Project == Id && !x.Owner).ToList();
                        break;
                    default:
                        break;
                }

                List<ProjectPersonVM> list = projectPeople.OrderByDescending(x => x.TimeCreated).Select(x => new ProjectPersonVM
                {
                    Id = x.Id,
                    Id_Dictum = x.Id_Dictum,
                    Id_Person = x.Id_Person,
                    Id_Project = x.Id_Project,
                    PersonName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                    PersonCareer = x.Person.Career.Name,
                    PersonEmail = x.Person.Email,
                    ProjectName = x.Project.ProjectPersons.FirstOrDefault(y => y.Project.Id == x.Id_Project).Project.Titulo.Replace(" ", "%20"),
                    Owner = x.Owner,
                    Active = x.Active,
                    TimeCreated = x.TimeCreated
                }).ToList();

                return PartialView("~/Views/Project/_ListMember.cshtml", list);
            }
        }

        [HttpPost]
        public JsonResult MemberDictum(long Id, string Dictum)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectPerson projectPerson = DBC.ProjectPersons.FirstOrDefault(x => x.Id == Id);

                projectPerson.Id_Dictum = Dictum.Contains("Accept") ? 3 : Dictum.Contains("Reject") ? 1 : projectPerson.Id_Dictum;
                projectPerson.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddEditAdviser(long Id)
        {
            return PartialView("~/Views/Project/_AddEditRevisor.cshtml", Id);
        }

        [HttpPost]
        public ActionResult Members(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectPerson projectPerson = DBC.ProjectPersons.FirstOrDefault(x => x.Id_Project == Id && x.Owner);

                ProjectPersonVM owner = new ProjectPersonVM
                {
                    Id = projectPerson.Id,
                    Id_Person = projectPerson.Id_Person,
                    Id_Dictum = projectPerson.Id_Dictum,
                    Id_Project = projectPerson.Id_Project,
                    PersonName = string.Concat(projectPerson.Person.Name, " ", projectPerson.Person.MiddleName, " ", projectPerson.Person.LastName),
                    PersonCareer = projectPerson.Person.Career.Name,
                    TimeCreated = projectPerson.TimeCreated,
                    Active = projectPerson.Active
                };

                return PartialView("~/Views/Project/_ManageMembers.cshtml", owner);
            }
        }

        public ActionResult Bank()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                //long CareerId = (long)Session["CareerId"];

                IEnumerable<ProjectPerson> projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase >= 5 && x.Project.Id_ProjectType == 2 && x.Id_Person == UserId).ToList();
                long total = projects.Count();

                if (total > 0)
                {
                    List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                    {
                        Id = x.Project.Id,
                        TipoDeProyecto = x.Project.ProjectType.Nombre,
                        Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                        Caracter = x.Project.Nature.Nombre,
                        Ambito = x.Project.Ambit.Nombre,
                        Tipo = x.Project.ProjectType.Nombre,
                        Id_ProjectPhase = x.Project.Id_ProjectPhase,
                        Etapa = x.Project.ProjectPhase.Nombre,
                        Titulo = x.Project.Titulo,
                        ObjetivoGeneral = x.Project.ObjetivoGeneral,
                        ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                        Justificacion = x.Project.Justificacion,
                        Actividades = x.Project.Actividades,
                        Comentarios = x.Project.Comentarios,
                        CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                        //Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true) is null ? "Sin revisor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        //Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true) is null ? "Sin asesor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        CommentRevisor = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4).Mensaje,
                        Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                        EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                        TimeCreated = x.TimeCreated,
                        Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Id).Select(y => y.Career.Name).ToList()),
                        LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                    }).ToList();

                    ViewBag.ProjectList = projectList;
                }
                else
                {
                    ViewBag.ProjectList = null;
                }

                ViewBag.Total = total;

                return View();
            }
        }

        [HttpPost]
        public ActionResult Paginate(string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                long? TotalRegisters = null;

                #region Filtro
                switch (Filter)
                {
                    case "Pending":
                        TotalRegisters = User.IsInRole("Jefe academia") || User.IsInRole("Jefe departamental") ? DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase < 5 && x.Project.Id_ProjectType == 2 && x.Project.Active).Count() : DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase < 5 && x.Id_Person == UserId).Count();
                        break;
                    case "Accepted":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 3 && x.Project.Active == true).Count();
                        break;
                    case "AcceptedA":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 5 && x.Project.Id_ProjectType == 2 && x.Project.Active && x.Owner).Count();
                        break;
                    case "Rejected":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 1 && x.Project.Active == true).Count();
                        break;
                    case "Deleted":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == false).Count();
                        break;
                    case "Finished":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 4 && x.Project.Active == true).Count();
                        break;
                    case "Student":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId).Count();
                        break;
                    case "StudentBank":
                        TotalRegisters = DBC.ProjectCareers.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 5 && x.Project.Id_ProjectType == 2 && x.Id_Career == CareerId).Count();
                        break;
                    case "BankDocente":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId && x.Project.Id_ProjectType == 2).Count();
                        break;
                    case "PendingCC":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Owner && x.Project.Id_ProjectPhase == 7 && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).Count();
                        break;
                    case "PendingRevisor":
                        TotalRegisters = DBC.ProjectCareers.Where(x => x.Project.Active && x.Id_Project == x.Project.Id && x.Id_Career == CareerId && x.Project.Id_ProjectPhase == 8 && x.Project.ProjectPersons.Where(r => r.Owner).Select(t => t.Owner).FirstOrDefault()).Count();
                        break;
                    case "PendingAdviser":
                        TotalRegisters = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active && x.Project.Id_ProjectPhase >= 12 && x.Project.Id_ProjectPhase < 16 && x.Id_AdviserType == 2).Count();
                        break;
                    case "AllCC":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase >= 5 && x.Owner && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).Count();
                        break;
                    case "AllTeacher":
                        TotalRegisters = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active).Count();
                        break;
                    case "PendingAdviserJD":
                        TotalRegisters = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Owner && x.Project.Id_ProjectPhase == 11 && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).Count();
                        break;
                    default:
                        break;
                }
                #endregion

                return PartialView("~/Views/Shared/_Paginacion.cshtml", TotalRegisters);
            }
        }

        [HttpPost]
        public JsonResult Dictum(long Id, string Dictum, string Comment)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id);
                long UserId = (long)Session["Id"];

                switch (Dictum)
                {
                    case "Accept":
                        project.Id_ProjectPhase = project.Id_ProjectPhase is 2 ? 3 : project.Id_ProjectPhase is 3 ? 5 : project.Id_ProjectPhase is 7 ? 8 : project.Id_ProjectPhase is 10 ? 11 : 12; //la fase 12 aún está por definir;
                        break;
                    case "Reject":
                        project.Id_ProjectPhase = project.Id_ProjectPhase is 2 ? 1 : project.Id_ProjectPhase is 3 ? 4 : 6;

                        Comment comment = new Comment
                        {
                            Id_CommentType = User.IsInRole("Jefe academia") || User.IsInRole("Jefe departamental") && project.Id_ProjectType == 2 ? 2 : User.IsInRole("Coordinador de carrera") ? 3 : 4,
                            Id_Person = UserId,
                            Id_Project = Id,
                            Mensaje = Comment,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };
                        DBC.Comments.Add(comment);
                        break;
                    case "Unpublish":
                        project.Active = false;

                        Comment commentU = new Comment
                        {
                            Id_CommentType = 5,
                            Id_Person = UserId,
                            Id_Project = Id,
                            Mensaje = Comment,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };
                        DBC.Comments.Add(commentU);
                        break;
                    default:
                        break;
                }

                project.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Index(ProjectVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                if (modelo.Id > 0)
                {
                    Project project = DBC.Projects.FirstOrDefault(x => x.Id == modelo.Id);

                    project.Id_ProjectType = User.IsInRole("Alumno") ? 1 : 2;
                    project.Id_ProjectPhase = project.Id_ProjectPhase is 1 ? 2 : 3;
                    project.Id_Company = modelo.Id_Company;
                    project.Id_Nature = modelo.Id_Nature;
                    project.Id_Ambit = modelo.Id_Ambit;
                    project.Id_Kind = modelo.Id_Kind;
                    project.Titulo = modelo.Titulo;
                    project.ObjetivoGeneral = modelo.ObjetivoGeneral;
                    project.ObjetivosEspecificos = modelo.ObjetivosEspecificos;
                    project.Justificacion = modelo.Justificacion;
                    project.Actividades = modelo.Actividades;
                    project.Comentarios = modelo.Comentarios;
                    project.TimeUpdated = DateTime.Now;
                    project.Active = true;

                    DBC.ProjectCareers.RemoveRange(DBC.ProjectCareers.Where(x => x.Id_Project == project.Id));
                    DBC.SaveChanges();

                    foreach (var Id_Career in modelo.Id_Carreras)
                    {
                        ProjectCareer projectCareer = new ProjectCareer
                        {
                            Id_Career = Id_Career,
                            Id_Project = project.Id,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.ProjectCareers.Add(projectCareer);
                    }

                    DBC.SaveChanges();
                }
                else
                {
                    Project Project = new Project
                    {
                        Id_ProjectType = User.IsInRole("Alumno") ? 1 : 2,
                        Id_Company = modelo.Id_Company,
                        Id_Nature = modelo.Id_Nature,
                        Id_Ambit = modelo.Id_Ambit,
                        Id_Kind = modelo.Id_Kind,
                        Id_ProjectPhase = 2,
                        Titulo = modelo.Titulo,
                        ObjetivoGeneral = modelo.ObjetivoGeneral,
                        ObjetivosEspecificos = modelo.ObjetivosEspecificos,
                        Justificacion = modelo.Justificacion,
                        Actividades = modelo.Actividades,
                        Comentarios = modelo.Comentarios,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };


                    DBC.Projects.Add(Project);
                    DBC.SaveChanges();

                    long lastProjectId = Project.Id;

                    foreach (var Id_Career in modelo.Id_Carreras)
                    {
                        ProjectCareer projectCareer = new ProjectCareer
                        {
                            Id_Career = Id_Career,
                            Id_Project = lastProjectId,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.ProjectCareers.Add(projectCareer);
                    }

                    ProjectPerson projectPerson = new ProjectPerson
                    {
                        Id_Project = lastProjectId,
                        Id_Person = UserId,
                        Id_Dictum = 3,
                        Owner = true,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.ProjectPersons.Add(projectPerson);
                    DBC.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult StudentProject(ProjectVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                bool success = false;

                if (modelo.Id > 0)
                {
                    Project project = DBC.Projects.FirstOrDefault(x => x.Id == modelo.Id);
                    if (project.Active)
                    {
                        project.Id_ProjectType = 1;
                        project.Id_Company = null;
                        project.Id_Nature = modelo.Id_Nature;
                        project.Id_Ambit = modelo.Id_Ambit;
                        project.Id_Kind = modelo.Id_Kind;
                        project.Titulo = modelo.Titulo;
                        project.ObjetivoGeneral = "-";
                        project.ObjetivosEspecificos = "-";
                        project.Justificacion = "-";
                        project.Actividades = "-";
                        project.Comentarios = "-";
                        project.TimeUpdated = DateTime.Now;
                        project.Active = true;

                        DBC.ProjectCareers.RemoveRange(DBC.ProjectCareers.Where(x => x.Id_Project == project.Id));
                        DBC.SaveChanges();

                        foreach (var Id_Career in modelo.Id_Carreras)
                        {
                            ProjectCareer projectCareer = new ProjectCareer
                            {
                                Id_Career = Id_Career,
                                Id_Project = project.Id,
                                Active = true,
                                TimeCreated = DateTime.Now
                            };

                            DBC.ProjectCareers.Add(projectCareer);
                        }
                        DBC.SaveChanges();
                        success = true;
                    }
                }
                else
                {
                    ProjectPerson project = DBC.ProjectPersons.Where(x => x.Id_Person == UserId).OrderByDescending(x => x.Project.Id).FirstOrDefault();

                    bool canCreate = project is null ? true : project.Project.Active ? false : true;

                    if (canCreate)
                    {
                        Project newProject = new Project
                        {
                            Id_ProjectType = 1,
                            Id_Company = modelo.Id_Company,
                            Id_Nature = modelo.Id_Nature,
                            Id_Ambit = modelo.Id_Ambit,
                            Id_Kind = modelo.Id_Kind,
                            Id_ProjectPhase = 7,
                            Titulo = modelo.Titulo,
                            ObjetivoGeneral = "-",
                            ObjetivosEspecificos = "-",
                            Justificacion = "-",
                            Actividades = "-",
                            Comentarios = "-",
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Projects.Add(newProject);
                        DBC.SaveChanges();

                        long lastProjectId = newProject.Id;

                        //foreach (var Id_Career in modelo.Id_Carreras)
                        //{
                        //    ProjectCareer projectCareer = new ProjectCareer
                        //    {
                        //        Id_Career = Id_Career,
                        //        Id_Project = lastProjectId,
                        //        Active = true,
                        //        TimeCreated = DateTime.Now
                        //    };

                        //    DBC.ProjectCareers.Add(projectCareer);
                        //}

                        ProjectPerson projectPerson = new ProjectPerson
                        {
                            Id_Project = lastProjectId,
                            Id_Person = UserId,
                            Id_Dictum = 3,
                            Owner = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        var xxx = modelo.File.FileName.Substring(modelo.File.FileName.Length - 3).ToLower();

                        if (modelo.File.FileName.Substring(modelo.File.FileName.Length - 3).ToLower().Contains("pdf"))
                        {
                            string Folders = string.Concat("/Assets/pdf/anteproyectos/", (string)Session["Enrollment"], "/");
                            string NombreArchivo = Path.GetFileName(modelo.File.FileName);

                            ProjectFile projectFile = new ProjectFile
                            {
                                Id_Project = lastProjectId,
                                Nombre = NombreArchivo,
                                Tipo = modelo.File.ContentType,
                                Ruta = "-",
                                Proyecto = true,
                                Active = true,
                                TimeCreated = DateTime.Now
                            };

                            DBC.ProjectFiles.Add(projectFile);
                            DBC.SaveChanges();
                            long lastFileId = projectFile.Id;

                            Folders = string.Concat(Folders, lastFileId, "/");
                            string Ruta = Server.MapPath(Folders);
                            string Destination = Ruta + NombreArchivo;
                            string RelativePath = Folders + NombreArchivo;
                            Directory.CreateDirectory(Ruta);
                            modelo.File.SaveAs(Destination);

                            projectFile.Ruta = RelativePath;
                        }

                        DBC.ProjectPersons.Add(projectPerson);

                        if (!string.IsNullOrEmpty(modelo.Member))
                        {
                            string[] members = modelo.Member.Split(',');
                            foreach (var integrante in members)
                            {
                                string member = integrante.Replace(" ", "");
                                Person person = DBC.People.FirstOrDefault(x => x.Enrollment == member);

                                if (person != null)
                                {
                                    ProjectPerson newMember = new ProjectPerson
                                    {
                                        Id_Project = lastProjectId,
                                        Id_Person = person.Id,
                                        Id_Dictum = 2,
                                        Owner = false,
                                        Active = true,
                                        TimeCreated = DateTime.Now
                                    };
                                    DBC.ProjectPersons.Add(newMember);

                                    ProjectCareer projectCareer = new ProjectCareer
                                    {
                                        Id_Career = person.CareerId,
                                        Id_Project = lastProjectId,
                                        Active = true,
                                        TimeCreated = DateTime.Now
                                    };

                                    DBC.ProjectCareers.Add(projectCareer);
                                }
                            }
                        }
                        DBC.SaveChanges();
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                }

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Comment(long Id, string Dictum)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                CommentVM modelo = new CommentVM
                {
                    Id = Id
                };

                if (Dictum == "Reject")
                {
                    return PartialView("~/Views/Project/_CommentReject.cshtml", modelo);
                }
                else
                {
                    return PartialView("~/Views/Project/_CommentUnpublish.cshtml", modelo);
                }
            }
        }

        [HttpPost]
        public ActionResult Details(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectVM modelo = new ProjectVM();

                List<Company> companies = DBC.Companies.Where(x => x.Active == true && x.Id_Dictum == 3).ToList();
                ViewBag.CompanyList = new SelectList(companies, "Id", "Nombre");

                List<Nature> natures = DBC.Natures.Where(x => x.Active == true).ToList();
                ViewBag.NatureList = new SelectList(natures, "Id", "Nombre");

                List<Ambit> ambits = DBC.Ambits.Where(x => x.Active == true).ToList();
                ViewBag.AmbitList = new SelectList(ambits, "Id", "Nombre");

                List<Kind> kinds = DBC.Kinds.Where(x => x.Active == true).ToList();
                ViewBag.KindList = new SelectList(kinds, "Id", "Nombre");

                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.CareerList = new SelectList(careers, "Id", "Name");


                if (Id > 0)
                {
                    Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = project.Id;
                    modelo.Id_ProjectType = project.Id_ProjectType;
                    modelo.Id_Company = project.Id_Company;
                    modelo.Id_Nature = project.Id_Nature;
                    modelo.Id_Ambit = project.Id_Ambit;
                    modelo.Id_Kind = project.Id_Kind;
                    modelo.Titulo = project.Titulo;
                    modelo.ObjetivoGeneral = project.ObjetivoGeneral;
                    modelo.ObjetivosEspecificos = project.ObjetivosEspecificos;
                    modelo.Justificacion = project.Justificacion;
                    modelo.Comentarios = project.Comentarios;
                    modelo.Actividades = project.Actividades;
                    modelo.Id_Carreras = project.ProjectCareers.Where(y => y.Id_Project == Id).Select(y => y.Id_Career).ToList();
                    modelo.Comments = User.IsInRole("Alumno") ? project.Comments.OrderByDescending(z => z.TimeCreated).Where(z => z.Id_Project == Id && z.Id_CommentType != 1 && z.Id_CommentType != 2).Select(z => new CommentVM { Autor = string.Concat(z.Person.Name, " ", z.Person.MiddleName, " ", z.Person.LastName), Mensaje = z.Mensaje, TimeCreated = z.TimeCreated }).ToList() : project.Comments.OrderByDescending(z => z.TimeCreated).Where(z => z.Id_Project == Id).Select(z => new CommentVM { Autor = string.Concat(z.Person.Name, " ", z.Person.MiddleName, " ", z.Person.LastName), Mensaje = z.Mensaje, TimeCreated = z.TimeCreated }).ToList();
                }

                return modelo.Id_ProjectType is 1 ? PartialView("~/Views/Project/_SideStudentDetails.cshtml", modelo) : PartialView("~/Views/Project/_SideDetails.cshtml", modelo);

            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id, bool ReadOnly)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectVM modelo = new ProjectVM();

                List<Company> companies = DBC.Companies.Where(x => x.Active == true && x.Id_Dictum == 3).ToList();
                ViewBag.CompanyList = new SelectList(companies, "Id", "Nombre");

                List<Nature> natures = DBC.Natures.Where(x => x.Active == true).ToList();
                ViewBag.NatureList = new SelectList(natures, "Id", "Nombre");

                List<Ambit> ambits = DBC.Ambits.Where(x => x.Active == true).ToList();
                ViewBag.AmbitList = new SelectList(ambits, "Id", "Nombre");

                List<Kind> kinds = DBC.Kinds.Where(x => x.Active == true).ToList();
                ViewBag.KindList = new SelectList(kinds, "Id", "Nombre");

                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.CareerList = new SelectList(careers, "Id", "Name");


                if (Id > 0)
                {
                    Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = project.Id;
                    modelo.Id_ProjectType = project.Id_ProjectType;
                    modelo.Id_Company = project.Id_Company;
                    modelo.Id_Nature = project.Id_Nature;
                    modelo.Id_Ambit = project.Id_Ambit;
                    modelo.Id_Kind = project.Id_Kind;
                    modelo.Titulo = project.Titulo;
                    modelo.ObjetivoGeneral = project.ObjetivoGeneral;
                    modelo.ObjetivosEspecificos = project.ObjetivosEspecificos;
                    modelo.Justificacion = project.Justificacion;
                    modelo.Comentarios = project.Comentarios;
                    modelo.Actividades = project.Actividades;
                    modelo.Id_Carreras = project.ProjectCareers.Where(y => y.Id_Project == Id).Select(y => y.Id_Career).ToList();
                    modelo.Comments = User.IsInRole("Alumno") ? project.Comments.OrderByDescending(z => z.TimeCreated).Where(z => z.Id_Project == Id && z.Id_CommentType != 1 && z.Id_CommentType != 2).Select(z => new CommentVM
                    {
                        Id_CommentType = z.Id_CommentType,
                        CommentType = z.CommentType.Nombre,
                        Autor = string.Concat(z.Person.Name, " ", z.Person.MiddleName, " ", z.Person.LastName),
                        Rol = z.Person.PersonPermissions.Where(y => y.PersonId == z.Id_Person).Select(x => x.Permission.Name).FirstOrDefault(),
                        Mensaje = z.Mensaje,
                        TimeCreated = z.TimeCreated
                    }).ToList() : project.Comments.OrderByDescending(z => z.TimeCreated).Where(z => z.Id_Project == Id).Select(z => new CommentVM
                    {
                        Id_CommentType = z.Id_CommentType,
                        CommentType = z.CommentType.Nombre,
                        Autor = string.Concat(z.Person.Name, " ", z.Person.MiddleName, " ", z.Person.LastName),
                        Rol = z.Person.PersonPermissions.Where(y => y.PersonId == z.Id_Person).Select(x => x.Permission.Name).FirstOrDefault(),
                        Mensaje = z.Mensaje,
                        TimeCreated = z.TimeCreated
                    }).ToList();
                }

                if (ReadOnly)
                {
                    return modelo.Id_ProjectType is 1 ? PartialView("~/Views/Project/_StudentDetails.cshtml", modelo) : PartialView("~/Views/Project/_Details.cshtml", modelo);
                }
                else
                {
                    return User.IsInRole("Alumno") && modelo.Id_ProjectType is 2 ? PartialView("~/Views/Project/_Details.cshtml", modelo) : User.IsInRole("Alumno") ? PartialView("~/Views/Project/_StudentAddEdit.cshtml", modelo) : PartialView("~/Views/Project/_AddEdit.cshtml", modelo);
                }
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Project Project = DBC.Projects.FirstOrDefault(x => x.Id == Id);

                if (Project.Active)
                {
                    Project.Active = false;
                }
                else
                {
                    Project.Active = true;
                }

                Project.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SendAnteproyecto(ProjectVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                bool success = false;
                string message = string.Empty;

                if (modelo.File != null)
                {
                    if (modelo.File.FileName.Substring(modelo.File.FileName.Length - 3).ToLower().Contains("pdf"))
                    {
                        Project project = DBC.Projects.FirstOrDefault(x => x.Id == modelo.Id);
                        string Folders = string.Concat("/Assets/pdf/anteproyectos/", (string)Session["Enrollment"], "/");
                        string NombreArchivo = Path.GetFileName(modelo.File.FileName);

                        ProjectFile projectFile = new ProjectFile
                        {
                            Id_Project = modelo.Id,
                            Nombre = NombreArchivo,
                            Tipo = modelo.File.ContentType,
                            Ruta = "-",
                            Proyecto = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.ProjectFiles.Add(projectFile);
                        DBC.SaveChanges();
                        long lastFileId = projectFile.Id;

                        Folders = string.Concat(Folders, lastFileId, "/");
                        string Ruta = Server.MapPath(Folders);
                        string Destination = Ruta + NombreArchivo;
                        string RelativePath = Folders + NombreArchivo;
                        Directory.CreateDirectory(Ruta);
                        modelo.File.SaveAs(Destination);

                        projectFile.Ruta = RelativePath;
                        project.Id_ProjectPhase = project.Id_ProjectPhase is 5 || project.Id_ProjectPhase is 6 ? 7 : 10;
                        project.TimeUpdated = DateTime.Now;

                        success = true;
                        message = "El anteproyecto se guardó éxitosamente";
                        DBC.SaveChanges();
                    }
                    else
                    {
                        message = "El archivo del anteproyecto debe ser formato PDF";
                    }
                }
                else
                {
                    message = "Debes adjuntar el archivo PDF del anteproyecto";
                }

                return Json(new { message = message, success = success }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddAnteproyecto(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ProjectVM project = new ProjectVM
                {
                    Id = Id
                };

                return PartialView("~/Views/Project/_StudentAddAnteproyecto.cshtml", project);
            }
        }

        #region Bitácora de proyectos para docentes
        [HttpPost]
        public ActionResult ProjectListTeacher(string Filter, string Keyword, int Skip = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                IEnumerable<Adviser> projects = Enumerable.Empty<Adviser>();

                #region Filtro
                switch (Filter)
                {
                    case "Pending":
                        projects = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active && (x.Project.Id_ProjectPhase == 9 || x.Project.Id_ProjectPhase == 10) && x.Id_AdviserType == 1).Take(12).ToList();
                        break;
                    case "PendingAdviser":
                        projects = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active && x.Project.Id_ProjectPhase >= 12 && x.Project.Id_ProjectPhase < 16 && x.Id_AdviserType == 2).Take(12).ToList();
                        break;
                    case "AllTeacher":
                        projects = DBC.Advisers.Where(x => x.Id_Person == UserId && x.Active && x.Project.Active).Take(12).ToList();
                        break;
                    default:
                        break;
                }
                #endregion

                long TotalRecords = projects.Count();

                #region Búsqueda
                if (!string.IsNullOrEmpty(Keyword))
                {
                    Keyword = Keyword.ToLower();

                    projects = projects.Where(x => x.Project.Titulo.ToLower().Contains(Keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(Keyword)
                    );
                }
                #endregion

                // PAGINADO
                int skip = 12 * Skip;
                projects = projects.Skip(skip).Take(12);

                #region Lista
                List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                {
                    Id = x.Project.Id,
                    TipoDeProyecto = x.Project.ProjectType.Nombre,
                    Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                    Caracter = x.Project.Nature.Nombre,
                    Ambito = x.Project.Ambit.Nombre,
                    Tipo = x.Project.ProjectType.Nombre,
                    Id_ProjectPhase = x.Project.Id_ProjectPhase,
                    Etapa = x.Project.ProjectPhase.Nombre,
                    Titulo = x.Project.Titulo,
                    ObjetivoGeneral = x.Project.ObjetivoGeneral,
                    ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                    Justificacion = x.Project.Justificacion,
                    Actividades = x.Project.Actividades,
                    Comentarios = x.Project.Comentarios,
                    CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                    Active = x.Project.Active,
                    TimeCreated = x.Project.TimeCreated,
                    Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Career.Name).ToList()),
                    Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_Dictum == 3).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email,
                        ProjectOwner = y.Owner
                    }).ToList(),
                    Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                }).ToList();
                #endregion

                return PartialView("~/Views/Project/_SearchCC.cshtml", projectList);
            }
        }
        #endregion

        #region Bitácora de proyectos para los alumnos
        [HttpPost]
        public ActionResult ProjectListStudent(string Filter, string Keyword, int Skip = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];

                if (Filter == "MyProject" || Filter == "Unpublished")
                {
                    IEnumerable<ProjectPerson> myProject = Enumerable.Empty<ProjectPerson>();

                    #region Filtro
                    switch (Filter)
                    {
                        case "Unpublished":
                            myProject = DBC.ProjectPersons.Where(x => x.Project.Active == false && x.Id_Person == UserId).ToList();
                            break;
                        case "MyProject":
                            myProject = DBC.ProjectPersons.Where(x => (x.Project.Active && x.Id_Person == UserId && x.Project.Id_ProjectType == 1) || (x.Project.Active && x.Project.Id_ProjectType == 2 && x.Id_Dictum == 3 && x.Id_Person == UserId)).ToList();
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Myproyect
                    List<ProjectVM> projectList = myProject.Select(x => new ProjectVM
                    {
                        Id = x.Project.Id,
                        TipoDeProyecto = x.Project.ProjectType.Nombre,
                        Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                        Caracter = x.Project.Nature.Nombre,
                        Ambito = x.Project.Ambit.Nombre,
                        Tipo = x.Project.ProjectType.Nombre,
                        Id_ProjectPhase = x.Project.Id_ProjectPhase,
                        Etapa = x.Project.ProjectPhase.Nombre,
                        Titulo = x.Project.Titulo,
                        ObjetivoGeneral = x.Project.ObjetivoGeneral,
                        ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                        Justificacion = x.Project.Justificacion,
                        Actividades = x.Project.Actividades,
                        Comentarios = x.Project.Comentarios,
                        CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                        //Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true) is null ? "Sin revisor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        PDFExists = x.Project.ProjectFiles.Any(y => y.Id_Project == x.Project.Id && y.Proyecto && y.Active),
                        //Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true) is null ? "Sin asesor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        CommentRevisor = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4).Mensaje,
                        Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                        EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                        TimeCreated = x.TimeCreated,
                        Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Id).Select(y => y.Career.Name).ToList()),
                        LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                    }).ToList();
                    #endregion

                    return PartialView("~/Views/Project/_StudentSearch.cshtml", projectList);
                }
                else
                {
                    IEnumerable<ProjectCareer> projects = DBC.ProjectCareers.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 5 && x.Project.Id_ProjectType == 2 && x.Id_Career == CareerId).ToList();

                    long TotalRecords = projects.Count();

                    #region Búsqueda
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        Keyword = Keyword.ToLower();

                        projects = projects.Where(x => x.Project.Titulo.ToLower().Contains(Keyword) ||
                        x.TimeCreated.ToString().ToLower().Contains(Keyword)
                        );
                    }
                    #endregion

                    // PAGINADO
                    int skip = 12 * Skip;
                    projects = projects.Skip(skip).Take(12);

                    #region Lista
                    List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                    {
                        Id = x.Project.Id,
                        TipoDeProyecto = x.Project.ProjectType.Nombre,
                        Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                        Caracter = x.Project.Nature.Nombre,
                        Ambito = x.Project.Ambit.Nombre,
                        Tipo = x.Project.ProjectType.Nombre,
                        Id_ProjectPhase = x.Project.Id_ProjectPhase,
                        Etapa = x.Project.ProjectPhase.Nombre,
                        Titulo = x.Project.Titulo,
                        ObjetivoGeneral = x.Project.ObjetivoGeneral,
                        ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                        Justificacion = x.Project.Justificacion,
                        Actividades = x.Project.Actividades,
                        Comentarios = x.Project.Comentarios,
                        CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                        //Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true) is null ? "Sin revisor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                        {
                            UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                            Enrollment = y.Person.Enrollment,
                            Email = y.Person.Email
                        }).ToList(),
                        //Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true) is null ? "Sin asesor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 2 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                        CommentRevisor = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 4).Mensaje,
                        Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                        EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                        TimeCreated = x.TimeCreated,
                        Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Id).Select(y => y.Career.Name).ToList()),
                        LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Id).Select(y => y.Mensaje).LastOrDefault()
                    }).ToList();
                    #endregion

                    return PartialView("~/Views/Project/_StudentBank.cshtml", projectList);
                }
            }
        }
        #endregion

        #region Bitácora de proyectos
        [HttpPost]
        public ActionResult ProjectList(string Filter, string Keyword, int Skip = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                IEnumerable<ProjectPerson> projects = Enumerable.Empty<ProjectPerson>();

                #region Filtro
                switch (Filter)
                {
                    case "Pending":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase < 5 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase < 5 && x.Project.Id_ProjectType == 2 && x.Project.Active).ToList();
                        break;
                    case "AcceptedJD":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 3 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 3 && x.Project.Id_ProjectType == 2 && x.Project.Active).ToList();
                        break;
                    case "RejectedJD":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 1 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 1 && x.Project.Id_ProjectType == 2 && x.Project.Active).ToList();
                        break;
                    case "RejectedA":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 4 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 4 && x.Project.Id_ProjectType == 2 && x.Project.Active).ToList();
                        break;
                    case "AcceptedA":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase == 5 && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Id_ProjectPhase == 5 && x.Project.Id_ProjectType == 2 && x.Project.Active && x.Owner).ToList();
                        break;
                    case "Unpublished":
                        projects = User.IsInRole("Docente") ? DBC.ProjectPersons.Where(x => x.Project.Active == false && x.Id_Person == UserId).ToList() : DBC.ProjectPersons.Where(x => x.Project.Active == false && x.Project.Id_ProjectType == 2).ToList();
                        break;
                    case "Student":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId).ToList();
                        break;
                    case "Project":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId && x.Project.Id_ProjectType == 2).ToList();
                        break;
                    case "BankDocente":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Id_Person == UserId && x.Project.Id_ProjectType == 2).ToList();
                        break;
                    case "PendingCC":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Owner && x.Project.Id_ProjectPhase == 7 && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).ToList();
                        break;
                    case "AllCC":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Project.Id_ProjectPhase >= 5 && x.Owner && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).ToList();
                        break;
                    case "PendingJD":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Owner && x.Project.Id_ProjectPhase == 8 && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).ToList();
                        break;
                    case "PendingAdviserJD":
                        projects = DBC.ProjectPersons.Where(x => x.Project.Active == true && x.Owner && x.Project.Id_ProjectPhase == 11 && x.Project.ProjectCareers.Where(r => r.Id_Career == CareerId).Select(t => t.Id_Career).FirstOrDefault() == CareerId).ToList();
                        break;
                    default:
                        break;
                }
                #endregion

                long TotalRecords = projects.Count();

                #region Búsqueda
                if (!string.IsNullOrEmpty(Keyword))
                {
                    Keyword = Keyword.ToLower();

                    projects = projects.Where(x => x.Project.Titulo.ToLower().Contains(Keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(Keyword)
                    );
                }
                #endregion

                // PAGINADO
                int skip = 12 * Skip;
                projects = projects.Skip(skip).Take(12);

                #region Lista
                List<ProjectVM> projectList = projects.Select(x => new ProjectVM
                {
                    Id = x.Project.Id,
                    TipoDeProyecto = x.Project.ProjectType.Nombre,
                    Empresa = x.Project.Company is null ? "-" : x.Project.Company.Nombre,
                    Caracter = x.Project.Nature.Nombre,
                    Ambito = x.Project.Ambit.Nombre,
                    Tipo = x.Project.ProjectType.Nombre,
                    Id_ProjectPhase = x.Project.Id_ProjectPhase,
                    Etapa = x.Project.ProjectPhase.Nombre,
                    Titulo = x.Project.Titulo,
                    ObjetivoGeneral = x.Project.ObjetivoGeneral,
                    ObjetivosEspecificos = x.Project.ObjetivosEspecificos,
                    Justificacion = x.Project.Justificacion,
                    Actividades = x.Project.Actividades,
                    Comentarios = x.Project.Comentarios,
                    CommentCC = x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3) is null ? "Aún no se publican comentarios" : x.Project.Comments.LastOrDefault(y => y.Id_Project == x.Project.Id && y.Id_CommentType == 3).Mensaje,
                    Active = x.Project.Active,
                    TimeCreated = x.Project.TimeCreated,
                    //Presentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => string.Concat(s.Person.Name, " ", s.Person.MiddleName, " ", s.Person.LastName)).FirstOrDefault(),
                    //EmailPresentador = x.Project.ProjectPersons.Where(g => g.Id_Project == x.Project.Id).Select(s => s.Person.Email).FirstOrDefault(),
                    Carrera = string.Join(", ", x.Project.ProjectCareers.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Career.Name).ToList()),
                    Miembros = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_Dictum == 3).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email,
                        ProjectOwner = y.Owner
                    }).ToList(),
                    Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 1).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    Asesores = x.Project.Advisers.Where(y => y.Id_Project == x.Project.Id && y.Active && y.Id_AdviserType == 2).Select(y => new PersonVM
                    {
                        UserFullName = string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName),
                        Enrollment = y.Person.Enrollment,
                        Email = y.Person.Email
                    }).ToList(),
                    MiembrosPostulados = x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Id_Dictum == 2).Count(),
                    //Revisores = x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true) is null ? "Sin revisor asignado" : string.Join(", ", x.Project.Advisers.Where(y => y.Id_Project == x.Id && y.Id_AdviserType == 1 && y.Active == true).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName)).ToList()),
                    //Miembros = string.Join(", ", x.Project.ProjectPersons.Where(y => y.Id_Project == x.Project.Id && y.Owner != true && y.Id_Dictum == 3).Select(y => string.Concat(y.Person.Name, " ", y.Person.MiddleName, " ", y.Person.LastName, " (", y.Person.Enrollment, ")")).ToList()),
                    LastComment = x.Project.Comments.Where(y => y.Id_Project == x.Project.Id).Select(y => y.Mensaje).LastOrDefault()
                }).ToList();
                #endregion

                return Filter.Contains("BankDocente") ? PartialView("~/Views/Project/_SearchBank.cshtml", projectList) : User.IsInRole("Coordinador de carrera") || User.IsInRole("Jefe departamental") || User.IsInRole("Jefe academia") || User.IsInRole("Docente") ? PartialView("~/Views/Project/_SearchCC.cshtml", projectList) : PartialView("~/Views/Project/_Search.cshtml", projectList);
            }
        }
        #endregion
    }
}