using ControlRutas.Data;
using ControlRutas.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlRutas.Controllers
{
    public class APIUsuariosController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: APIUsuarios
        public ActionResult Index()
        {
            return View();
        }

        // GET: APIUsuarios/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: APIUsuarios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: APIUsuarios/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: APIUsuarios/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: APIUsuarios/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: APIUsuarios/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: APIUsuarios/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetUsuarios()
        {
            try
            {
                // Usar 'var' para la lista de tipos anónimos.
                // Incluir explícitamente TiposUsuarios.
                var list = await db.Usuarios
                    .Include(u => u.TiposUsuarios) // Carga previa para TiposUsuarios.Nombre
                    .Select(u => new // Proyección a un tipo anónimo
                    {
                        Id = u.Id,
                        GUID = u.GUID,
                        PrimerNombre = u.PrimerNombre,
                        SegundoNombre = u.SegundoNombre,
                        PrimerApellido = u.PrimerApellido,
                        SegundoApellido = u.SegundoApellido,
                        Email = u.Email,
                        NumeroTelefono = u.NumeroTelefono,
                        IdTipoUsuario = u.IdTipoUsuario,
                        TipoUsuario = u.TiposUsuarios != null ? u.TiposUsuarios.Nombre : "Sin Tipo", // Acceso seguro
                        Estado = u.Estado, // Asumiendo que es una propiedad directa (ej. bool o int)

                        // Subconsulta simplificada para IdEstablecimiento.
                        // ADVERTENCIA: Esto todavía puede causar problemas N+1 si EF no lo optimiza bien.
                        // Considera pre-cargar estos datos si el rendimiento es un problema.
                        IdEstablecimiento = db.UsuariosEstablecimientos
                            .Where(ue => ue.IdUsuario == u.Id)
                            .Select(ue => ue.EstablecimientosEducativos != null ? ue.EstablecimientosEducativos.Id : (int?)null) // Proyectar a int? para manejar nulos
                            .FirstOrDefault() ?? 0, // Si es null (no encontrado o EstablecimientoEducativos es null), devuelve 0. El DTO espera int.

                        // Subconsulta simplificada para Establecimiento.
                        Establecimiento = db.UsuariosEstablecimientos
                            .Where(ue => ue.IdUsuario == u.Id)
                            .Select(ue => ue.EstablecimientosEducativos != null ? ue.EstablecimientosEducativos.Nombre : null)
                            .FirstOrDefault() ?? "No Asignado"
                    })
                    .ToListAsync(); // Usar ToListAsync() para operaciones asíncronas

                var response = new ResponseRutasDTO
                {
                    message = "Usuarios obtenidos correctamente",
                    status = HttpStatusCode.OK,
                    data = list
                };
                // Para .NET Framework MVC, JsonRequestBehavior.AllowGet es necesario para peticiones GET.
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Es MUY importante registrar la excepción 'ex' en tu sistema de logging.
                // logger.LogError(ex, "Error en GetUsuarios");

                var errorResponse = new ResponseRutasDTO
                {
                    data = null,
                    message = "Error al obtener los usuarios. " + ex.Message, // Mensaje genérico en producción
                    status = HttpStatusCode.InternalServerError
                };
                // HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Opcional en .NET Framework
                return Json(errorResponse, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetUsuariosPorTipo(int idTipoUsuario) // Renombrado 'id' para mayor claridad
        {
            try
            {
                // Usar 'var' para la lista de tipos anónimos.
                var list = await db.Usuarios
                    // 1. FILTRAR PRIMERO por IdTipoUsuario para reducir el conjunto de datos.
                    .Where(u => u.IdTipoUsuario == idTipoUsuario)
                    // 2. INCLUIR entidades relacionadas necesarias para la proyección.
                    .Include(u => u.TiposUsuarios) // Para u.TiposUsuarios.Nombre
                                                   // 3. SELECCIONAR (proyectar) al tipo anónimo.
                    .Select(u => new
                    {
                        Id = u.Id,
                        GUID = u.GUID,
                        PrimerNombre = u.PrimerNombre,
                        SegundoNombre = u.SegundoNombre,
                        PrimerApellido = u.PrimerApellido,
                        SegundoApellido = u.SegundoApellido,
                        Email = u.Email,
                        NumeroTelefono = u.NumeroTelefono,
                        IdTipoUsuario = u.IdTipoUsuario, // Ya filtrado, pero bueno tenerlo en el resultado
                        TipoUsuario = u.TiposUsuarios != null ? u.TiposUsuarios.Nombre : "Sin Tipo", // Acceso seguro
                        Estado = u.Estado,

                        // Subconsulta para IdEstablecimiento.
                        // ADVERTENCIA: Potencial N+1, incluso después de filtrar. Considerar pre-carga.
                        IdEstablecimiento = db.UsuariosEstablecimientos
                            .Where(ue => ue.IdUsuario == u.Id) // 'u' aquí es un usuario ya filtrado por tipo
                            .Select(ue => ue.EstablecimientosEducativos != null ? ue.EstablecimientosEducativos.Id : (int?)null)
                            .FirstOrDefault() ?? 0,

                        // Subconsulta para Establecimiento.
                        Establecimiento = db.UsuariosEstablecimientos
                            .Where(ue => ue.IdUsuario == u.Id)
                            .Select(ue => ue.EstablecimientosEducativos != null ? ue.EstablecimientosEducativos.Nombre : null)
                            .FirstOrDefault() ?? "No Asignado"
                    })
                    .ToListAsync(); // Usar ToListAsync() para operaciones asíncronas

                var response = new ResponseRutasDTO
                {
                    message = "Usuarios obtenidos correctamente",
                    status = HttpStatusCode.OK,
                    data = list
                };
                // Para .NET Framework MVC, JsonRequestBehavior.AllowGet es necesario.
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Registrar 'ex'
                var errorResponse = new ResponseRutasDTO
                {
                    data = null,
                    message = "Error al obtener los usuarios por tipo. " + ex.Message,
                    status = HttpStatusCode.InternalServerError
                };
                return Json(errorResponse, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetUsuario(int id)
        {
            try
            {
                // 1. Usar FindAsync para operaciones asíncronas y sintaxis simplificada.
                Usuarios usuario = await db.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    // 2. Añadir JsonRequestBehavior.AllowGet y eliminar cast innecesario.
                    return Json(new
                    {
                        success = false,
                        message = "Usuario no encontrado"
                    }, JsonRequestBehavior.AllowGet);
                }

                UsuarioOB usuarioOb;
                switch (usuario.IdTipoUsuario)
                {
                    case 4:
                        usuarioOb = new UsuarioPiloto(usuario, db);
                        break;
                    case 5:
                        usuarioOb = new UsuarioPadre(usuario, db);
                        break;
                    default:
                        usuarioOb = new UsuarioOB(usuario, db);
                        break;
                }

                return Json(new
                {
                    success = true,
                    message = "Éxito",
                    usuario = usuarioOb
                }, JsonRequestBehavior.AllowGet); // Añadido JsonRequestBehavior.AllowGet
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción.
                // logger.LogError(ex, "Error en GetUsuario para id {UsuarioId}", id);

                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error al obtener el usuario."
                    // , errorDetail = ex.Message // Opcional, para desarrollo
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
