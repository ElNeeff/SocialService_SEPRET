using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class InstrumentationVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_Unit { get; set; }
        public Nullable<long> Id_InstrumentationType { get; set; }
        public string Indice { get; set; }
        public string Descripcion { get; set; }
        public bool Preset { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTES
        public long Id_Subject { get; set; }
        public string Competencia { get; set; }
        public string HorasTeoricas { get; set; }
        public string HorasPracticas { get; set; }
        //TOPIC
        public string topicsRAW { get; set; }
        public List<Instrumentation> topics { get; set; }
        //LEARNINGACTIVITY
        public string learningActivitiesRAW { get; set; }
        public List<Instrumentation> learningActivities { get; set; }
        //TEACHINGACTIVITY
        public string teachingActivitiesRAW { get; set; }
        public List<Instrumentation> teachingActivities { get; set; }
        //PROEFICIENCY
        public string proeficienciesRAW { get; set; }
        public List<Instrumentation> proeficiencies { get; set; }
    }
}