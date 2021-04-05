using System;

namespace SEPRET.Models.Custom
{
    public class MyFileCommentVM
    {
        public long Id { get; set; }
        public long Id_File { get; set; }
        public string Comment { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
    }
}