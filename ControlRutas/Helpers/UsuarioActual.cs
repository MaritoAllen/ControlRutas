using ControlRutas.Data;
using ControlRutas.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.Helpers
{
    public class UsuarioActual
    {
        public static string ObtenerNombreUsuario()
        {
            // Acceder de forma segura a HttpContext.Current y sus propiedades anidadas
            var identity = HttpContext.Current?.User?.Identity;

            if (identity != null && identity.IsAuthenticated)
            {
                return identity.Name;
            }

            return ""; // Valor predeterminado si no está autenticado o el contexto/identidad no está disponible
        }
        public static int ObtenerIdUsuario()
        {
            // 1. Verificación robusta de la identidad del usuario y disponibilidad del contexto HTTP
            var identity = HttpContext.Current?.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return 0; // Usuario no autenticado o contexto no disponible
            }

            string nombreUsuario = identity.Name;

            // Es buena práctica verificar si el nombre de usuario (que se usará para la búsqueda) es válido
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                // Considera loguear esta situación: usuario autenticado pero sin nombre.
                return 0;
            }

            // 2. Manejo adecuado del ciclo de vida del DbContext con 'using'
            //    ADVERTENCIA: Ver nota importante sobre la instanciación de DbContext abajo.
            using (ControlRutasEntities db = new ControlRutasEntities())
            {
                // 3. Consulta LINQ simplificada y más segura
                Usuarios usuarioEncontrado = db.Usuarios
                    .FirstOrDefault(u => u.Email == nombreUsuario); // Asume que Identity.Name es el Email

                // 4. Verificar si el usuario fue encontrado en la base de datos
                if (usuarioEncontrado != null)
                {
                    return usuarioEncontrado.Id;
                }
                else
                {
                    // El usuario está autenticado a nivel de HttpContext,
                    // pero no se encontró un registro coincidente en la tabla Usuarios.
                    // Esto podría indicar un problema de sincronización de datos o configuración.
                    // Considera loguear esta situación.
                    return 0; // Devuelve 0 si no se encuentra, según la lógica original
                }
            }
        }
        public static string ObtenerNombreCompletoUsuario()
        {
            // 1. Verificación robusta de la identidad del usuario y disponibilidad del contexto HTTP
            var identity = HttpContext.Current?.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return ""; // Usuario no autenticado o contexto no disponible
            }

            string nombreUsuarioClaim = identity.Name; // Nombre de usuario del sistema de identidad (ej. Email)

            if (string.IsNullOrEmpty(nombreUsuarioClaim))
            {
                // Considera loguear esta situación: usuario autenticado pero sin nombre en el claim.
                return "";
            }

            // 2. Manejo adecuado del ciclo de vida del DbContext con 'using'
            //    ADVERTENCIA: Revisa la nota importante sobre la instanciación de DbContext en métodos estáticos al final.
            using (ControlRutasEntities db = new ControlRutasEntities())
            {
                // 3. Consulta LINQ simplificada para encontrar al usuario
                //    Asume que 'nombreUsuarioClaim' (proveniente de Identity.Name) se usa para buscar por Email.
                Usuarios usuarioEncontrado = db.Usuarios
                    .FirstOrDefault(u => u.Email == nombreUsuarioClaim);

                // 4. Verificar si el usuario fue encontrado en la base de datos
                if (usuarioEncontrado != null)
                {
                    // Construir el nombre completo de forma robusta, manejando partes nulas o vacías.
                    var partesDelNombre = new List<string>();
                    if (!string.IsNullOrWhiteSpace(usuarioEncontrado.PrimerNombre))
                    {
                        partesDelNombre.Add(usuarioEncontrado.PrimerNombre.Trim());
                    }
                    if (!string.IsNullOrWhiteSpace(usuarioEncontrado.PrimerApellido))
                    {
                        partesDelNombre.Add(usuarioEncontrado.PrimerApellido.Trim());
                    }
                    // Podrías añadir SegundoNombre y SegundoApellido aquí si existen y los quieres incluir:
                    // if (!string.IsNullOrWhiteSpace(usuarioEncontrado.SegundoNombre))
                    // {
                    //     partesDelNombre.Add(usuarioEncontrado.SegundoNombre.Trim());
                    // }
                    // if (!string.IsNullOrWhiteSpace(usuarioEncontrado.SegundoApellido))
                    // {
                    //     partesDelNombre.Add(usuarioEncontrado.SegundoApellido.Trim());
                    // }

                    return string.Join(" ", partesDelNombre); // Une las partes con un espacio
                }
                else
                {
                    // El usuario está autenticado (HttpContext), pero no se encontró en la tabla Usuarios.
                    // Considera loguear esta discrepancia.
                    return ""; // Devuelve cadena vacía si no se encuentra, según la lógica original
                }
            }
        }
        public static UsuarioOB ObtenerUsuario(string id)
        {
            // 1. Verificación robusta de la identidad del usuario y disponibilidad del contexto HTTP
            var identity = HttpContext.Current?.User?.Identity;

            // Si el propósito es obtener un usuario específico por 'id' (GUID)
            // y la autenticación es solo una puerta de entrada general al método.
            if (identity == null || !identity.IsAuthenticated)
            {
                // Considera si esta verificación de autenticación general es realmente lo que quieres aquí,
                // ya que buscas un usuario por un 'id' específico, no necesariamente el usuario logueado.
                // Si es un requerimiento, está bien.
                return null;
            }

            if (string.IsNullOrEmpty(id)) // Buena práctica: verificar si el ID proporcionado es válido.
            {
                // Considera loguear o manejar este caso (ej. ID inválido).
                return null;
            }

            // 2. Manejo adecuado del ciclo de vida del DbContext con 'using'
            //    ADVERTENCIA: Revisa la nota importante sobre la instanciación de DbContext en métodos estáticos al final.
            using (ControlRutasEntities db = new ControlRutasEntities())
            {
                // 3. Consulta LINQ simplificada para encontrar al usuario por GUID
                Usuarios usuarioEntity = db.Usuarios.FirstOrDefault(u => u.GUID == id);

                // 4. Verificar si el usuario fue encontrado en la base de datos
                if (usuarioEntity == null)
                {
                    return null; // Usuario con el GUID especificado no encontrado
                }

                // 5. Instanciar el tipo de UsuarioOB apropiado.
                //    Los casts a (UsuarioOB) no son necesarios si las clases derivadas
                //    heredan de UsuarioOB, la asignación es implícita.
                UsuarioOB usuarioResultado;
                switch (usuarioEntity.IdTipoUsuario)
                {
                    case 1:
                        usuarioResultado = new UsuarioAdmin(usuarioEntity, db);
                        break;
                    case 2:
                        usuarioResultado = new UsuarioAdminEstablecimiento(usuarioEntity, db);
                        break;
                    case 3:
                        usuarioResultado = new UsuarioDueñoBuses(usuarioEntity, db);
                        break;
                    case 4:
                        usuarioResultado = new UsuarioPiloto(usuarioEntity, db);
                        break;
                    case 5:
                        usuarioResultado = new UsuarioPadre(usuarioEntity, db);
                        break;
                    default:
                        usuarioResultado = new UsuarioOB(usuarioEntity, db); // Tipo base o por defecto
                        break;
                }
                return usuarioResultado;
            }
        }
    }
}