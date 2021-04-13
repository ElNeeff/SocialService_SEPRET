using Microsoft.Ajax.Utilities;
using Rotativa;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Policy;
using System.Web;
using System.Web.Management;
using System.Web.ModelBinding;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        [Authorize(Roles = "Super Administrador, Administrador, Financiero")]
        public ActionResult Index()
        {
            using (SEPRETEntities db = new SEPRETEntities())
            {
                IEnumerable<Receipt> receipts = db.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.Active == true && x.PhaseId == 2).Take(12).ToList();
                long total = receipts.Count();

                if (total > 0)
                {
                    List<ReceiptVM> binnacle = receipts.Select(x => new ReceiptVM
                    {
                        Id = x.Id,
                        Enrollment = x.Person.Enrollment,
                        Career = x.Person.Career.Name,
                        PersonName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        Email = x.Person.Email,
                        PaymentName = x.Payment.Name,
                        MethodName = x.Method.Name,
                        Voucher = x.Voucher,
                        Image = x.Image,
                        PhaseId = x.Phase.Id,
                        PriceFormatted = x.Payment.Price.ToString("C"),
                        TimeCreatedFormatted = x.TimeCreated.ToString()
                    }).ToList();

                    ViewBag.Binnacle = binnacle;
                }
                else
                {
                    ViewBag.Binnacle = null;
                }

                ViewBag.BinnacleTotal = total;

                return View();
            }
        }

        [HttpGet]
        public JsonResult ReceiptCounter()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ReceiptCounter receiptCounter = new ReceiptCounter
                {
                    Rejected = DBC.Receipts.Count(x => x.PhaseId == 1 && x.Active),
                    Pending = DBC.Receipts.Count(x => x.PhaseId == 2 && x.Active),
                    Accepted = DBC.Receipts.Count(x => x.PhaseId == 3 && x.Active),
                    Invoiced = DBC.Receipts.Count(x => x.PhaseId == 4 && x.Active),
                    Deleted = DBC.Receipts.Count(x => !x.Active),
                    Total = DBC.Receipts.Count(x => x.Active)
                };

                return Json(receiptCounter, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Paginate(string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Receipt> receipts = DBC.Receipts.Where(x => x.Active == true).ToList();

                #region Filtro
                switch (Filter)
                {
                    case "Pending":
                        receipts = DBC.Receipts.Where(x => x.PhaseId == 2 && x.Active == true).ToList();
                        break;
                    case "Accepted":
                        receipts = DBC.Receipts.Where(x => x.PhaseId == 3 && x.Active == true).ToList();
                        break;
                    case "Rejected":
                        receipts = DBC.Receipts.Where(x => x.PhaseId == 1 && x.Active == true).ToList();
                        break;
                    case "Deleted":
                        receipts = DBC.Receipts.Where(x => x.Active == false).ToList();
                        break;
                    case "Finished":
                        receipts = DBC.Receipts.Where(x => x.PhaseId == 4 && x.Active == true).ToList();
                        break;
                    default:
                        break;
                }
                #endregion
                long? TotalRegisters = receipts.Count();

                return PartialView("~/Views/Dashboard/_Paginacion.cshtml", TotalRegisters);
            }
        }

        [HttpPost]
        public ActionResult CorreoIns(long Idusu)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {



                Person person = DBC.People.FirstOrDefault(x => x.Id == Idusu);

                string enrollment = person.Enrollment;

                string emailIns = string.Concat("L", enrollment, "@matehuala.tecnm.mx");



                PersonVM modelo = new PersonVM
                {
                    Email = emailIns,
                    UserFullName = string.Concat(person.Name, " ", person.MiddleName, " ", person.LastName),

                };

                return new ViewAsPdf(modelo);
            }
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            if (model != null)
                ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }


        [HttpPost]
        public JsonResult SendMail(long ReceiptId, string Type, bool RequierePDF)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == ReceiptId);
                string correoalumno = receipt.Person.Email;

                ReceiptVM modelo = new ReceiptVM
                {
                    Enrollment = receipt.Person.Enrollment,
                    PersonName = string.Concat(receipt.Person.Name, " ", receipt.Person.MiddleName, " ", receipt.Person.LastName),
                    PaymentName = receipt.Payment.Name,
                    Email = receipt.Person.Email,
                    RejectDescription = receipt.Rejections.Select(x => x.Reason.Description).LastOrDefault()
                };

                string renderedHTML = string.Empty;
                string subject = string.Empty;
                string coordinatorEmail = string.Empty;
                switch (Type)
                {
                    case "Accepted":
                    case "Invoiced":
                        subject = "Pago aceptado";
                        renderedHTML = RenderPartialViewToString("EmailReceiptAccepted", modelo);
                        break;
                    case "Rejected":
                        subject = "Pago rechazado";
                        renderedHTML = RenderPartialViewToString("EmailReceiptRejected", modelo);
                        break;
                    default:
                        break;
                }

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp-mail.outlook.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("itmh@matehuala.tecnm.mx", "Mypassw0rd");

                string body = renderedHTML;
                using (var message = new MailMessage("itmh@matehuala.tecnm.mx", receipt.Person.Email))
                {
                    if (Type == "Invoiced")
                    {
                        switch (receipt.Payment.Name.Trim().ToLower())
                        {
                            case "inscripción extemporánea":
                            case "inscripción nuevo ingreso":
                            case "reinscripcion":
                            case "tramite de titulo y acto recepcional":
                                switch (receipt.Person.CareerId)
                                {
                                    case 3:
                                        coordinatorEmail = "coor_ige@matehuala.tecnm.mx";
                                        break;
                                    case 8:
                                        coordinatorEmail = "coor_contador@matehuala.tecnm.mx";
                                        break;
                                    case 2:
                                        coordinatorEmail = "coor_civil@matehuala.tecnm.mx";
                                        break;
                                    case 5:
                                        coordinatorEmail = "coor_industrial@matehuala.tecnm.mx";
                                        break;
                                    case 6:
                                        coordinatorEmail = "coor_sist@matehuala.tecnm.mx";
                                        break;
                                    default:
                                        break;
                                }

                                MailAddress division = new MailAddress("division@matehuala.tecnm.mx");
                                message.CC.Add(division);
                                MailAddress coordinador = new MailAddress(coordinatorEmail);
                                message.CC.Add(coordinador);
                                break;
                            case "constancia de estudios":
                            case "preinscripcion":
                            case "certificado parcial de estudios":
                            case "certificado final de estudios":
                            case "duplicado":
                                MailAddress escolares = new MailAddress("escolares@matehuala.tecnm.mx");
                                message.CC.Add(escolares);
                                break;
                            case "reposición de credencial":
                                MailAddress biblioteca = new MailAddress("biblioteca@matehuala.tecnm.mx");
                                message.CC.Add(biblioteca);
                                break;
                            case "curso de ingles":
                            case "examen de ubicación de certificación de ingles":
                            case "examen general de inglés":
                            case "examen ubicacion de ingles":
                                MailAddress ingles = new MailAddress("administrador_lingo@matehuala.tecnm.mx");
                                message.CC.Add(ingles);
                                break;
                            default:
                                break;
                        }
                    }

                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    if (RequierePDF)
                    {
                        var file = Server.MapPath(receipt.Invoice);
                        var imagepath = Server.MapPath(receipt.Image);
                        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                        message.Attachments.Add(data);
                        Attachment image = new Attachment(imagepath, MediaTypeNames.Application.Octet);
                        message.Attachments.Add(image);
                    }
                    smtp.Send(message);
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
        }

        #region Adjuntar factura
        [HttpPost]
        public JsonResult SaveInvoice(ReceiptVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                string RelativePath, Destination;
                RelativePath = Destination = string.Empty;

                if (modelo.Id > 0 && modelo.File.ContentType.Contains("application/pdf"))
                {
                    string Folders = string.Concat("/Assets/img/receipts/", modelo.PersonId, "/", modelo.PaymentId + "/");
                    var Ruta = Server.MapPath(string.Concat("~", Folders));
                    var NombreArchivo = Path.GetFileName(modelo.File.FileName);
                    Destination = string.Concat(Ruta, NombreArchivo);
                    RelativePath = string.Concat(Folders, NombreArchivo);
                    Directory.CreateDirectory(Ruta);
                    modelo.File.SaveAs(Destination);

                    Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == modelo.Id);
                    receipt.PhaseId = ++receipt.PhaseId;
                    receipt.Invoice = RelativePath is null ? receipt.Invoice : RelativePath;
                    receipt.InvoicedBy = (long)Session["Id"];
                    receipt.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddInvoice(long ReceiptId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ReceiptVM modelo = new ReceiptVM();

                if (ReceiptId > 0)
                {
                    Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == ReceiptId);
                    modelo.Id = receipt.Id;
                    modelo.PaymentId = receipt.PaymentId;
                    modelo.PersonId = receipt.PersonId;
                }

                return PartialView("~/Views/Dashboard/_AddInvoice.cshtml", modelo);
            }
        }
        #endregion

        #region Actualizar fase
        [HttpPost]
        public JsonResult DeleteReceipt(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (Id > 0)
                {
                    Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == Id);
                    receipt.Active = false;
                    receipt.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult AcceptReceipt(long ReceiptId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == ReceiptId);

                receipt.PhaseId = ++receipt.PhaseId;
                receipt.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RejectReceipt(ConfirmReceiptRejectVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                if (modelo.ReceiptId > 0)
                {
                    Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == modelo.ReceiptId);

                    if (!string.IsNullOrEmpty(modelo.Description))
                    {

                        Reason reason = new Reason
                        {
                            Description = modelo.Description,
                            Preset = false,
                            Active = true,
                            CreatedBy = (long)Session["Id"],
                            TimeCreated = DateTime.Now
                        };

                        DBC.Reasons.Add(reason);
                        DBC.SaveChanges();

                        long LastReasonId = reason.Id;

                        Rejection rejection = new Rejection
                        {
                            ReceiptId = receipt.Id,
                            ReasonId = LastReasonId,
                            Active = true,
                            CreatedBy = (long)Session["Id"],
                            TimeCreated = DateTime.Now
                        };

                        DBC.Rejections.Add(rejection);
                    }
                    else
                    {
                        Rejection rejection = new Rejection
                        {
                            ReceiptId = receipt.Id,
                            ReasonId = modelo.ReasonId,
                            Active = true,
                            CreatedBy = (long)Session["Id"],
                            TimeCreated = DateTime.Now
                        };

                        DBC.Rejections.Add(rejection);
                    }


                    Attempt attempt = new Attempt
                    {
                        ReceiptId = receipt.Id,
                        Active = true,
                        TimeCreated = DateTime.Now
                    };

                    DBC.Attempts.Add(attempt);

                    receipt.PhaseId = 1;
                    receipt.TimeUpdated = DateTime.Now;

                    DBC.SaveChanges();

                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ConfirmReceiptReject(long Id)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Reason> reasons = DBC.Reasons.Where(x => x.Active && x.Preset == true).ToList();
                ViewBag.ReasonList = new SelectList(reasons, "Id", "Description");

                ConfirmReceiptRejectVM modelo = new ConfirmReceiptRejectVM();

                if (Id > 0)
                {
                    modelo.ReceiptId = Id;
                }

                return PartialView("~/Views/Dashboard/_ConfirmReceiptReject.cshtml", modelo);
            }
        }
        #endregion

        #region Bitácora de pagos
        [HttpPost]
        public ActionResult ReceiptList(string Filter, string Keyword, int Skip = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Receipt> receipts = DBC.Receipts.Where(x => x.Active == true).ToList();

                #region Filtro
                switch (Filter)
                {
                    case "Pending":
                        receipts = DBC.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.PhaseId == 2 && x.Active == true).ToList();
                        break;
                    case "Accepted":
                        receipts = DBC.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.PhaseId == 3 && x.Active == true).ToList();
                        break;
                    case "Rejected":
                        receipts = DBC.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.PhaseId == 1 && x.Active == true).ToList();
                        break;
                    case "Deleted":
                        receipts = DBC.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.Active == false).ToList();
                        break;
                    case "Finished":
                        receipts = DBC.Receipts.OrderByDescending(x => x.TimeCreated).Where(x => x.PhaseId == 4 && x.Active == true).ToList();
                        break;
                    default:
                        break;
                }
                #endregion

                long TotalRecords = receipts.Count();

                #region Búsqueda
                if (!string.IsNullOrEmpty(Keyword))
                {
                    Keyword = Keyword.ToLower();

                    receipts = receipts.Where(x => x.Person.Name.ToLower().Contains(Keyword) ||
                    x.Person.MiddleName.ToLower().Contains(Keyword) ||
                    x.Person.LastName.ToLower().Contains(Keyword) ||
                    x.Person.Email.ToLower().Contains(Keyword) ||
                    x.Person.Enrollment.ToLower().Contains(Keyword) ||
                    x.Person.Career.Name.ToLower().Contains(Keyword) ||
                    x.Payment.Name.ToLower().Contains(Keyword) ||
                    x.Payment.Price.ToString().ToLower().Contains(Keyword) ||
                    x.Method.Name.ToLower().Contains(Keyword) ||
                    x.Phase.Name.ToLower().Contains(Keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(Keyword) ||
                    x.Voucher.ToString().ToLower().Contains(Keyword)
                    );
                }
                #endregion

                // PAGINADO
                int skip = 12 * Skip;
                receipts = receipts.Skip(skip).Take(12);

                #region Lista
                List<ReceiptVM> binnacle = receipts.Select(x => new ReceiptVM
                {
                    Id = x.Id,
                    Enrollment = x.Person.Enrollment,
                    Career = x.Person.Career.Name,
                    PersonName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                    Email = x.Person.Email,
                    PaymentName = x.Payment.Name,
                    MethodName = x.Method.Name,
                    Voucher = x.Voucher,
                    Image = x.Image,
                    PhaseId = x.Phase.Id,
                    PriceFormatted = x.Payment.Price.ToString("C"),
                    Active = x.Active,
                    TimeCreatedFormatted = x.TimeCreated.ToString(),
                    RejectDescription = x.Rejections.Select(xx => xx.Reason.Description).LastOrDefault(),
                    TotalRecords = TotalRecords
                }).ToList();
                #endregion

                return PartialView("~/Views/Dashboard/_SearchReceipt.cshtml", binnacle);
            }
        }
        #endregion
    }
}