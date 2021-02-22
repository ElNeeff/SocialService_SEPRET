using System;
using System.Linq;
using System.Web.Mvc;
using SEPRET.Models;
using SEPRET.Models.Custom;

namespace SEPRET.Controllers
{
    public class PaymentDBController : Controller
    {
        // GET: PaymentDB
        public ActionResult Index()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                DateTime start = new DateTime();
                DateTime end = new DateTime();

                if (DateTime.Now.Month <= 6)
                {
                    start = new DateTime(DateTime.Now.Year, 1, 1);
                    end = new DateTime(DateTime.Now.Year, 6, 30);
                }
                else
                {
                    start = new DateTime(DateTime.Now.Year, 7, 1);
                    end = new DateTime(DateTime.Now.Year, 12, 31);
                }

                PaymentDBVM paymentDBVM = new PaymentDBVM
                {
                    Rejected = DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Pending = DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Accepted = DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Invoiced = DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Total = DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Progress = DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count() != 0 ? Math.Round(DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end && x.PhaseId == 4).Count() / (decimal)DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count() * 100, 2) : 0,
                    RejectedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Rechazados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    PendingGeneral = new PaymentDBGeneral
                    {
                        Phase = "Pendientes",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    AcceptedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Aceptados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    InvoicedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Facturados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    }
                };

                return View(paymentDBVM);
            }
        }

        [HttpPost]
        public JsonResult Filter(string startDate, string endDate)
        {

            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                DateTime start = Convert.ToDateTime(startDate);
                DateTime end = Convert.ToDateTime(endDate);

                PaymentDBVM paymentDBVM = new PaymentDBVM
                {
                    Rejected = DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Pending = DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Accepted = DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Invoiced = DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Total = DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                    Progress = DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count() != 0 ? Math.Round(DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end && x.PhaseId == 4).Count() / (decimal)DBC.Receipts.Where(x => x.Active && x.TimeCreated >= start && x.TimeCreated <= end).Count() * 100, 2) : 0,
                    RejectedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Rechazados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 1 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    PendingGeneral = new PaymentDBGeneral
                    {
                        Phase = "Pendientes",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 2 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    AcceptedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Aceptados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 3 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    },
                    InvoicedGeneral = new PaymentDBGeneral
                    {
                        Phase = "Facturados",
                        Data = new long[]
                        {
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 2 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 3 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 5 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 6 && x.TimeCreated >= start && x.TimeCreated <= end).Count(),
                            DBC.Receipts.Where(x => x.Active && x.PhaseId == 4 && x.Person.CareerId == 8 && x.TimeCreated >= start && x.TimeCreated <= end).Count()
                        }
                    }
                };

                return Json(paymentDBVM, JsonRequestBehavior.AllowGet);
            }
        }
    }
}