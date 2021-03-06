//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEPRET.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Project()
        {
            this.Advisers = new HashSet<Adviser>();
            this.Comments = new HashSet<Comment>();
            this.ProjectCareers = new HashSet<ProjectCareer>();
            this.ProjectCompanies = new HashSet<ProjectCompany>();
            this.ProjectFiles = new HashSet<ProjectFile>();
            this.ProjectPersons = new HashSet<ProjectPerson>();
            this.SpecificObjectives = new HashSet<SpecificObjective>();
        }
    
        public long Id { get; set; }
        public long Id_ProjectType { get; set; }
        public long Id_Company { get; set; }
        public long Id_Nature { get; set; }
        public long Id_Ambit { get; set; }
        public long Id_Kind { get; set; }
        public long Id_ProjectPhase { get; set; }
        public string Titulo { get; set; }
        public string ObjetivoGeneral { get; set; }
        public string ObjetivosEspecificos { get; set; }
        public string Justificacion { get; set; }
        public string Actividades { get; set; }
        public string Comentarios { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Adviser> Advisers { get; set; }
        public virtual Ambit Ambit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual Company Company { get; set; }
        public virtual Kind Kind { get; set; }
        public virtual Nature Nature { get; set; }
        public virtual ProjectType ProjectType { get; set; }
        public virtual ProjectPhase ProjectPhase { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectCareer> ProjectCareers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectCompany> ProjectCompanies { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectFile> ProjectFiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectPerson> ProjectPersons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpecificObjective> SpecificObjectives { get; set; }
    }
}
