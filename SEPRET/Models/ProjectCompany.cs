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
    
    public partial class ProjectCompany
    {
        public long Id { get; set; }
        public long Id_Project { get; set; }
        public long Id_Company { get; set; }
        public string Nombre { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Project Project { get; set; }
    }
}
