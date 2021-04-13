using SEPRET.Models;
using SEPRET.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SEPRET.Controllers
{
    public class MyFileController : Controller
    {
        // GET: MyFile
        public ActionResult Index()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                IEnumerable<MyFile> data = User.IsInRole("División de estudios profesionales") ? DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2).Take(12).ToList() : DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2 && x.Person.CareerId == CareerId).Take(12).ToList();

                List<MyFileVM> myFiles = data.Select(x => new MyFileVM
                {
                    Id = x.Id,
                    Id_FileDictum = x.Id_FileDictum,
                    Nombre = x.Nombre,
                    TypeName = x.FileType.Nombre,
                    DictumName = x.FileDictum.Nombre,
                    Comments = x.MyFileComments.Where(y => y.Active && y.Id_File == x.Id).ToList(),
                    Person = new PersonVM
                    {
                        Id = x.Person.Id,
                        Enrollment = x.Person.Enrollment,
                        UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        Email = x.Person.Email,
                        Career = x.Person.Career.Name
                    },
                    CoordinadorEmail = DBC.PersonPermissions.FirstOrDefault(y => y.Person.Active && y.Person.CareerId == x.Person.CareerId && y.Permission.Name.ToLower().Contains("coordinador")).Person.Email,
                    DivisionEmail = DBC.PersonPermissions.FirstOrDefault(y => y.Person.Active && y.Permission.Name.ToLower().Contains("estudios profesionales")).Person.Email,
                    TimeUpdated = x.TimeUpdated == null ? x.TimeCreated : x.TimeUpdated
                }).OrderByDescending(x => x.Id).ToList();

                return View(myFiles);
            }
        }

        [HttpPost]
        public JsonResult Dictum(long Id, string Dictum, string Comment)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                MyFile myFile = DBC.MyFiles.FirstOrDefault(x => x.Id == Id);
                long dictum = myFile.Id_FileDictum;
                long UserId = (long)Session["Id"];
                bool success = false;

                switch (Dictum)
                {
                    case "Accept":
                        myFile.Id_FileDictum = 3;
                        success = true;
                        break;
                    case "Reject":
                        if (!string.IsNullOrEmpty(Comment.Trim()))
                        {
                            myFile.Id_FileDictum = 1;

                            MyFileComment comment = new MyFileComment
                            {
                                Id_File = myFile.Id,
                                Comment = Comment,
                                Active = true,
                                TimeCreated = DateTime.Now
                            };
                            DBC.MyFileComments.Add(comment);
                            success = true;
                        }
                        break;
                    default:
                        break;
                }

                myFile.TimeUpdated = DateTime.Now;

                DBC.SaveChanges();

                return Json(success, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Paginate(string Filter)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                long TotalRegisters = User.IsInRole("Alumno") ? DBC.MyFiles.Where(x => x.Id_Person == UserId && x.Active).Count() : User.IsInRole("Coordinador de carrera") && Filter == "All" ? DBC.MyFiles.Where(x => x.Active && x.Person.CareerId == CareerId).Count() : User.IsInRole("Coordinador de carrera") ? DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2 && x.Person.CareerId == CareerId).Count() : User.IsInRole("División de estudios profesionales") && Filter == "All" ? DBC.MyFiles.Where(x => x.Active).Count() : DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2).Count();

                return PartialView("~/Views/Shared/_Paginacion.cshtml", TotalRegisters);
            }
        }

        [HttpPost]
        public ActionResult Tracking(string Filter, string Keyword, int Skip = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                long CareerId = (long)Session["CareerId"];
                IEnumerable<MyFile> data = User.IsInRole("Alumno") ? DBC.MyFiles.Where(x => x.Id_Person == UserId && x.Active).ToList() : User.IsInRole("Coordinador de carrera") && Filter == "All" ? DBC.MyFiles.Where(x => x.Active && x.Person.CareerId == CareerId).ToList() : User.IsInRole("Coordinador de carrera") ? DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2 && x.Person.CareerId == CareerId).ToList() : User.IsInRole("División de estudios profesionales") && Filter == "All" ? DBC.MyFiles.Where(x => x.Active).ToList() : DBC.MyFiles.Where(x => x.Active && x.Id_FileDictum == 2).ToList();

                #region Búsqueda
                if (!string.IsNullOrEmpty(Keyword))
                {
                    Keyword = Keyword.ToLower();

                    data = data.Where(x => x.FileDictum.Nombre.ToLower().Contains(Keyword) ||
                    x.Person.Enrollment.ToLower().Contains(Keyword) ||
                    x.Person.Career.Name.ToLower().Contains(Keyword) ||
                    string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName).ToLower().Contains(Keyword) ||
                    x.FileType.Nombre.ToLower().Contains(Keyword) ||
                    x.TimeCreated.ToString().ToLower().Contains(Keyword)
                    );
                }
                #endregion

                if (!User.IsInRole("Alumno"))
                {
                    int skip = 12 * Skip;
                    data = data.Skip(skip).Take(12);
                }

                List<MyFileVM> myFiles = data.Select(x => new MyFileVM
                {
                    Id = x.Id,
                    Id_FileDictum = x.Id_FileDictum,
                    Id_FileType = x.Id_FileType,
                    Nombre = x.Nombre,
                    TypeName = x.FileType.Nombre,
                    DictumName = x.FileDictum.Nombre,
                    Comments = x.MyFileComments.Where(y => y.Active && y.Id_File == x.Id).ToList(),
                    Person = new PersonVM
                    {
                        Id = x.Person.Id,
                        Enrollment = x.Person.Enrollment,
                        UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                        Email = x.Person.Email,
                        Career = x.Person.Career.Name
                    },
                    CoordinadorEmail = DBC.PersonPermissions.FirstOrDefault(y => y.Person.Active && y.Person.CareerId == x.Person.CareerId && y.Permission.Name.ToLower().Contains("coordinador")).Person.Email,
                    DivisionEmail = DBC.PersonPermissions.FirstOrDefault(y => y.Person.Active && y.Permission.Name.ToLower().Contains("estudios profesionales")).Person.Email,
                    TimeUpdated = x.TimeUpdated == null ? x.TimeCreated : x.TimeUpdated
                }).OrderByDescending(x => x.Id).ToList();

                return PartialView(string.Concat("~/Views/MyFile/", User.IsInRole("Alumno") ? "_Tracking" : "_SearchFiles", ".cshtml"), myFiles);
            }
        }

        [HttpGet]
        public ActionResult UploadPanel()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                UploadPanelVM uploadPanel = new UploadPanelVM
                {
                    MyFileVMs = DBC.MyFiles.Where(x => x.FileType.Nombre.ToLower().Contains("formatos de residencias profesionales")).OrderByDescending(x => x.Id).Select(x => new MyFileVM
                    {
                        Id = x.Id,
                        Nombre = x.Nombre,
                        Active = x.Active,
                        TimeCreated = x.TimeCreated,
                        Person = new PersonVM
                        {
                            Id = x.Person.Id,
                            UserFullName = string.Concat(x.Person.Name, " ", x.Person.MiddleName, " ", x.Person.LastName),
                            PermissionName = x.Person.PersonPermissions.FirstOrDefault(y => y.PersonId == x.Person.Id).Permission.Name
                        }
                    }).ToList()
                };

                return PartialView("~/Views/MyFile/_UploadPanel.cshtml", uploadPanel);
            }
        }

        [HttpGet]
        public FileResult DownloadFormats(long Id = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                MyFile myFile = Id is 0 ? DBC.MyFiles.Where(x => x.Active && x.FileType.Nombre.ToLower().Contains("formatos de residencias profesionales")).ToList().LastOrDefault() : DBC.MyFiles.FirstOrDefault(x => x.Id == Id);
                var filePath = Server.MapPath(myFile.Ruta);

                return File(filePath, "application/force-download", Path.GetFileName(filePath));
            }
        }

        [HttpPost]
        public JsonResult UploadFormats(MyFileVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                bool success = false;
                string message = string.Empty;
                long UserId = (long)Session["Id"];

                if (modelo.File != null)
                {
                    if (modelo.File.ContentType.Contains("application/x-zip-compressed") || modelo.File.FileName.Substring(modelo.File.FileName.Length - 3).ToLower().Contains("rar"))
                    {
                        string Folders = "~/Assets/FileSystem/Residencias/";
                        string NombreArchivo = Path.GetFileName(modelo.File.FileName);
                        string Ruta = Server.MapPath(string.Concat("~", Folders));
                        string Destination = Ruta + NombreArchivo;
                        string RelativePath = Folders + NombreArchivo;
                        Directory.CreateDirectory(Ruta);
                        modelo.File.SaveAs(Destination);

                        MyFile fileExists = DBC.MyFiles.Where(x => x.FileType.Nombre.ToLower() == "formatos de residencias profesionales" && x.Active).ToList().LastOrDefault();

                        MyFile myFile = new MyFile
                        {
                            Id_Person = UserId,
                            Id_FileType = DBC.FileTypes.FirstOrDefault(x => x.Nombre.ToLower().Contains("formatos de residencias profesionales")).Id,
                            Id_FileDictum = 3,
                            Nombre = NombreArchivo,
                            Tipo = modelo.File.ContentType,
                            Ruta = RelativePath,
                            Active = true,
                            TimeCreated = DateTime.Now
                        };

                        DBC.MyFiles.Add(myFile);
                        if (fileExists != null)
                        {
                            fileExists.Active = false;
                            fileExists.TimeUpdated = DateTime.Now;
                        }

                        DBC.SaveChanges();
                        success = true;
                        message = "Los formatos se subieron exitosamente";
                    }
                    else
                    {
                        message = "Los formatos deben estar comprimidos en ZIP";
                    }
                }
                else
                {
                    message = "Debes adjuntar los formatos comprimidos en ZIP";
                }

                return Json(new { message, success }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult UploadFile(MyFileVM modelo)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                bool success = false;
                string message = string.Empty;
                long UserId = (long)Session["Id"];
                long FileId = 0;
                long TypeId = 0;

                if (modelo.File != null)
                {
                    if (modelo.File.ContentType.Contains("pdf"))
                    {
                        string Folders = string.Concat("/Assets/pdf/anteproyectos/", (string)Session["Enrollment"], "/");
                        string NombreArchivo = Path.GetFileName(modelo.File.FileName);
                        string Ruta = Server.MapPath(string.Concat("~", Folders));
                        string Destination = Ruta + NombreArchivo;
                        string RelativePath = Folders + NombreArchivo;
                        Directory.CreateDirectory(Ruta);
                        modelo.File.SaveAs(Destination);

                        MyFile fileExists = DBC.MyFiles.FirstOrDefault(x => x.Id_Person == UserId && x.Id_FileType == modelo.Id_FileType && x.Active);

                        if (fileExists == null)
                        {
                            MyFile myFile = new MyFile
                            {
                                Id_Person = UserId,
                                Id_FileType = modelo.Id_FileType,
                                Id_FileDictum = 2,
                                Nombre = NombreArchivo,
                                Tipo = modelo.File.ContentType,
                                Ruta = RelativePath,
                                Active = true,
                                TimeCreated = DateTime.Now
                            };

                            DBC.MyFiles.Add(myFile);
                            DBC.SaveChanges();
                            FileId = myFile.Id;
                            TypeId = myFile.Id_FileType;
                        }
                        else
                        {
                            fileExists.Id_FileDictum = 2;
                            fileExists.Nombre = NombreArchivo;
                            fileExists.Tipo = modelo.File.ContentType;
                            fileExists.Ruta = RelativePath;
                            fileExists.TimeUpdated = DateTime.Now;

                            FileId = fileExists.Id;
                            TypeId = fileExists.Id_FileType;
                        }

                        success = true;
                        message = "El archivo se envió exitosamente";
                        DBC.SaveChanges();
                    }
                    else
                    {
                        message = "El archivo debe ser formato PDF";
                    }
                }
                else
                {
                    message = "Debes adjuntar el archivo PDF";
                }

                return Json(new { FileId, TypeId, message, success }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult ViewPDF(long TypeId = 0, long Id = 0)
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                long UserId = (long)Session["Id"];
                MyFile myFile = Id is 0 ? DBC.MyFiles.FirstOrDefault(x => x.Id_FileType == TypeId && x.Active && x.Id_Person == UserId) : DBC.MyFiles.FirstOrDefault(x => x.Id == Id && x.Active);
                dynamic base64 = false;
                long MyFileId, Id_FileDictum, Id_FileType;
                MyFileId = Id_FileDictum = Id_FileType = 0;

                if (myFile != null)
                {
                    var file = Server.MapPath(myFile.Ruta);

                    byte[] bytes = System.IO.File.ReadAllBytes(file);
                    base64 = string.Concat("data:application/pdf;base64,", Convert.ToBase64String(bytes));
                    MyFileId = myFile.Id;
                    Id_FileDictum = myFile.Id_FileDictum;
                    Id_FileType = myFile.Id_FileType;
                }

                return new JsonResult()
                {
                    Data = new { base64, MyFileId, Id_FileType, Id_FileDictum },
                    MaxJsonLength = 86753090,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpGet]
        public ActionResult getFileTypes()
        {
            using (SEPRETEntities DBC = new SEPRETEntities())
            {
                MyFileVM fileTypes = new MyFileVM
                {
                    FileTypes = DBC.FileTypes.Where(x => x.Active && x.Nombre.ToLower() != "reporte preliminar" && x.Nombre.ToLower() != "formatos de residencias profesionales").ToList()
                };

                return PartialView("~/Views/Myfile/_MyFiles.cshtml", fileTypes);
            }
        }
    }
}