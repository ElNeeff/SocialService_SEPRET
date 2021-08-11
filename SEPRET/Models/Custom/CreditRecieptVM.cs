using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class CreditRecieptVM
    {
        public long Id { get; set; }//id del credito enviado
        public long PersonId { get; set; }
        public long CreditId { get; set; }
        public long PhaseId { get; set; }
        public string URL_PDF { get; set; }
        public string Approbate { get; set; }
        public Nullable<long> ApprobateBy { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }


        //DATOS DE PERSONA
        public string PersonName { get; set; }
        public string Email { get; set; }
        public string Enrollment { get; set; }
        public string Career { get; set; }

        //Datos del credito
        public string Credit { get; set; }

        //Datos del archivo pdf con el credito
        public string RelativePath { get; set; }
        public HttpPostedFileBase File { get; set; }



    }
}