using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class SubjectVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_Career { get; set; }
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public string HorasTeoricas { get; set; }
        public string HorasPracticas { get; set; }
        public string Creditos { get; set; }
        public string Competencia { get; set; }
        public string Caracterizacion { get; set; }
        public string IntencionDidactica { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string TPC { get; set; }
        public string TimeCreatedFormatted { get; set; }
    }
}