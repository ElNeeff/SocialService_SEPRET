using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class ProjectPersonVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_Project { get; set; }
        public Nullable<long> Id_Person { get; set; }
        public Nullable<long> Id_Dictum { get; set; }
        public bool Owner { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string PersonName { get; set; }
        public string PersonEmail { get; set; }
        public string PersonCareer { get; set; }
        public string ProjectName { get; set; }
        public string TimeCreatedFormatted { get; set; }
    }
}