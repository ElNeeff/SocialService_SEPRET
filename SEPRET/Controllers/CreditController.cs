using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class CreditController : Controller
    {
        // GET: Credit
        public ActionResult Index()
        {
            SEPRETEntities DBC = new SEPRETEntities();
            long UserId = (long)Session["Id"];
            IEnumerable<CreditReceipt> creditReceipts = DBC.CreditReceipts.Where(x => x.Active == true && x.PersonId == UserId);
            List<CreditRecieptVM> listCRVM = creditReceipts.OrderByDescending(x => x.TimeCreated).Select(x => new CreditRecieptVM
            {
                Id = x.Id,
                PersonId = x.PersonId,
                PersonName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                Email = x.Person.Email,
                Enrollment = x.Person.Enrollment,
                Career = x.Person.Career.Name,
                CreditId = x.CreditId,
                Credit = x.Credit.TypeCredit,
                PhaseId = x.PhaseId,
                URL_PDF = x.URL_PDF,
                Active = x.Active,
                TimeCreated = x.TimeCreated
            }).ToList();
            return View(listCRVM);
        }

        [HttpPost]
        public ActionResult Index(CreditRecieptVM model)//Con este Index POST es para cuendo se quiere registra un nuevo credito o eeditar informacion de uno 
        {
            using(SEPRETEntities DBC =new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                string RelativePath, Destination,FileName, Folders, Ruta;
                RelativePath = Destination = FileName = Folders = Ruta = string.Empty;
                dynamic response;


                string message = "";
               
                if (model.File !=null &&string.Equals(Path.GetExtension(model.File.FileName), ".pdf"))//Si el archivo no esta vacio y ademas es un pdf
                {
                    //Se consulta el numero de control del alumno que subira el credito para crear la ruta
                    CreditReceipt enrollment = DBC.CreditReceipts.FirstOrDefault(y => y.PersonId == UserId);
                    //Creaccion de la ruta donde se guardara el archivo
                    Folders = string.Concat("/Assets/Credit/", enrollment.Person.Enrollment, "/", model.CreditId, "/");
                    Ruta = Server.MapPath(string.Concat("~", Folders));
                    FileName = Path.GetFileName(model.File.FileName);
                    RelativePath = Folders + FileName;
                    Destination = Ruta + FileName;
                    Directory.CreateDirectory(Ruta);
                    model.File.SaveAs(Destination);

                    if (model.Id > 0)
                    {
                        CreditReceipt editCreditReciept = DBC.CreditReceipts.FirstOrDefault(x => x.Id == model.Id);
                        //Edito los datos deel credito que se volvera a enviar
                        editCreditReciept.PhaseId = 1;
                        editCreditReciept.URL_PDF = RelativePath;
                        editCreditReciept.TimeUpdated = DateTime.Now;
                        message = "El credito fue reenviado con exito.";
                    }
                    else
                    {
                        CreditReceipt creditReceipt = new CreditReceipt();


                        creditReceipt.PersonId = UserId;
                        creditReceipt.CreditId = model.CreditId;
                        creditReceipt.PhaseId = 1;


                        creditReceipt.URL_PDF = RelativePath;
                        creditReceipt.TimeCreated = DateTime.Now;
                        creditReceipt.Active = true;

                        //Agrego a la base de datos el nuevo registro
                        DBC.CreditReceipts.Add(creditReceipt);
                        message = "El credito fue registrado de forma exitosa.";
                    }


                    //Se guardan los cambios
                    DBC.SaveChanges();


                    
                }
                else if(model.File==null)//Si no se subio un archivo
                {
                    message = "Debes de subir un archivo.";

                }
                else if (!string.Equals(Path.GetExtension(model.File.FileName), ".pdf"))//Si el archivo elegido no es un pdf
                {
                    message = "El archivo no es un PDF, asegurate de haberlo subido correctamente.";
                }

                
                response = message;

                return Json(response, JsonRequestBehavior.AllowGet);

            }


        }


        [HttpPost]
        public ActionResult CreditList(string Keyword)//Lista de los tipos dde creditos qque puede realizar el alumno
        {
            SEPRETEntities DBC = new SEPRETEntities();
            IEnumerable<Credit> credits = null;

            if(string.Equals(Keyword, "NoCredit"))
            {
                credits = DBC.Credits.Where(x => x.Active == true);
            }
            else
            {
                Keyword = Keyword.ToLower();
                credits = DBC.Credits.Where(x => x.Active == true && x.TypeCredit.ToLower().Contains(Keyword));
            }

            List<CreditVM> listcredits = credits.Select(x => new CreditVM
            {
                Id=x.Id,
                TypeCredit=x.TypeCredit
            }).ToList();


            return PartialView("~/Views/Credit/_CreditList.cshtml", listcredits);
        }

        [HttpPost]
        public ActionResult CreditRecieptList(String Keyword)//Lista de los creditos realizados por eel alumno
        {
            SEPRETEntities DBC = new SEPRETEntities();
            long UserId = (long)Session["Id"];

            IEnumerable<CreditReceipt> creditReceipts = null;
            if (string.Equals(Keyword, "NoCredit"))
            {//Este if es para cuando no se esta realizando una busqueda
               creditReceipts = DBC.CreditReceipts.Where(x => x.Active == true && x.PersonId == UserId);
            }
            else//Else para cuanddo se esta realizando una busqueda
            {
                Keyword = Keyword.ToLower();
                creditReceipts = DBC.CreditReceipts.Where(x => x.Active == true && x.PersonId == UserId 
                && x.Credit.TypeCredit.ToLower().Contains(Keyword)||
                x.Person.Email.ToLower().Contains(Keyword)||
                x.Person.Name.ToLower().Contains(Keyword) || 
                x.Person.MiddleName.ToLower().Contains(Keyword)||
                x.Person.LastName.ToLower().Contains(Keyword)||
                x.Phase.Name.ToLower().Contains(Keyword)||
                x.TimeCreated.ToString().ToLower().Contains(Keyword)||
                x.Person.Career.Name.ToLower().Contains(Keyword)
                ); 
            }
            //Creacion del modelo
            List<CreditRecieptVM> listCRVM = creditReceipts.OrderByDescending(x=>x.TimeCreated).Select(x => new CreditRecieptVM
            {
                Id = x.Id,
                PersonId = x.PersonId,
                PersonName=string.Concat(x.Person.Name," ",x.Person.MiddleName," ",x.Person.LastName),
                Email=x.Person.Email,
                Enrollment=x.Person.Enrollment,
                Career=x.Person.Career.Name,
                CreditId = x.CreditId,
                Credit=x.Credit.TypeCredit,
                PhaseId = x.PhaseId,
                URL_PDF = x.URL_PDF,
                Active=x.Active,
                TimeCreated=x.TimeCreated
            }).ToList();
            //Return de la partial view con el modelo creado
            return PartialView("~/Views/Credit/_CreditRecieptList.cshtml", listCRVM);
        }

        [HttpPost]
        public ActionResult AddEditCreditReceipt(long CreditID, long CreditReceiptID)
        {
            ViewBag.RelativePath = Request.Url.AbsolutePath.Contains("SEPRET") ? "/SEPRET/Assets/img/_BlankPicture.png" : "/Assets/img/_BlankPicture.png";
            CreditRecieptVM model = new CreditRecieptVM();

            if (CreditReceiptID != 0)
            {//Este if es para cuando se quiere volver a envviar un credito que fuee rechazado
                SEPRETEntities DBC = new SEPRETEntities();
                CreditReceipt credit = DBC.CreditReceipts.FirstOrDefault(x => x.Id == CreditReceiptID);

                model.CreditId = CreditID;
                model.Id = CreditReceiptID;
                model.PersonName = string.Concat(credit.Person.Name, " ", credit.Person.MiddleName, " ", credit.Person.LastName);
                ViewBag.MessageEdit = "Sube una vez más el archivo con el credito.";
                ViewBag.FormTittle = "Editar Credito";
            }
            else
            {//El else es para cuando se crea un nuevo credito
                model.CreditId = CreditID;
                ViewBag.FormTittle = "Registrar Nuevo Credito";
            }

            return PartialView("~/Views/Credit/_AddEditCreditReceipt.cshtml", model);
        }
    }
}