using Microsoft.Ajax.Utilities;
using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador")]
    public class PermissionController : Controller
    {
        // GET: Permissions
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(PermissionVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    Permission permission = DBC.Permissions.FirstOrDefault(x => x.Id == modelo.Id);
                    permission.Name = modelo.Name;
                    permission.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }
                else
                {
                    Permission permission = new Permission
                    {
                        Name = modelo.Name,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Permissions.Add(permission);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditPermission(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                PermissionVM modelo = new PermissionVM();

                if (Id > 0)
                {
                    Permission permission = DBC.Permissions.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = permission.Id;
                    modelo.Name = permission.Name;
                }

                return PartialView("~/Views/Permission/_AddEditPermission.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdatePermissionStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Permission permission = DBC.Permissions.FirstOrDefault(x => x.Id == Id);

                if (permission.Active)
                {
                    permission.Active = false;
                }
                else
                {
                    permission.Active = true;
                }

                permission.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        #region DataTable
        [HttpPost]
        public JsonResult PermissionDT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Permission> permissions = Enumerable.Empty<Permission>();

                switch (Filter)
                {
                    case "Active":
                        permissions = DBC.Permissions.Where(x => x.Active == true).ToList();
                        break;
                    case "Inactive":
                        permissions = DBC.Permissions.Where(x => x.Active == false).ToList();
                        break;
                    default:
                        break;
                }

                int total = permissions.Count();

                #region Búsqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    permissions = permissions.Where(x => x.Name.Contains(keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(keyword)
                    );
                }
                int totalFiltrado = permissions.Count();
                #endregion

                #region Ordenamiento
                int colId = param.order[0].column;
                Func<Permission, string> orderFunction = (x => colId == 0 ? x.Name : x.TimeCreated.ToString());

                if (param.order[0].dir == "asc")
                {
                    permissions = permissions.OrderBy(orderFunction);
                }
                else
                {
                    permissions = permissions.OrderByDescending(orderFunction);
                }
                #endregion

                #region Paginado
                permissions.Skip(param.start).Skip(param.length);
                #endregion

                #region DataTable
                List<PermissionVM> data = permissions.Select(x => new PermissionVM
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
                    iTotalDisplayRecords = totalFiltrado
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}