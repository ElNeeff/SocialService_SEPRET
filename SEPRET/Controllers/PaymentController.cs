using Antlr.Runtime.Tree;
using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize(Roles = "Super Administrador, Administrador, Financiero")]
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }

        #region Conceptos
        [HttpPost]
        public ActionResult Index(PaymentVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.Id > 0)
                {
                    // U P D A T E
                    Payment payment = DBC.Payments.FirstOrDefault(x => x.Id == modelo.Id);
                    payment.Name = modelo.Name;
                    payment.Price = modelo.Price;
                    payment.Account = modelo.Account;
                    payment.TimeUpdated = DateTime.Now;
                    DBC.SaveChanges();
                }
                else
                {
                    // I N S E R T
                    Payment payment = new Payment
                    {
                        Name = modelo.Name,
                        Price = modelo.Price,
                        Account = modelo.Account,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Payments.Add(payment);
                    DBC.SaveChanges();
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEditPayment(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                PaymentVM modelo = new PaymentVM();

                if (Id > 0)
                {
                    Payment payment = DBC.Payments.FirstOrDefault(x => x.Id == Id);
                    modelo.Id = payment.Id;
                    modelo.Name = payment.Name;
                    modelo.Price = payment.Price;
                    modelo.Account = payment.Account;
                }

                return PartialView("~/Views/Payment/_AddEditPayment.cshtml", modelo);
            }
        }

        [HttpPost]
        public JsonResult UpdatePaymentStatus(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Payment payment = DBC.Payments.FirstOrDefault(x => x.Id == Id);
                bool success = false;

                if (payment.Active)
                {
                    payment.Active = false;
                }
                else
                {
                    payment.Active = true;
                }

                payment.TimeUpdated = DateTime.Now;

                int updated = DBC.SaveChanges();

                if (updated > 0)
                {
                    success = true;
                }

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult PaymentDT(DataTablesParameters param, string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                #region Listar
                IEnumerable<Payment> payments = Enumerable.Empty<Payment>();

                switch (Filter)
                {
                    case "Active":
                        payments = DBC.Payments.Where(x => x.Active == true);
                        break;
                    case "Inactive":
                        payments = DBC.Payments.Where(x => x.Active == false);
                        break;
                    default:
                        break;
                }

                long total = payments.Count();
                #endregion

                #region Busqueda
                string keyword = param.search.value;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();

                    payments = payments.Where(x => x.Name.ToLower().Contains(keyword) ||
                    x.Price.ToString().Contains(keyword) ||
                    x.Account.ToLower().ToLower().Contains(keyword) ||
                    x.TimeCreated.ToString().Contains(keyword)
                    );
                }

                long totalFiltrado = payments.Count();
                #endregion

                #region Ordenamiento
                int colId = param.order[0].column;

                Func<Payment, string> orderingFunction = (x => colId == 0 ? x.Name :
                colId == 1 ? x.Price.ToString() : x.Account
                );

                if (param.order[0].dir == "asc")
                {
                    payments = payments.OrderBy(orderingFunction);
                }
                else
                {
                    payments = payments.OrderByDescending(orderingFunction);
                }
                #endregion

                #region Paginado
                payments = payments.Skip(param.start).Take(param.length);
                #endregion

                #region ListaFinal
                List<PaymentVM> payment = payments.Select(x => new PaymentVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    Account = x.Account,
                    TimeCreatedFormatted = x.TimeCreated.ToString(),
                    Active = x.Active
                }).ToList();
                #endregion

                return Json(new
                {
                    aaData = payment,
                    param.draw,
                    iTotalDisplayRecords = totalFiltrado,
                    iTotalRecords = total
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}