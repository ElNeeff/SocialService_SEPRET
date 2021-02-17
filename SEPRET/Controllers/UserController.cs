using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador, Administrador")]
    public class UserController : Controller
    {
        // GET: Users
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(PersonVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    // U P D A T E
                    PersonPermission user = DBC.PersonPermissions.FirstOrDefault(x => x.PersonId == modelo.Id);
                    user.Person.CareerId = modelo.CareerId;
                    user.PermissionId = modelo.PermissionID;
                    user.Person.Name = modelo.Name.ToTitleCase();
                    user.Person.MiddleName = modelo.MiddleName.ToTitleCase();
                    user.Person.LastName = modelo.LastName.ToTitleCase();
                    user.Person.Email = modelo.Email;
                    user.Person.Password = modelo.Password;
                    user.Person.TimeUpdated = DateTime.Now;
                    user.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    // I N S E R T
                    Person user = new Person
                    {
                        CareerId = modelo.CareerId,
                        Enrollment = string.Concat(DateTime.Now.ToString("yy"), 66, DateTime.Now.ToString("ffff")),
                        Name = modelo.Name.ToTitleCase(),
                        MiddleName = modelo.MiddleName.ToTitleCase(),
                        LastName = modelo.LastName.ToTitleCase(),
                        Email = modelo.Email,
                        Password = modelo.Password,
                        LastLogin = DateTime.Now,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.People.Add(user);
                    DBC.SaveChanges();

                    long LastUserID = user.Id;

                    PersonPermission personPermission = new PersonPermission
                    {
                        PersonId = LastUserID,
                        PermissionId = modelo.PermissionID,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.PersonPermissions.Add(personPermission);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditUser(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.CareerList = new SelectList(careers, "Id", "Name");
                List<Permission> permissions = DBC.Permissions.Where(x => x.Active == true).ToList();
                ViewBag.PermissionList = new SelectList(permissions, "Id", "Name");

                PersonVM modelo = new PersonVM();
                if (Id > 0)
                {
                    PersonPermission user = DBC.PersonPermissions.FirstOrDefault(x => x.Person.Id == Id);
                    modelo.CareerId = user.Person.CareerId;
                    modelo.PermissionID = user.PermissionId;
                    modelo.Id = user.Person.Id;
                    modelo.Name = user.Person.Name;
                    modelo.MiddleName = user.Person.MiddleName;
                    modelo.LastName = user.Person.LastName;
                    modelo.Email = user.Person.Email;
                    modelo.Password = user.Person.Password;
                }

                return PartialView("~/Views/User/_AddEditUser.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateUserStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Person user = DBC.People.FirstOrDefault(x => x.Id == Id);
                bool success = false;

                if (user.Active)
                {
                    user.Active = false;
                }
                else
                {
                    user.Active = true;
                }

                user.TimeUpdated = DateTime.Now;

                int updated = DBC.SaveChanges();
                if (updated > 0)
                {
                    success = true;
                }

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UserDT(DataTablesParameters DTParam, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<PersonPermission> Users = Enumerable.Empty<PersonPermission>();
                switch (Filter)
                {
                    case "Active":
                        Users = DBC.PersonPermissions.Where(x => x.Person.Active == true && x.PermissionId != 4).ToList();
                        break;
                    case "Inactive":
                        Users = DBC.PersonPermissions.Where(x => x.Person.Active == false).ToList();
                        break;
                    case "Student":
                        Users = DBC.PersonPermissions.Where(x => x.Person.Active == true && x.PermissionId == 4).ToList();
                        break;
                    default:
                        break;
                }

                var totalCount = Users.Count();

                #region Filtering
                // Apply filters for searching
                var searchBy = DTParam.search.value;

                if (!string.IsNullOrEmpty(searchBy))
                {
                    searchBy = searchBy.ToLower();

                    Users = Users.Where(x => x.Person.Name.ToLower().Contains(searchBy) ||
                                             x.Person.Enrollment.ToLower().Contains(searchBy) ||
                                             x.Person.MiddleName.ToLower().Contains(searchBy) ||
                                             x.Person.LastName.ToLower().Contains(searchBy) ||
                                             x.Person.Email.ToLower().Contains(searchBy) ||
                                             x.Person.Career.Name.ToLower().Contains(searchBy) ||
                                             x.Permission.Name.ToLower().Contains(searchBy)
                                             );
                }

                var filteredCount = Users.Count();

                #endregion Filtering

                #region Sorting
                // Sorting
                int colId = DTParam.order[0].column;
                Func<PersonPermission, string> orderingFunction = (x => colId == 0 ? x.Person.Enrollment :
                                      colId == 1 ? x.Person.Name :
                                      colId == 2 ? x.Person.Career.Name:
                                      colId == 3 ? x.Permission.Name :
                                      colId == 4 ? x.Person.Email :
                                      x.Person.LastLogin.ToString());

                if (DTParam.order[0].dir == "asc")
                {
                    Users = Users.OrderBy(orderingFunction);
                }
                else
                {
                    Users = Users.OrderByDescending(orderingFunction);
                }

                #endregion Sorting

                // Paging
                Users = Users.Skip(DTParam.start).Take(DTParam.length);

                List<PersonVM> data = Users.Select(x => new PersonVM
                {
                    Id = x.Person.Id,
                    Enrollment = x.Person.Enrollment,
                    UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                    PermissionName = x.Permission.Name,
                    CareerName = x.Person.Career.Name,
                    Email = x.Person.Email,
                    LastLoginFormatted = x.Person.LastLogin.ToString(),
                    Active = x.Person.Active
                }).ToList();

                return Json(new
                {
                    aaData = data,
                    DTParam.draw,
                    iTotalDisplayRecords = filteredCount,
                    iTotalRecords = totalCount
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}