using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.listaCarreras = new SelectList(careers, "Id", "Name");

                PersonVM modelo = new PersonVM();
                if (Session["Id"] != null)
                {
                    long UserId = (long)Session["Id"];
                    Person person = DBC.People.FirstOrDefault(x => x.Id == UserId);
                    modelo.Id = person.Id;
                    modelo.Enrollment = person.Enrollment;
                    modelo.UserFullName = string.Concat(person.Name, " ", person.MiddleName, " ", person.LastName);
                    modelo.Photo = person.Photo is null ? "/SEPRET/Assets/img/_BlankPicture.png" : person.Photo;
                    modelo.Phone = person.Phone;
                    modelo.Email = person.Email;
                    modelo.CareerId = person.CareerId;
                }

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult Index(PersonVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                string RelativePath, Destination;
                RelativePath = Destination = null;

                if (modelo.File != null && modelo.File.ContentType.Contains("image/"))
                {
                    string folders = string.Concat("/SEPRET/Assets/img/user/", Session["Id"], "/profile/");
                    string ruta = Server.MapPath(folders);
                    string filename = modelo.File.FileName;
                    Destination = string.Concat(ruta, filename);
                    RelativePath = string.Concat(folders, filename);
                    Directory.CreateDirectory(ruta);
                    //modelo.File.SaveAs(Destination);
                    modelo.File.SaveAs(Path.Combine(ruta, filename));
                }

                Person person = DBC.People.FirstOrDefault(x => x.Id == modelo.Id);
                person.Photo = string.IsNullOrEmpty(person.Photo) && RelativePath is null ? "/Assets/img/user/emptyAvatar.png" :  RelativePath is null ? person.Photo : RelativePath;
                person.Phone = modelo.Phone;
                person.Email = modelo.Email;
                person.CareerId = modelo.CareerId;
                person.TimeUpdated = DateTime.Now;

                Session["Photo"] = person.Photo;

                DBC.SaveChanges();

                return RedirectToAction("Index", "Profile", modelo);
            }
        }
    }
}