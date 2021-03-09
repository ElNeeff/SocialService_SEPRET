using AdoNetCore.AseClient;
using SEPRET.CustomClasses;
using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;

namespace SEPRET.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.currentView = "Index";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Privacy()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SignIn()
        {
            ViewBag.currentView = "SignIn";
            return View();
        }

        public ActionResult FAQ()
        {
            ViewBag.currentView = "FAQ";
            return View();
        }

        public ActionResult PaswordRecovery()
        {
            return View();
        }
        

        public JsonResult sybase()
        {
            var connectionString = "Data Source=192.168.7.213;Port=5000;Database=bdtec;Uid=sa;";
            StudentSII[] allRecords = null;
            string careerName = "";

            using (var connection = new AseConnection(connectionString))
            {
                connection.Open();

                // use the connection...

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT c.carrera, c.nombre_carrera, a.nombre_alumno, a.apellido_paterno, a.apellido_materno FROM carreras AS c INNER JOIN alumnos AS a ON c.carrera = a.carrera WHERE a.no_de_control = '16660017'";

                    using (var reader = command.ExecuteReader())
                    {
                        var list = new List<StudentSII>();
                        // Get the results.
                        while (reader.Read())
                        {
                            careerName = reader.GetString(1);
                            // Do something with the data...
                            list.Add(new StudentSII { CareerId = reader.GetString(0), CareerName = reader.GetString(1), Name = reader.GetString(2), MiddleName = reader.GetString(3), LastName = reader.GetString(4) });
                            allRecords = list.ToArray();
                        }
                        return Json(allRecords, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            //using (SEPRETEntities DBC = new SEPRETEntities())
            //{
            //    Career career = DBC.Careers.FirstOrDefault(x => x.Name.ToLower() == careerName.ToLower());
            //    long id = career.Id;

            //    return Json(new { allRecords, id }, JsonRequestBehavior.AllowGet);
            //}
        }

        [HttpPost]
        public JsonResult PaswordRecovery(string Enrollment)
        {
            string resetCode = Guid.NewGuid().ToString();
            var verifyUrl = "/Home/ResetPassword/" + resetCode;
            //var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var link = string.Concat("http://matehuala.tecnm.mx:800/Home/ResetPassword/", resetCode);
            string Message = string.Empty;

            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                Person person = DBC.People.FirstOrDefault(x => x.Enrollment == Enrollment.Trim());
                if (person != null)
                {
                    person.ResetPasswordCode = resetCode;
                    DBC.Configuration.ValidateOnSaveEnabled = false;
                    DBC.SaveChanges();

                    ReceiptVM modelo = new ReceiptVM
                    {
                        Enrollment = person.Enrollment,
                        PersonName = string.Concat(person.Name, " ", person.MiddleName, " ", person.LastName),
                        Career = link
                    };

                    var renderedHTML = FakeController.RenderViewToString("EmailPasswordRecovery", "Home", modelo);

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp-mail.outlook.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("ITMH@matehuala.tecnm.mx", "Mypassw0rd");

                    string body = renderedHTML;

                    using (var message = new MailMessage("ITMH@matehuala.tecnm.mx", person.Email))
                    {
                        message.Subject = "Solicitud de nueva contraseña";
                        message.Body = body;
                        message.IsBodyHtml = true;
                        smtp.Send(message);

                        Message = string.Concat("El enlace para reestablecer tu contraseña ha sido enviado a tu correo electrónico: ", person.Email);
                    }
                }
                else
                {
                    Message = "Aún no has creado una cuenta para acceder al sitio.";
                }
            }

            return Json(Message, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                bool validCode = DBC.People.Any(x => x.ResetPasswordCode == id);
                if (validCode)
                {
                    PasswordRecoveryVM model = new PasswordRecoveryVM();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordRecoveryVM modelo)
        {
            if (ModelState.IsValid)
            {
                using (SEPRETEntities DBC = new SEPRETEntities())
                {
                    var user = DBC.People.FirstOrDefault(a => a.ResetPasswordCode == modelo.ResetCode);
                    if (user != null)
                    {
                        user.Password = modelo.NewPassword;
                        user.ResetPasswordCode = null;
                        user.TimeUpdated = DateTime.Now;
                        DBC.Configuration.ValidateOnSaveEnabled = false;
                        DBC.SaveChanges();

                        return RedirectToAction("SignIn", "Home");
                    }
                }
            }
            else
            {
                ViewBag.Message = "Ocurrió un error al procesar la solicitud";
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInVM modelo)
        {
            if (ModelState.IsValid)
            {
                using (SEPRETEntities DBC = new SEPRETEntities())
                {
                    var ValidUser = DBC.People.FirstOrDefault(x => x.Email.ToLower() == modelo.Email.ToLower() && x.Password == modelo.Password && x.Active);

                    if (ValidUser != null)
                    {
                        Session["Id"] = ValidUser.Id;
                        Session["Enrollment"] = ValidUser.Enrollment;
                        Session["CareerId"] = ValidUser.CareerId;
                        Session["DepartmentId"] = ValidUser.Career.Department.Id;
                        Session["UserFullName"] = string.Concat(ValidUser.Name, " ", ValidUser.MiddleName);
                        Session["Photo"] = string.IsNullOrEmpty(ValidUser.Photo) ? "/Assets/img/user/emptyAvatar.png" : ValidUser.Photo;

                        FormsAuthentication.SetAuthCookie(ValidUser.Email, false);

                        ValidUser.LastLogin = DateTime.Now;
                        DBC.SaveChanges();

                        return RedirectToAction("SignInRedirect");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Usuario ó contraseña inválido");
                    }
                }

            }
            else
            {
                ModelState.AddModelError(string.Empty, "Todos los campos son obligatorios");
            }

            return View(modelo);
        }

        public ActionResult SignUp()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.CareerList = new SelectList(careers, "Id", "Name");
                ViewBag.currentView = "SignUp";
                return View();

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(PersonVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                List<Career> careers = DBC.Careers.Where(x => x.Active == true).ToList();
                ViewBag.CareerList = new SelectList(careers, "Id", "Name");

                if (ModelState.IsValid)
                {
                    bool userExists = DBC.People.Any(x => x.Email == modelo.Email || x.Enrollment == modelo.Enrollment);
                    if (userExists)
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un usuario registrado con este correo electrónico ó número de control");
                    }
                    else
                    {
                        //var connectionString = "Data Source=192.168.7.11;Port=5000;Database=bdtec;Uid=sa;Pwd=Syba2018;";
                        //var nuevo199 no sirve = "Data Source=192.168.7.11;Port=5000;Database=bdtec;Uid=sa;Pwd=sybase08;";
                        //var ingles = "Data Source=192.168.7.213;Port=5000;Database=bdtec;Uid=sa;";
                        var competencias = "Data Source=192.168.7.209;Port=5000;Database=bdtec;Uid=sa;Pwd=Syba2018;";

                        string careerSII, name, middlename, lastname;
                        careerSII = name = middlename = lastname = string.Empty;

                        using (var connection = new AseConnection(competencias))
                        {
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = string.Concat("SELECT c.carrera, c.nombre_carrera, a.nombre_alumno, a.apellido_paterno, a.apellido_materno FROM carreras AS c INNER JOIN alumnos AS a ON c.carrera = a.carrera WHERE a.no_de_control =", "'", modelo.Enrollment, "'");

                                using (var reader = command.ExecuteReader())
                                {
                                    // Get the results.
                                    while (reader.Read())
                                    {
                                        careerSII = reader.GetString(0);
                                        name = reader.GetString(2);
                                        middlename = reader.GetString(3);
                                        lastname = reader.GetString(4);
                                    }
                                }
                            }
                            connection.Close();
                            if (string.IsNullOrEmpty(careerSII))
                            {
                                var ingles = "Data Source=192.168.7.213;Port=5000;Database=bdtec;Uid=sa;";

                                using (var dbc = new AseConnection(ingles))
                                {
                                    dbc.Open();
                                    using (var command = dbc.CreateCommand())
                                    {
                                        command.CommandText = string.Concat("SELECT c.carrera, c.nombre_carrera, a.nombre_alumno, a.apellido_paterno, a.apellido_materno FROM carreras AS c INNER JOIN alumnos AS a ON c.carrera = a.carrera WHERE a.no_de_control =", "'", modelo.Enrollment, "'");

                                        using (var reader = command.ExecuteReader())
                                        {
                                            // Get the results.
                                            while (reader.Read())
                                            {
                                                careerSII = reader.GetString(0);
                                                name = reader.GetString(2);
                                                middlename = reader.GetString(3);
                                                lastname = reader.GetString(4);
                                            }
                                        }
                                    }
                                    dbc.Close();
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(careerSII))
                        {
                            ModelState.AddModelError(string.Empty, "El número de control no existe en la base de datos.");
                        }
                        else
                        {
                            Career career = DBC.Careers.FirstOrDefault(x => x.CareerId.ToLower() == careerSII.ToLower());

                            if (career != null)
                            {
                                long careerId = career.Id;

                                Person person = new Person
                                {
                                    CareerId = careerId,
                                    Enrollment = modelo.Enrollment,
                                    Name = name.ToTitleCase(),
                                    MiddleName = middlename.ToTitleCase(),
                                    LastName = lastname.ToTitleCase(),
                                    Email = modelo.Email,
                                    Password = modelo.Password,
                                    LastLogin = DateTime.Now,
                                    Active = true,
                                    TimeCreated = DateTime.Now
                                };

                                DBC.People.Add(person);
                                DBC.SaveChanges();

                                long LastUserID = person.Id;

                                PersonPermission personPermission = new PersonPermission
                                {
                                    PersonId = LastUserID,
                                    PermissionId = 4,
                                    Active = true,
                                    TimeCreated = DateTime.Now
                                };

                                DBC.PersonPermissions.Add(personPermission);

                                Session["Id"] = LastUserID;
                                Session["UserFullName"] = string.Concat(person.Name, " ", person.MiddleName);
                                Session["Photo"] = "/Assets/img/user/emptyAvatar.png";

                                FormsAuthentication.SetAuthCookie(person.Email, false);

                                person.LastLogin = DateTime.Now;
                                DBC.SaveChanges();

                                return RedirectToAction("SignInRedirect");
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Todos los campos son obligatorios");
                }

                return View(modelo);
            }
        }

        public ActionResult SignOut()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();

            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult SignInRedirect()
        {
            string[] roles = Roles.GetRolesForUser();

            if (roles.Contains("Super Administrador"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (roles.Contains("Administrador"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (roles.Contains("Financiero"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (roles.Contains("Jefe departamental"))
            {
                return RedirectToAction("Administrative", "Project");
            }
            else if (roles.Contains("Jefe academia"))
            {
                return RedirectToAction("Index", "Project");
            }
            else if (roles.Contains("Coordinador de carrera"))
            {
                return RedirectToAction("Administrative", "Project");
            }
            else if (roles.Contains("Docente"))
            {
                return RedirectToAction("Teacher", "Project");
            }
            else if (roles.Contains("Gestión Tecnológica Y Vinculación"))
            {
                return RedirectToAction("Index", "Company");
            }
            else
            {
                return RedirectToAction("Index", "Receipt");
            }
        }
    }
}