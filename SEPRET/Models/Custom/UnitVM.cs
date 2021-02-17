using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class UnitVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_Subject { get; set; }
        public string Indice { get; set; }
        public string Competencia { get; set; }
        public string Descripcion { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string TimeCreatedFormatted { get; set; }
        public List<Instrumentation> Topics { get; set; }
        public List<Instrumentation> LearningActivities { get; set; }
        public List<Instrumentation> TeachingActivities { get; set; }
        public List<Instrumentation> Proeficiencies { get; set; }
    }
}