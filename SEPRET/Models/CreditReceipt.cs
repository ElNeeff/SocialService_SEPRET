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
    
    public partial class CreditReceipt
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CreditReceipt()
        {
            this.Rejections = new HashSet<CreditRejection>();
        }
    
        public long Id { get; set; }
        public long PersonId { get; set; }
        public long CreditId { get; set; }
        public long PhaseId { get; set; }
        public string URL_PDF { get; set; }
        public string Approbate { get; set; }
        public Nullable<long> ApprobateBy { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
    
        public virtual Credit Credit { get; set; }
        public virtual CreditPhase Phase { get; set; }
        public virtual Person Person { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditRejection> Rejections { get; set; }
    }
}
