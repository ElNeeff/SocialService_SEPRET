using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class PhaseVM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTES
        public string TimeCreatedFormatted { get; set; }
    }
}