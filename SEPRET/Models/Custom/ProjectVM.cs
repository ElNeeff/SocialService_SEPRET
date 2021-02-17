using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class ProjectVM
    {
        public long Id { get; set; }
        public Nullable<long> Id_ProjectType { get; set; }
        public Nullable<long> Id_Company { get; set; }
        public Nullable<long> Id_Nature { get; set; }
        public Nullable<long> Id_Ambit { get; set; }
        public Nullable<long> Id_Kind { get; set; }
        public Nullable<long> Id_ProjectPhase { get; set; }
        public string Titulo { get; set; }
        public string ObjetivoGeneral { get; set; }
        public string ObjetivosEspecificos { get; set; }
        public string Justificacion { get; set; }
        public string Actividades { get; set; }
        public string Comentarios { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        //CUSTOM ATTRIBUTES
        public string TipoDeProyecto { get; set; }
        public string Empresa { get; set; }
        public string Caracter { get; set; }
        public string Ambito { get; set; }
        public string Tipo { get; set; }
        public string Etapa { get; set; }
        public string Carrera { get; set; }
        public int MiembrosPostulados { get; set; }
        public List<PersonVM> Miembros { get; set; }
        public List<long?> Id_Carreras { get; set; }
        public List<CommentVM> Comments { get; set; }
        public string CommentCC { get; set; }
        public List<PersonVM> Revisores { get; set; }
        public List<PersonVM> Asesores { get; set; }
        public string CommentRevisor { get; set; }
        public string LastComment { get; set; }
        public string TimeCreatedFormatted { get; set; }
        public HttpPostedFileBase File { get; set; }
        public string Member { get; set; }
        public string Presentador { get; set; }
        public string EmailPresentador { get; set; }
        public bool PDFExists { get; set; }

        // PROJECTCAREER ATTRIBUTES
        public Nullable<long> Id_Career { get; set; }
    }
}