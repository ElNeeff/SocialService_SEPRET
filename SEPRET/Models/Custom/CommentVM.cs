using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class CommentVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_Project { get; set; }
        public Nullable<long> Id_CommentType { get; set; }
        public Nullable<long> Id_Person { get; set; }
        public string Mensaje { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string Autor { get; set; }
        public string Rol { get; set; }
        public string CommentType { get; set; }
        public string TimeCreatedFormatted { get; set; }
    }
}