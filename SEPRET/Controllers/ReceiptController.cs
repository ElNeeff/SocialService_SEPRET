using Rotativa;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize]
    public class ReceiptController : Controller
    {
        // GET: Receipt
        public ActionResult Index()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                IEnumerable<Receipt> receipts = DBC.Receipts.Where(x => x.PersonId == UserId && x.Active == true).ToList();

                List<ReceiptVM> ReceiptList = receipts.OrderByDescending(x => x.TimeCreated).Select(x => new ReceiptVM
                {
                    Id = x.Id,
                    PaymentId = x.PaymentId,
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
                    RejectDescription = x.Rejections.Select(xx => xx.Reason.Description).LastOrDefault()
                }).ToList();

                return View(ReceiptList);
            }
        }

        [HttpPost]
        public ActionResult Voucher(long PaymentId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                string reference = string.Empty;

                Payment payment = DBC.Payments.FirstOrDefault(x => x.Id == PaymentId);
                Person person = DBC.People.FirstOrDefault(x => x.Id == UserId);

                string enrollment = person.Enrollment;
                if (enrollment.Length > 6)
                {
                    reference = string.Concat(enrollment[0], enrollment[1], enrollment[4], enrollment[5], enrollment[6], enrollment[7], payment.Id, "0", Convert.ToInt64(payment.Price));
                }
                else
                {
                    reference = enrollment;
                }

                ReceiptVM modelo = new ReceiptVM
                {
                    Enrollment = enrollment,
                    Career = person.Career.Name,
                    PersonName = string.Concat(person.Name, " ", person.MiddleName, " ", person.LastName),
                    PaymentId = payment.Id,
                    PaymentName = payment.Name,
                    PriceFormatted = payment.Price.ToString("C"),
                    Reference = reference
                };

                return new ViewAsPdf(modelo);
            }
        }

        [HttpPost]
        public ActionResult Index(ReceiptVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                string RelativePath, Destination, NombreArchivo, Folders, Ruta;
                RelativePath = Destination = NombreArchivo = Folders = Ruta = string.Empty;
                dynamic response;

                Receipt receipt = DBC.Receipts.Where(x => x.PersonId == UserId && x.PaymentId == modelo.PaymentId && x.Active == true).OrderByDescending(x => x.Id).FirstOrDefault();

                if (modelo.File != null && modelo.File.ContentType.Contains("image"))
                {
                    Folders = string.Concat("/Assets/img/receipts/", (long)Session["Id"], "/", modelo.PaymentId + "/");
                    Ruta = Server.MapPath(Folders);
                    NombreArchivo = Path.GetFileName(modelo.File.FileName);
                    RelativePath = Folders + NombreArchivo;
                    Destination = Ruta + NombreArchivo;
                    Directory.CreateDirectory(Ruta);
                    modelo.File.SaveAs(Destination);

                    response = "isImage";
                }
                else if (modelo.File != null)
                {
                    response = "notImage";
                }
                else
                {
                    response = "emptyFile";
                }

                if (response == "isImage")
                {
                    if (receipt == null || receipt.PhaseId == 4)
                    {
                        //INSERT
                        Receipt newReceipt = new Receipt
                        {
                            PersonId = (long)Session["Id"],
                            PaymentId = modelo.PaymentId,
                            PhaseId = 2,
                            Image = RelativePath,
                            Voucher = modelo.Voucher,
                            MethodId = modelo.MethodId,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Receipts.Add(newReceipt);
                        DBC.SaveChanges();

                        long LastReceiptId = newReceipt.Id;
                        Attempt attempt = new Attempt
                        {
                            ReceiptId = LastReceiptId,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.Attempts.Add(attempt);
                        DBC.SaveChanges();

                        response = true;
                    }
                    else if (receipt.PhaseId == 1)
                    {
                        //UPDATE
                        Folders = string.Concat("/Assets/img/receipts/", modelo.Id, "/", modelo.PaymentId + "/");
                        receipt.MethodId = modelo.MethodId;
                        receipt.PhaseId = 2;
                        receipt.Voucher = modelo.Voucher;
                        receipt.Image = string.IsNullOrEmpty(RelativePath) ? receipt.Image : RelativePath;
                        receipt.TimeUpdated = DateTime.Now;

                        DBC.SaveChanges();

                        response = true;
                    }
                    else
                    {
                        //EN PROCESO
                        response = false;
                    }
                }

                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddEditReceipt(long PaymentId, long ReceiptId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                ReceiptVM modelo = new ReceiptVM();
                List<Method> methods = DBC.Methods.Where(x => x.Active == true).ToList();
                ViewBag.MethodList = new SelectList(methods, "Id", "Name");
                ViewBag.RelativePath = "/Assets/img/_BlankPicture.png";

                if (ReceiptId > 0)
                {
                    Receipt receipt = DBC.Receipts.FirstOrDefault(x => x.Id == ReceiptId);
                    modelo.Id = receipt.Id;
                    //modelo.PersonId = receipt.Person.Id;
                    modelo.PaymentId = receipt.Payment.Id;
                    modelo.MethodId = receipt.Method.Id;
                    //modelo.PhaseId = receipt.Phase.Id;
                    modelo.Voucher = receipt.Voucher;
                    modelo.Image = receipt.Image;
                    ViewBag.RelativePath = receipt.Image;
                }
                else if (PaymentId > 0)
                {
                    modelo.PaymentId = PaymentId;
                }

                return PartialView("~/Views/Receipt/_AddEditReceipt.cshtml", modelo);
            }
        }

        [HttpPost]
        public ActionResult ReceiptList(string Keyword)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                IEnumerable<Receipt> receipts = DBC.Receipts.Where(x => x.PersonId == UserId && x.Active == true).ToList();

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

                List<ReceiptVM> ReceiptList = receipts.OrderByDescending(x => x.TimeCreated).Select(x => new ReceiptVM
                {
                    Id = x.Id,
                    PaymentId = x.PaymentId,
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
                    RejectDescription = x.Rejections.Select(xx => xx.Reason.Description).LastOrDefault()
                }).ToList();

                return PartialView("~/Views/Receipt/_SearchReceipt.cshtml", ReceiptList);
            }
        }

        [HttpPost]
        public ActionResult PaymentList(string Filter, string Keyword)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                IEnumerable<Payment> payments = DBC.Payments.Where(x => x.Active == true).OrderByDescending(x => x.Highlight);

                #region Búsqueda
                if (!string.IsNullOrEmpty(Keyword))
                {
                    Keyword = Keyword.ToLower();

                    payments = payments.Where(x => x.Name.ToLower().Contains(Keyword) ||
                    x.Price.ToString().Contains(Keyword) ||
                    x.Account.ToLower().Contains(Keyword)
                    );
                }
                #endregion

                #region Lista
                List<PaymentVM> list = payments.OrderByDescending(x => x.Highlight).Select(x => new PaymentVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    PriceFormatted = x.Price.ToString("C"),
                    Account = x.Account
                }).ToList();
                #endregion

                return PartialView("~/Views/Receipt/_SearchPayment.cshtml", list);
            }
        }
    }
}