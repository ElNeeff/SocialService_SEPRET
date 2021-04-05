using System;
using System.Collections.Generic;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class MyFileVM
    {
        public long Id { get; set; }
        public long Id_Person { get; set; }
        public long Id_FileType { get; set; }
        public long Id_FileDictum { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Ruta { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTES
        public string TypeName { get; set; }
        public string DictumName { get; set; }
        public string CoordinadorEmail { get; set; }
        public string DivisionEmail { get; set; }
        public PersonVM Person { get; set; }
        public List<MyFileComment> Comments { get; set; }
        public List<FileType> FileTypes { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
}