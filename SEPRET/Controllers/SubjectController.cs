using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class SubjectController : Controller
    {
        // GET: Subject
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateTeacher(long Id_Person, long Id_Subject)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Subject subject = DBC.Subjects.FirstOrDefault(x => x.Id == Id_Subject);

                PersonSubject personSubject = DBC.PersonSubjects.FirstOrDefault(x => x.Id_Subject == Id_Subject && x.Id_Person == Id_Person);

                if (personSubject != null)
                {
                    personSubject.Active = personSubject.Active is false;
                }
                else
                {
                    personSubject = new PersonSubject
                    {
                        Id_Subject = Id_Subject,
                        Id_Person = Id_Person,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.PersonSubjects.Add(personSubject);
                }

                subject.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        #region DataTable Docentes
        [HttpPost]
        public JsonResult TeacherDT(DataTablesParameters param, string Filter, long Id_Subject)
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
                //Project project = DBC.Projects.FirstOrDefault(x => x.Id == Id_Project);
                //List<PersonVM> data = project is null ? null : project.Id_ProjectPhase >= 11 ?
                //people.Select(x => new PersonVM
                //{
                //    Id = x.Id,
                //    UserFullName = string.Concat(x.Name, " ", x.MiddleName, " ", x.LastName),
                //    Active = x.Advisers.Where(y => y.Project.Id_ProjectPhase >= 11 && y.Project.Active && y.Id_Person == y.Person.Id && y.Id_Project == Id_Project && y.Id_AdviserType == 2).Select(z => z.Active).FirstOrDefault(),
                //    Projects = x.Advisers.Where(y => y.Project.Id_ProjectPhase >= 11 && y.Project.Active && y.Active && y.Id_Person == x.Id && y.Id_AdviserType == 2).Select(z => new ProjectVM
                //    {
                //        Id = z.Project.Id,
                //        Titulo = z.Project.Titulo
                //    }).ToList(),
                //    TimeCreatedFormatted = x.TimeCreated.ToString()
                //}).ToList() :
                //people.Select(x => new PersonVM
                //{
                //    Id = x.Id,
                //    UserFullName = string.Concat(x.Name, " ", x.MiddleName, " ", x.LastName),
                //    Active = x.Advisers.Where(y => y.Project.Id_ProjectPhase == 9 || y.Project.Id_ProjectPhase == 10 && y.Project.Active && y.Id_Person == y.Person.Id && y.Id_Project == Id_Project).Select(z => z.Active).FirstOrDefault(),
                //    Projects = x.Advisers.Where(y => y.Project.Id_ProjectPhase == 9 || y.Project.Id_ProjectPhase == 10 && y.Project.Active && y.Active && y.Id_Person == x.Id).Select(z => new ProjectVM
                //    {
                //        Id = z.Project.Id,
                //        Titulo = z.Project.Titulo
                //    }).ToList(),
                //    TimeCreatedFormatted = x.TimeCreated.ToString()
                //}).ToList();
                List<PersonVM> data = people.Select(x => new PersonVM
                {
                    Id = x.Id,
                    UserFullName = string.Concat(x.Name, " ", x.MiddleName, " ", x.LastName),
                    Active = x.PersonSubjects.Where(y => y.Subject.Active && y.Id_Person == y.Person.Id && y.Id_Subject == Id_Subject).Select(z => z.Active).FirstOrDefault(),
                    Subjects = x.PersonSubjects.Where(y => y.Subject.Active && y.Active && y.Id_Person == x.Id).Select(z => new SubjectVM
                    {
                        Id = z.Subject.Id,
                        Nombre = z.Subject.Nombre
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
        public ActionResult AddEditDocente(long Id)
        {
            return PartialView("~/Views/Subject/_AddEditDocente.cshtml", Id);
        }

        [HttpPost]
        public ActionResult ListInstrumentacion(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Unit> units = DBC.Units.Where(x => x.Id_Subject == Id && x.Active).ToList();
                ViewBag.NombreAsignatura = DBC.Subjects.FirstOrDefault(x => x.Id == x.Id && x.Active).Nombre;

                IEnumerable<UnitVM> unitVMs = units.Select(x => new UnitVM
                {
                    Id = x.Id,
                    Id_Subject = x.Id_Subject,
                    Indice = x.Indice,
                    Competencia = x.Competencia,
                    Descripcion = x.Descripcion,
                    Active = x.Active,
                    TimeCreated = x.TimeCreated,
                    Topics = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 3).ToList(),
                    LearningActivities = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 4).ToList(),
                    TeachingActivities = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 5).ToList(),
                    Proeficiencies = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 6).ToList()
                }).ToList();

                return PartialView("~/Views/Subject/_Search.cshtml", unitVMs);
            }
        }

        public ActionResult Instrumentacion(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Unit> units = DBC.Units.Where(x => x.Id_Subject == Id && x.Active).ToList();
                ViewBag.NombreAsignatura = DBC.Subjects.FirstOrDefault(x => x.Id == x.Id && x.Active).Nombre;

                IEnumerable<UnitVM> unitVMs = units.Select(x => new UnitVM
                {
                    Id = x.Id,
                    Id_Subject = x.Id_Subject,
                    Indice = x.Indice,
                    Competencia = x.Competencia,
                    Descripcion = x.Descripcion,
                    Active = x.Active,
                    TimeCreated = x.TimeCreated,
                    Topics = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 3).ToList(),
                    LearningActivities = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 4).ToList(),
                    TeachingActivities = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 5).ToList(),
                    Proeficiencies = x.Instrumentations.Where(y => y.Id_Unit == x.Id && y.Active && y.Id_InstrumentationType == 6).ToList()
                }).ToList();

                return View(unitVMs);
            }
        }

        public void DeleteUnit(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Unit unit = DBC.Units.FirstOrDefault(x => x.Id == Id);
                unit.Active = false;

                var instrumentation = DBC.Instrumentations.Where(x => x.Id_Unit == Id).ToList();
                instrumentation.ForEach(x => x.Active = false);
                DBC.SaveChanges();
            }
        }

        public List<string> CleanList(List<string> CleanList)
        {
            foreach (var indice in CleanList.ToList())
            {
                if (string.IsNullOrEmpty(indice))
                {
                    CleanList.Remove(indice);
                }
            }
            return CleanList;
        }

        [HttpPost]
        public JsonResult SaveInstrumentation(InstrumentationVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                var topics = new List<string>();
                var index = new List<string>();
                var description = new List<string>();
                bool success = false;

                string IndiceUnidad = !string.IsNullOrEmpty(modelo.Competencia) ? Regex.Replace(modelo.Competencia, @"\D", "") : "-";
                string CompetenciaUnidad = !string.IsNullOrEmpty(modelo.Competencia) ? Regex.Replace(modelo.Competencia, @"\n*\d+(\.|\d*)+\s", "") : "-";

                if (modelo.Id_Unit > 0)
                {
                    success = true;
                }
                else
                {
                    var LearningActivities = new List<string>();
                    var TeachingActivities = new List<string>();
                    var Proeficiencies = new List<string>();

                    LearningActivities = CleanList(modelo.learningActivitiesRAW.Split('').ToList());
                    TeachingActivities = CleanList(modelo.teachingActivitiesRAW.Split('').ToList());
                    Proeficiencies = CleanList(modelo.proeficienciesRAW.Split('').ToList());
                    Regex getTemas = new Regex(@"(\d+(\.|\d*)+\D+\n\D+)|(\d+(\.|\d*)+\D+)");

                    Unit unit = new Unit
                    {
                        Id_Subject = modelo.Id_Subject,
                        Indice = IndiceUnidad,
                        Competencia = CompetenciaUnidad,
                        Descripcion = modelo.Descripcion.Replace("\r\n", " ").Trim(),
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Units.Add(unit);
                    DBC.SaveChanges();
                    long LastUnitInsertId = unit.Id;

                    foreach (Match match in getTemas.Matches(modelo.topicsRAW))
                    {
                        topics.Add(match.Value);
                    }

                    topics = CleanList(topics);

                    foreach (var item in topics)
                    {
                        string replace = item.Replace("\r\n", " ").Trim();
                        index.Add(CleanList(Regex.Split(replace, @"[^\d+(\.|\d*)+].*").ToList()).First());
                        description.Add(CleanList(Regex.Split(replace, @"\n*\d+(\.|\d*)+\s\n*").ToList()).First());
                    }

                    for (int i = 0; i < topics.Count; i++)
                    {
                        string indice = index[i];
                        string tema = description[i];

                        Instrumentation instrumentation = new Instrumentation
                        {
                            Id_Unit = LastUnitInsertId,
                            Id_InstrumentationType = 3,
                            Indice = indice,
                            Descripcion = tema,
                            Preset = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Instrumentations.Add(instrumentation);
                    }

                    foreach (var item in LearningActivities)
                    {
                        Instrumentation learningActivity = new Instrumentation
                        {
                            Id_Unit = LastUnitInsertId,
                            Id_InstrumentationType = 4,
                            Indice = "-",
                            Descripcion = item.Trim(),
                            Preset = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Instrumentations.Add(learningActivity);
                    }

                    foreach (var item in TeachingActivities)
                    {
                        Instrumentation teachingActivity = new Instrumentation
                        {
                            Id_Unit = LastUnitInsertId,
                            Id_InstrumentationType = 5,
                            Indice = "-",
                            Descripcion = item.Trim(),
                            Preset = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Instrumentations.Add(teachingActivity);
                    }

                    foreach (var item in Proeficiencies)
                    {
                        Instrumentation teachingActivity = new Instrumentation
                        {
                            Id_Unit = LastUnitInsertId,
                            Id_InstrumentationType = 6,
                            Indice = "-",
                            Descripcion = item.Trim(),
                            Preset = true,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Instrumentations.Add(teachingActivity);
                    }

                    Instrumentation HorasTP = new Instrumentation
                    {
                        Id_Unit = LastUnitInsertId,
                        Id_InstrumentationType = 7,
                        Indice = "-",
                        Descripcion = string.Concat(modelo.HorasTeoricas, "T - ", modelo.HorasPracticas, "P"),
                        Preset = true,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Instrumentations.Add(HorasTP);
                    DBC.SaveChanges();

                    success = true;
                }

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddEditInstrumentation(long Id, long IdSubject)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                InstrumentationVM modelo = new InstrumentationVM();
                modelo.Id_Unit = Id;
                modelo.Id_Subject = IdSubject;

                if (modelo.Id_Unit > 0)
                {
                    Unit unit = DBC.Units.FirstOrDefault(x => x.Id == Id);

                    modelo.topics = unit.Instrumentations.Where(x => x.Id_InstrumentationType == 3 && x.Id_Unit == modelo.Id_Unit && x.Active).ToList();
                    modelo.learningActivities = unit.Instrumentations.Where(x => x.Id_InstrumentationType == 4 && x.Id_Unit == modelo.Id_Unit && x.Active).ToList();
                    modelo.teachingActivities = unit.Instrumentations.Where(x => x.Id_InstrumentationType == 5 && x.Id_Unit == modelo.Id_Unit && x.Active).ToList();
                    modelo.proeficiencies = unit.Instrumentations.Where(x => x.Id_InstrumentationType == 6 && x.Id_Unit == modelo.Id_Unit && x.Active).ToList();
                }
                return PartialView("~/Views/Subject/_AddEditInstrumentation.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult Index(SubjectVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Subject subject = DBC.Subjects.FirstOrDefault(x => x.Id == modelo.Id);

                    subject.Id_Career = (long)Session["CareerId"];
                    subject.Nombre = modelo.Nombre;
                    subject.Clave = modelo.Clave;
                    subject.HorasTeoricas = modelo.HorasTeoricas;
                    subject.HorasPracticas = modelo.HorasPracticas;
                    subject.Creditos = modelo.Creditos;
                    subject.Competencia = modelo.Competencia;
                    subject.Caracterizacion = modelo.Caracterizacion;
                    subject.IntencionDidactica = modelo.IntencionDidactica;
                    subject.TimeUpdated = DateTime.Now;
                }
                else
                {
                    Subject subject = new Subject
                    {
                        Id_Career = (long)Session["CareerId"],
                        Nombre = modelo.Nombre,
                        Clave = modelo.Clave,
                        HorasTeoricas = modelo.HorasTeoricas,
                        HorasPracticas = modelo.HorasPracticas,
                        Creditos = modelo.Creditos,
                        Competencia = modelo.Competencia,
                        Caracterizacion = modelo.Caracterizacion,
                        IntencionDidactica = modelo.IntencionDidactica,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Subjects.Add(subject);
                }

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddEdit(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                SubjectVM modelo = new SubjectVM();

                if (Id > 0)
                {
                    Subject subject = DBC.Subjects.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Id;
                    modelo.Nombre = subject.Nombre;
                    modelo.Clave = subject.Clave;
                    modelo.HorasTeoricas = subject.HorasTeoricas;
                    modelo.HorasPracticas = subject.HorasPracticas;
                    modelo.Creditos = subject.Creditos;
                    modelo.Competencia = subject.Competencia;
                    modelo.Caracterizacion = subject.Caracterizacion;
                    modelo.IntencionDidactica = subject.IntencionDidactica;
                }

                return PartialView("~/Views/Subject/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Subject subject = DBC.Subjects.FirstOrDefault(x => x.Id == Id);

                if (subject.Active)
                {
                    subject.Active = false;
                }
                else
                {
                    subject.Active = true;
                }

                subject.TimeUpdated = DateTime.Now;
                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Subject> subjects = Enumerable.Empty<Subject>();
                long CareerId = (long)Session["CareerId"];
                long UserId = (long)Session["Id"];
                string Rol = System.Web.Security.Roles.GetRolesForUser().Single().ToLower();

                switch (Rol)
                {
                    case "super administrador":
                    case "administrador":
                        subjects = DBC.Subjects.Where(x => x.Active && x.Id_Career == CareerId).ToList();
                        break;
                    case "jefe departamental":
                        subjects = DBC.Subjects.Where(x => x.Active && x.Id_Career == CareerId).ToList();
                        break;
                    case "docente":
                        long?[] asignaturas = DBC.PersonSubjects.Where(x => x.Id_Person == UserId && x.Active).Select(x => x.Id_Subject).ToArray();
                        subjects = DBC.Subjects.Where(x => x.Active && asignaturas.Contains(x.Id)).ToList();
                        break;
                    default:
                        break;
                }

                long total = subjects.Count();

                #region Busqueda
                if (!string.IsNullOrEmpty(param.search.value))
                {
                    string Keyword = param.search.value.ToLower();

                    subjects = subjects.Where(x => x.Nombre.ToLower().Contains(Keyword) || x.Clave.ToLower().Contains(Keyword) ||
                    x.Competencia.ToLower().Contains(Keyword) || x.Caracterizacion.ToLower().Contains(Keyword) || x.IntencionDidactica.ToLower().Contains(Keyword)).ToList();
                }

                long totalFiltered = subjects.Count();

                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Subject, string> orderFunction = (x => columnId == 0 ? x.Nombre : columnId == 1 ? x.Clave : columnId == 2 ? x.HorasTeoricas : columnId == 3 ? x.TimeCreated.ToString() : x.Active.ToString());

                if (param.order[0].dir == "asc")
                {
                    subjects = subjects.OrderBy(orderFunction);
                }
                else
                {
                    subjects = subjects.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                subjects.Take(param.start).Skip(param.length);
                #endregion

                #region DT
                List<SubjectVM> data = subjects.Select(x => new SubjectVM
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Clave = x.Clave,
                    TPC = string.Concat(x.HorasTeoricas, "-", x.HorasPracticas, "-", x.Creditos),
                    Competencia = x.Competencia,
                    Caracterizacion = x.Caracterizacion,
                    IntencionDidactica = x.IntencionDidactica,
                    Active = x.Active,
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
    }
}