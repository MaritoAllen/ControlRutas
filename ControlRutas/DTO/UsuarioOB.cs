using ControlRutas.Data;
using System;
using System.Linq;

namespace ControlRutas.DTO
{
    public class UsuarioOB
    {
        public string DiaSemanaHoy()
        {
            string str = DateTime.Now.DayOfWeek.ToString();
            if (str != null)
            {
                switch (str.Length)
                {
                    case 6:
                        switch (str[0])
                        {
                            case 'F':
                                if (str == "Friday")
                                    return "Viernes";
                                break;
                            case 'M':
                                if (str == "Monday")
                                    return "Lunes";
                                break;
                            case 'S':
                                if (str == "Sunday")
                                    return "Domingo";
                                break;
                        }
                        break;
                    case 7:
                        if (str == "Tuesday")
                            return "Martes";
                        break;
                    case 8:
                        switch (str[0])
                        {
                            case 'S':
                                if (str == "Saturday")
                                    return "Sábado";
                                break;
                            case 'T':
                                if (str == "Thursday")
                                    return "Jueves";
                                break;
                        }
                        break;
                    case 9:
                        if (str == "Wednesday")
                            return "Miércoles";
                        break;
                }
            }
            return "Día no encontrado";
        }

        public int Id { get; set; }

        public string GUID { get; set; }

        public string Email { get; set; }

        public string PrimerNombre { get; set; }

        public string SegundoNombre { get; set; }

        public string PrimerApellido { get; set; }

        public string SegundoApellido { get; set; }

        public string NombreCompleto
        {
            get => $"{this.PrimerNombre} {this.SegundoNombre} {this.PrimerApellido} {this.SegundoApellido}";
        }

        public string NumeroTelefono { get; set; }

        public int IdTipoUsuario { get; set; }

        public string TipoUsuario { get; set; }

        public bool Activo { get; set; }

        public int IdEstablecimiento { get; set; }

        public string Establecimiento { get; set; }

        public string Estado { get; set; }

        public string RutaGuid { get; set; }

        public string MessageToken { get; set; }

        public UsuarioOB(Usuarios usuario, ControlRutasEntities db)
        {
            this.Id = usuario.Id;
            this.GUID = usuario.GUID;
            this.Email = usuario.Email;
            this.PrimerNombre = usuario.PrimerNombre;
            this.SegundoNombre = usuario.SegundoNombre;
            this.PrimerApellido = usuario.PrimerApellido;
            this.SegundoApellido = usuario.SegundoApellido;
            this.NumeroTelefono = usuario.NumeroTelefono;
            this.IdTipoUsuario = usuario.IdTipoUsuario;
            this.TipoUsuario = usuario.TiposUsuarios.Nombre;
            this.Activo = usuario.Estado == 1;
            UsuariosEstablecimientos establecimientos = usuario.UsuariosEstablecimientos.FirstOrDefault(x => x.IdUsuario == usuario.Id);
            this.IdEstablecimiento = establecimientos?.EstablecimientosEducativos?.Id ?? 0;
            this.Establecimiento = establecimientos?.EstablecimientosEducativos?.Nombre ?? string.Empty;
            this.MessageToken = usuario.MessageToken;
        }
    }
}