using Rotativa;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class PDFGeneratorController : Controller
    {
        public ActionResult EvaluacionRP()
        {
            //return View();
            return new ViewAsPdf();
        }

        public ActionResult SolicitudRP()
        {
            return new ViewAsPdf();
            //return View();
        }

        [HttpPost]
        public ActionResult AsignacionAsesor(long ProjectId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                ProjectPerson projectPerson = DBC.ProjectPersons.FirstOrDefault(x => x.Id_Project == ProjectId);
                Person person = DBC.People.FirstOrDefault(x => x.Id == UserId);

                PDFGeneratorVM modelo = new PDFGeneratorVM
                {
                    Asesor = DBC.Advisers.Where(x => x.Id_Project == ProjectId).Select(x => string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName)).FirstOrDefault(),
                    Fecha = string.Concat(DateTime.Now.Day, " ", DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("es")), ", ", DateTime.Now.Year),
                    Residentes = DBC.ProjectPersons.Where(x => x.Id_Project == ProjectId).Select(x => new PersonVM
                    {
                        Enrollment = x.Person.Enrollment,
                        UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        CareerName = x.Person.Career.Name
                    }).ToList(),
                    Proyect = DBC.Projects.Where(x => x.Id == ProjectId).Select(x => new ProjectVM
                    {
                        Titulo = x.Titulo
                    }).FirstOrDefault(),
                    Periodo = DateTime.Now.Month <= 6 ? "ENERO - JUNIO" : "JULIO - DICIEMBRE",
                    Empresa = DBC.ProjectCompanies.Where(x => x.Id_Project == ProjectId).Select(x => new CompanyVM {
                        Nombre = x.Company.Nombre
                    }).FirstOrDefault(),
                    JefedeDepartamento = DBC.PersonPermissions.Where(x => x.Id == UserId).Select(x => new PersonVM
                    {
                        UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        PermissionName = x.Permission.Name,
                        Career = x.Person.Career.Name
                    }).FirstOrDefault()
                };

                return new ViewAsPdf(modelo);
            }
        }
        [HttpPost]
        public ActionResult CartaPresentacion(long ProjectId)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];

                ProjectPerson projectPerson = DBC.ProjectPersons.FirstOrDefault(x => x.Id_Project == ProjectId);
                Person person = DBC.People.FirstOrDefault(x => x.Id == UserId);

                //ReceiptVM modelo = new ReceiptVM
                //{
                //    Enrollment = enrollment,
                //    Career = person.Career.Name,
                //    PersonName = string.Concat(person.Name, " ", person.MiddleName, " ", person.LastName),
                //    PaymentId = payment.Id,
                //    PaymentName = payment.Name,
                //    PriceFormatted = payment.Price.ToString("C"),
                //    Reference = reference
                //};

                return new ViewAsPdf();
            }
        }
    }
}