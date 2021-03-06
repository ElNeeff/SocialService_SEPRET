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
    
    public partial class MyFile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MyFile()
        {
            this.MyFileComments = new HashSet<MyFileComment>();
        }
    
        public long Id { get; set; }
        public long Id_Person { get; set; }
        public long Id_FileType { get; set; }
        public long Id_FileDictum { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Ruta { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual FileDictum FileDictum { get; set; }
        public virtual FileType FileType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MyFileComment> MyFileComments { get; set; }
    }
}
