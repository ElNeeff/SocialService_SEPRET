using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class CompanyVM
    {

        public long Id { get; set; }
        public long Id_Person { get; set; }
        public long Id_Dictum { get; set; }
        public long Id_Sector { get; set; }
        public string Nombre { get; set; }
        public string RFC { get; set; }
        public string Lema { get; set; }
        public string Mision { get; set; }
        public string Valores { get; set; }
        public string Calle { get; set; }
        public string Colonia { get; set; }
        public string CP { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string Telefono { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string TimeCreatedFormatted { get; set; }
        public bool IsAdmin { get; set; }
        public string Dictamen { get; set; }
    }
}