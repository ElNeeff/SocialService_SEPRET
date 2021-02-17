using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class ProjectTypeVM
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTE
        public string TimeCreatedFormatted { get; set; }
    }
}