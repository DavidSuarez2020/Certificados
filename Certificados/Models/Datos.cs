//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Certificados.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Datos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Datos()
        {
            this.Plantilla_Dato = new HashSet<Plantilla_Dato>();
            this.Documentos = new HashSet<Documentos>();
        }
    
        public int Id { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string curso { get; set; }
        public string fecha { get; set; }
        public string opcional1 { get; set; }
        public string opcional2 { get; set; }
        public string opcional3 { get; set; }
        public string opcional4 { get; set; }
        public System.DateTime fecha_registro { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Plantilla_Dato> Plantilla_Dato { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Documentos> Documentos { get; set; }
    }
}
