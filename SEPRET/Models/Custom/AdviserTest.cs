using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class AdviserTest
    {
        public long Id { get; set; }
        public Nullable<long> Id_Project { get; set; }
        public Nullable<long> Id_Person { get; set; }
        public Nullable<long> Id_AdviserType { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
        public Adviser advisers { get; set; }
        public int total { get; set; }
    }
}