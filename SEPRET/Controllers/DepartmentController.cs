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
    public class DepartmentController : Controller
    {
        // GET: Department
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(DepartmentVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Department Department = DBC.Departments.FirstOrDefault(x => x.Id == modelo.Id);

                    Department.Name = modelo.Name;
                    Department.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Department Department = new Department
                    {
                        Name = modelo.Name,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Departments.Add(Department);
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
                DepartmentVM modelo = new DepartmentVM();

                if (Id > 0)
                {
                    Department Department = DBC.Departments.FirstOrDefault(x => x.Id == Id);

                    modelo.Id = Department.Id;
                    modelo.Name = Department.Name;
                }

                return PartialView("~/Views/Department/_AddEdit.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdateStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Department Department = DBC.Departments.FirstOrDefault(x => x.Id == Id);

                if (Department.Active)
                {
                    Department.Active = false;
                }
                else
                {
                    Department.Active = true;
                }

                Department.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Department> Department = Enumerable.Empty<Department>();

                switch (Filter)
                {
                    case "Active":
                        Department = DBC.Departments.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        Department = DBC.Departments.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                long total = Department.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    Department = Department.Where(x => x.Name.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword));
                }

                long totalFiltered = Department.Count();
                #endregion

                #region Ordenamiento
                int columnId = param.order[0].column;

                Func<Department, string> orderFunction = (x => columnId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    Department = Department.OrderBy(orderFunction);
                }
                else
                {
                    Department = Department.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                Department.Skip(param.start).Take(param.length);
                #endregion

                #region DataTable
                List<DepartmentVM> data = Department.Select(x => new DepartmentVM
                {
                    Id = x.Id,
                    Name = x.Name,
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