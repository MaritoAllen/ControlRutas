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
    
    public partial class UsuariosEstablecimientos
    {
        public int Id { get; set; }
        public string GUID { get; set; }
        public int IdUsuario { get; set; }
        public int IdEstablecimientoEducativo { get; set; }
        public bool Activo { get; set; }
    
        public virtual EstablecimientosEducativos EstablecimientosEducativos { get; set; }
        public virtual Usuarios Usuarios { get; set; }
    }
}
