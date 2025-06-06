//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ControlRutas.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstablecimientosEducativos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstablecimientosEducativos()
        {
            this.MediosTransporte = new HashSet<MediosTransporte>();
            this.Estudiantes = new HashSet<Estudiantes>();
            this.UsuariosEstablecimientos = new HashSet<UsuariosEstablecimientos>();
        }
    
        public int Id { get; set; }
        public string GUID { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public bool Estado { get; set; }
        public string Foto { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MediosTransporte> MediosTransporte { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Estudiantes> Estudiantes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UsuariosEstablecimientos> UsuariosEstablecimientos { get; set; }
    }
}
