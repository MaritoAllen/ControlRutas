using ControlRutas.Data;
using ControlRutas.DTO;
using ControlRutas.Services;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlRutas.Controllers
{
    public class APIEventosController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private ControlRutasEntities db = new ControlRutasEntities();
        public APIEventosController()
        {
            _firebaseService = new FirebaseService();
        }
        // GET: APIEventos
        public ActionResult Index()
        {
            return View();
        }

        // GET: APIEventos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: APIEventos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: APIEventos/Create
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

        // GET: APIEventos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: APIEventos/Edit/5
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

        // GET: APIEventos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: APIEventos/Delete/5
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
        public JsonResult GetEventos()
        {
            var eventos = db.Eventos.ToList();
            return Json(new ResponseRutasDTO
            {
                data = eventos,
                message = "Eventos cargados correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PostEventos(Eventos evento)
        {
            db.Eventos.Add(evento);
            return Json(new ResponseRutasDTO
            {
                data = evento,
                message = "Evento creado correctamente",
                status = HttpStatusCode.Created,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> InicioRuta(int idPiloto, int direccion)
        {
            // Usaremos 'this.db' o solo 'db' si es un campo/propiedad de la clase.
            // Similar para '_firebaseService'.
            // 'eventosController' como alias de 'this' no es necesario.

            var rutaDia = new List<RutaTransporteDTO>();
            try
            {
                // Usar .Date para comparaciones de solo fecha
                DateTime fechaDeHoy = DateTime.Now.Date;

                MediosTransporte medioTransporte = db.MediosTransporte
                    .FirstOrDefault(r => r.IdCodigoPiloto == idPiloto);

                if (medioTransporte == null)
                {
                    // Considera devolver un error o un resultado indicando que el bus/piloto no se encontró.
                    return Json(new { error = "Medio de transporte para el piloto no encontrado." });
                }

                // Verificar si ya existe una ruta activa para hoy
                bool rutaActivaExistente = db.RutasMediosTransporte
                    .Any(x => x.EstudiantesMediosTransporte.MediosTransporte.Id == medioTransporte.Id &&
                              x.Fecha == fechaDeHoy && // Comparación de fecha simplificada
                              x.Estado == "Activa");

                if (rutaActivaExistente)
                {
                    // Si ya hay una ruta activa, obtener sus datos.
                    rutaDia = db.RutasMediosTransporte
                        .Where(x => x.EstudiantesMediosTransporte.MediosTransporte.Id == medioTransporte.Id &&
                                    x.Fecha == fechaDeHoy) // Podrías incluir x.Estado == "Activa" si solo quieres la activa
                        .Select(x => new RutaTransporteDTO // Mapeo directo a DTO
                        {
                            GUID = x.GUID,
                            Orden = x.Orden,
                            DireccionOrigen = x.DireccionOrigen,
                            LatitudOrigen = x.LatitudOrigen,
                            LongitudOrigen = x.LongitudOrigen,
                            DireccionDestino = x.DireccionDestino,
                            LatitudDestino = x.LatitudDestino,
                            LongitudDestino = x.LongitudDestino,
                            IdEstudiante = x.EstudiantesMediosTransporte.IdEstudiante,
                            EstudianteGUID = x.EstudiantesMediosTransporte.Estudiantes.GUID,
                            NombreEstudiante = (x.EstudiantesMediosTransporte.Estudiantes.PrimerNombre + " " +
                                               x.EstudiantesMediosTransporte.Estudiantes.SegundoNombre + " " +
                                               x.EstudiantesMediosTransporte.Estudiantes.PrimerApellido + " " +
                                               x.EstudiantesMediosTransporte.Estudiantes.SegundoApellido).Trim(),
                            Fecha = x.Fecha,
                            Grado = x.Grado
                            // Asegúrate que todas las propiedades de RutaTransporteDTO estén mapeadas.
                        })
                        .ToList();

                    var dataExistente = new
                    {
                        // Obtener el GUID de la ruta general (puede ser el GUID del primer tramo si todos comparten el mismo)
                        GUID = rutaDia.FirstOrDefault()?.GUID ?? db.RutasMediosTransporte
                                   .Where(x => x.EstudiantesMediosTransporte.MediosTransporte.Id == medioTransporte.Id && x.Fecha == fechaDeHoy && x.Estado == "Activa")
                                   .Select(x => x.GUID)
                                   .FirstOrDefault(),
                        ruta = rutaDia
                    };
                    return Json(dataExistente); // JsonRequestBehavior.AllowGet no suele ser necesario para POST en ASP.NET Core
                }

                // Si no hay ruta activa, se crea una nueva.
                List<EstudiantesMediosTransporte> estudiantesDelBus = db.EstudiantesMediosTransporte
                    .Include(et => et.Estudiantes) // Incluir Estudiantes para acceder a sus propiedades
                    .Where(r => r.IdMedioTransporte == medioTransporte.Id)
                    .ToList();

                var idsEstudiantesEnBus = estudiantesDelBus.Select(rm => rm.Id).ToList();

                // Filtrar estudiantes que no usaron la ruta hoy
                List<NoUsoRutasMediosTransporte> rutasNoUsoHoy = db.NoUsoRutasMediosTransporte
                    .Where(r => r.Fecha == fechaDeHoy && idsEstudiantesEnBus.Contains(r.IdEstudianteMedioTransporte))
                    .ToList();

                if (rutasNoUsoHoy.Any())
                {
                    var idsNoUso = rutasNoUsoHoy.Select(rnu => rnu.IdEstudianteMedioTransporte).ToHashSet();
                    estudiantesDelBus = estudiantesDelBus.Where(r => !idsNoUso.Contains(r.Id)).ToList();
                }

                string nuevoGuidRuta = Guid.NewGuid().ToString();
                int orden = 1;

                // Asumo que _firebaseService es un servicio inyectado o un miembro de la clase.
                await _firebaseService.NuevaRuta(nuevoGuidRuta, "0", "0"); // Parámetros "0","0" según lógica original

                foreach (EstudiantesMediosTransporte emtEstudiante in estudiantesDelBus.OrderBy(x => x.Orden))
                {
                    string dirOrigenDeterminado = direccion == 1 ? emtEstudiante.DireccionDestino : emtEstudiante.DireccionOrigen;
                    string dirDestinoDeterminado = direccion == 1 ? emtEstudiante.DireccionOrigen : emtEstudiante.DireccionDestino;
                    string latOrigenDeterminado = direccion == 1 ? emtEstudiante.LatitudDestino : emtEstudiante.LatitudOrigen;
                    string lonOrigenDeterminado = direccion == 1 ? emtEstudiante.LongitudDestino : emtEstudiante.LongitudOrigen;
                    string latDestinoDeterminado = direccion == 1 ? emtEstudiante.LatitudOrigen : emtEstudiante.LatitudDestino;
                    string lonDestinoDeterminado = direccion == 1 ? emtEstudiante.LongitudOrigen : emtEstudiante.LongitudDestino;

                    var nuevaRutaTramo = new RutasMediosTransporte
                    {
                        IdEstudianteMedioTransporte = emtEstudiante.Id,
                        GUID = nuevoGuidRuta,
                        Fecha = fechaDeHoy, // Usar la variable de fecha consistente
                        DireccionOrigen = dirOrigenDeterminado,
                        LatitudOrigen = latOrigenDeterminado,
                        LongitudOrigen = lonOrigenDeterminado,
                        DireccionDestino = dirDestinoDeterminado,
                        LatitudDestino = latDestinoDeterminado,
                        LongitudDestino = lonDestinoDeterminado,
                        Orden = orden,
                        Grado = "", // Según lógica original
                        HoraSalida = DateTime.Now, // La lógica original usaba fechaHoy, que era DateTime.Now con hora.
                                                   // Si solo es fecha, quizá necesites DateTime.Now o construir la hora.
                                                   // Para consistencia, si fechaDeHoy es .Date, HoraSalida podría ser fechaDeHoy + hora actual.
                                                   // O si la intención es la hora de inicio de la ruta: DateTime.Now.
                        HoraLlegada = DateTime.Now.AddMinutes(120.0), // Similar para HoraLlegada
                        Estado = "Activa"
                    };

                    db.RutasMediosTransporte.Add(nuevaRutaTramo);

                    rutaDia.Add(new RutaTransporteDTO
                    {
                        GUID = nuevoGuidRuta,
                        Orden = orden, // El DTO usa el 'orden' antes de incrementar para el siguiente tramo. Lógica original: orden se incrementaba y luego se usaba.
                                       // Si el DTO debe reflejar el orden del tramo actual, es 'orden'. Si es el siguiente, 'orden + 1'.
                                       // La lógica original `++orden; rutaDia.Add(new RutaTransporteDTO { Orden = orden, ...})` implica que el DTO tiene el orden del *siguiente* tramo o el orden ya actualizado.
                                       // Para que coincida con el tramo actual:
                                       // Orden = nuevaRutaTramo.Orden, (o simplemente 'orden' antes de '++orden')
                                       // Aquí mantendré la posible discrepancia si 'orden' se usa en el DTO después de ser incrementado para el *siguiente* DB Add.
                                       // Para ser claro: DTO.Orden = orden (actual)
                        DireccionOrigen = dirOrigenDeterminado,
                        LatitudOrigen = latOrigenDeterminado,
                        LongitudOrigen = lonOrigenDeterminado,
                        DireccionDestino = dirDestinoDeterminado,
                        LatitudDestino = latDestinoDeterminado,
                        LongitudDestino = lonDestinoDeterminado,
                        IdEstudiante = emtEstudiante.IdEstudiante,
                        EstudianteGUID = emtEstudiante.Estudiantes.GUID, // Necesita .Include(emt => emt.Estudiantes)
                        NombreEstudiante = $"{emtEstudiante.Estudiantes.PrimerNombre} {emtEstudiante.Estudiantes.SegundoNombre} {emtEstudiante.Estudiantes.PrimerApellido} {emtEstudiante.Estudiantes.SegundoApellido}".Trim(),
                        Fecha = fechaDeHoy,
                        Grado = ""
                    });

                    await _firebaseService.AgregarHijoRuta(nuevoGuidRuta, emtEstudiante.Estudiantes.GUID,
                        latOrigenDeterminado, lonOrigenDeterminado, latDestinoDeterminado, lonDestinoDeterminado);

                    orden++; // Incrementar orden para el siguiente estudiante/tramo
                }

                await db.SaveChangesAsync(); // Usar SaveChangesAsync en un método async

                return Json(new { GUID = nuevoGuidRuta, ruta = rutaDia });
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción completa.
                // log.Error("Error en InicioRuta", ex); 
                return Json(new { error = ex.Message, source = ex.Source, stackTrace = ex.StackTrace });
            }
        }

        // Si usas ASP.NET Core, IActionResult es más común que JsonResult directamente.
        // Si es MVC 5, Task<JsonResult> está bien.
        public JsonResult EntregaEstudiante(string guidRuta, string guidEstudiante)
        {
            try
            {
                // 1. Simplificación de la consulta LINQ y uso de FirstOrDefaultAsync
                RutasMediosTransporte rutaMedioTransporte = db.RutasMediosTransporte
                    // Considera incluir propiedades de navegación si no están cargadas por defecto y son necesarias para el filtro:
                    // .Include(r => r.EstudiantesMediosTransporte)
                    //     .ThenInclude(emt => emt.Estudiantes) 
                    .FirstOrDefault(r => r.GUID == guidRuta &&
                                              r.EstudiantesMediosTransporte.Estudiantes.GUID == guidEstudiante);

                // 2. Verificar si la entidad fue encontrada
                if (rutaMedioTransporte == null)
                {
                    // Puedes devolver NotFound() o un JSON con un error específico
                    return Json(new { error = "La ruta o el estudiante especificado no fue encontrado." });
                    // En ASP.NET Core, también podrías usar: return NotFound(new { error = "..." });
                }

                // 3. Modificar la entidad
                rutaMedioTransporte.Estado = "Finalizada";

                // 4. Guardar cambios de forma asíncrona
                // EF Core rastreará automáticamente el cambio en 'Estado' si 'rutaMedioTransporte'
                // fue obtenido del mismo contexto 'db'. No es necesario establecer el estado manualmente.
                db.SaveChanges();

                // 5. Devolver respuesta JSON simplificada
                return Json(new
                {
                    Message = "Entrega de estudiante actualizada correctamente.", // Mensaje de éxito opcional
                    GUID = guidRuta,
                    EstudianteGUID = guidEstudiante
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "Ocurrió un error al procesar la solicitud.",
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult VerificarRuta(int id) // Renombrado 'id' para mayor claridad
        {
            try
            {
                DateTime fechaDeHoy = DateTime.Now.Date; // Usar .Date para comparar solo la fecha

                // 1. Obtener el medio de transporte del piloto
                MediosTransporte medioTransporte = db.MediosTransporte
                    .FirstOrDefault(r => r.IdCodigoPiloto == id);

                if (medioTransporte == null)
                {
                    return Json(new { GUID = "", Estado = "Sin ruta", Mensaje = "Piloto o medio de transporte no encontrado." });
                }

                // 2. Verificar si existe una ruta activa para hoy y obtener su GUID
                // Incluimos las navegaciones necesarias para el filtro y la selección posterior
                var rutaActivaInfo = db.RutasMediosTransporte
                    .Include(x => x.EstudiantesMediosTransporte.MediosTransporte) // Para filtrar por medioTransporte.Id
                    .Where(x => x.EstudiantesMediosTransporte.MediosTransporte.Id == medioTransporte.Id &&
                                x.Fecha == fechaDeHoy &&
                                x.Estado == "Activa")
                    .Select(x => new { x.GUID }) // Solo necesitamos el GUID de la ruta principal por ahora
                    .FirstOrDefault();

                if (rutaActivaInfo == null) // No se encontró una ruta activa para hoy
                {
                    return Json(new { GUID = "", Estado = "Sin ruta" });
                }

                // 3. Si existe una ruta activa, obtener todos sus detalles/segmentos
                var detallesDeLaRuta = db.RutasMediosTransporte
                    .Include(x => x.EstudiantesMediosTransporte.Estudiantes) // Para el nombre del estudiante y su GUID
                                                                             // Filtramos por el GUID de la ruta activa y la fecha para asegurar que son los segmentos correctos
                    .Where(x => x.GUID == rutaActivaInfo.GUID && x.Fecha == fechaDeHoy)
                    .OrderBy(x => x.Orden) // Ordenar los segmentos de la ruta
                    .Select(x => new // Proyectar a un tipo anónimo o un DTO si lo tienes
                    {
                        x.GUID, // Este será el GUID de la ruta activa (compartido por todos los segmentos)
                        x.Orden,
                        x.DireccionOrigen,
                        x.LatitudOrigen,
                        x.LongitudOrigen,
                        x.DireccionDestino,
                        x.LatitudDestino,
                        x.LongitudDestino,
                        IdEstudiante = x.EstudiantesMediosTransporte.Estudiantes.Id,
                        EstudianteGUID = x.EstudiantesMediosTransporte.Estudiantes.GUID, // Asumiendo que Estudiantes tiene GUID
                        NombreEstudiante = (x.EstudiantesMediosTransporte.Estudiantes.PrimerNombre + " " +
                                           x.EstudiantesMediosTransporte.Estudiantes.SegundoNombre + " " +
                                           x.EstudiantesMediosTransporte.Estudiantes.PrimerApellido + " " +
                                           x.EstudiantesMediosTransporte.Estudiantes.SegundoApellido).Trim()
                        // Agrega más campos si son necesarios en la respuesta JSON
                    })
                    .ToList();

                return Json(new
                {
                    GUID = rutaActivaInfo.GUID, // El GUID de la ruta activa del día
                    Estado = "En ruta",
                    Ruta = detallesDeLaRuta
                });
            }
            catch (Exception ex)
            {
                // Es crucial registrar 'ex' en tu sistema de logging para diagnóstico.
                // logger.LogError(ex, "Error en VerificarRuta para idPiloto {PilotoId}", idPiloto);
                return Json(new
                {
                    error = "Ocurrió un error al verificar la ruta.",
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                });
            }
        }
        // Se recomienda usar IActionResult para mayor flexibilidad en ASP.NET Core
        public async Task<JsonResult> FinRuta(string guid)
        {
            try
            {
                // 1. Obtener todas las entidades (segmentos de ruta) que coincidan con el GUID.
                //    Usamos ToListAsync() para traerlos a memoria para la actualización.
                List<RutasMediosTransporte> rutasParaFinalizar = await db.RutasMediosTransporte
                    .Where(r => r.GUID == guid)
                    .ToListAsync();

                // Opcional: Verificar si se encontró alguna ruta para el GUID.
                if (!rutasParaFinalizar.Any())
                {
                    // Podrías devolver un NotFound o un mensaje específico.
                    // return NotFound(new { Message = $"No se encontraron rutas con GUID: {guid}" });
                    // O simplemente permitir que el resto del código no haga nada si la lista está vacía.
                }

                // 2. Modificar el estado de cada entidad encontrada.
                foreach (RutasMediosTransporte ruta in rutasParaFinalizar)
                {
                    ruta.Estado = "Finalizada";
                    // Entity Framework Core rastreará este cambio automáticamente porque 'ruta'
                    // fue obtenida del contexto 'db'. No es necesario establecer el estado manualmente.
                }

                // 3. Guardar todos los cambios en la base de datos de forma asíncrona.
                await db.SaveChangesAsync();

                // 4. Llamar al servicio externo.
                FirebaseResponse firebaseResponse = await _firebaseService.ActualizarRuta(guid, "Finalizada");
                // Aquí podrías querer verificar 'firebaseResponse' y manejar errores si es necesario.

                // 5. Devolver una respuesta JSON de éxito.
                return Json(new
                {
                    Message = "Ruta finalizada correctamente.", // Mensaje de éxito opcional
                    GUID = guid
                });
            }
            catch (Exception ex)
            {
                // Es MUY importante registrar esta excepción en tu sistema de logging.
                // logger.LogError(ex, "Error al finalizar la ruta con GUID {RutaGuid}", guid);

                // Devolver un error genérico o específico. Evita exponer StackTrace en producción.
                return Json(new
                {
                    error = "Ocurrió un error al finalizar la ruta.",
                    // detail = ex.Message // Opcional, para desarrollo
                });
                // En ASP.NET Core, para errores de servidor:
                // return StatusCode(StatusCodes.Status500InternalServerError, new { error = "..." });
            }
        }
        [HttpPost]
        // Es buena práctica usar IActionResult en ASP.NET Core para mayor flexibilidad
        public async Task<JsonResult> CrearNotificacion(string mensaje, string idEstudiante)
        {
            try
            {
                // 1. Obtener los registros de UsuariosEstudiantes de forma asíncrona.
                //    Es crucial incluir las entidades relacionadas para evitar N+1 o NullReferenceException.
                List<UsuariosEstudiantes> usuariosParaNotificar = await db.UsuariosEstudiantes
                    .Include(ue => ue.Estudiantes) // Para filtrar por ue.Estudiantes.GUID
                    .Include(ue => ue.Usuarios)    // Para acceder a ue.Usuarios.MessageToken
                    .Where(ue => ue.Estudiantes.GUID == idEstudiante)
                    .ToListAsync();

                // 2. Iterar sobre los resultados y enviar notificaciones.
                foreach (UsuariosEstudiantes usuarioEstudiante in usuariosParaNotificar)
                {
                    // Usar string.IsNullOrEmpty para una verificación más robusta y concisa del token.
                    if (!string.IsNullOrEmpty(usuarioEstudiante.Usuarios.MessageToken))
                    {
                        // El valor de retorno de EnviarNotificacion ('str' en el original) no se usaba.
                        // Si necesitas verificar el resultado del envío, puedes asignar y usar la respuesta.
                        await _firebaseService.EnviarNotificacion(mensaje, usuarioEstudiante.Usuarios.MessageToken);
                    }
                }

                // 3. Devolver una respuesta JSON de éxito.
                return Json(new
                {
                    // El mensaje podría ser más específico si algunas notificaciones no se envían
                    // (ej. si algunos tokens son nulos/vacíos).
                    mensaje = "Proceso de envío de notificaciones completado."
                });
            }
            catch (Exception ex)
            {
                // Es MUY importante registrar la excepción 'ex' en tu sistema de logging.
                // logger.LogError(ex, "Error en CrearNotificacion para idEstudiante {EstudianteId}", idEstudiante);

                // Devolver un error. Evita exponer StackTrace en producción.
                return Json(new
                {
                    error = "Ocurrió un error al enviar las notificaciones.",
                    // detail = ex.Message // Opcional, para desarrollo
                });
                // En ASP.NET Core, para errores de servidor:
                // return StatusCode(StatusCodes.Status500InternalServerError, new { error = "..." });
            }
        }
        [HttpPost]
        // Se recomienda usar IActionResult para mayor flexibilidad en ASP.NET Core
        public async Task<JsonResult> EstudianteNoAsistencia(int idEstudiante, string motivo, string fecha)
        {
            try
            {
                // 1. Obtener el registro de EstudiantesMediosTransporte de forma asíncrona.
                EstudiantesMediosTransporte estudianteMedioTransporte = await db.EstudiantesMediosTransporte
                    .FirstOrDefaultAsync(emt => emt.IdEstudiante == idEstudiante);

                // 2. Verificar si el estudiante fue encontrado en medios de transporte.
                if (estudianteMedioTransporte == null)
                {
                    return Json(new
                    {
                        Estatus = 404, // Not Found
                        Error = $"No se encontró un registro de medio de transporte para el estudiante con Id: {idEstudiante}."
                    });
                    // En ASP.NET Core, también podrías usar:
                    // return NotFound(new { Error = "..." });
                }

                // 3. Parsear la fecha de forma segura.
                DateTime fechaAusencia;
                if (!DateTime.TryParseExact(fecha, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaAusencia))
                {
                    return Json(new
                    {
                        Estatus = 400, // Bad Request
                        Error = "Formato de fecha inválido. Por favor, use el formato yyyyMMdd."
                    });
                    // En ASP.NET Core:
                    // return BadRequest(new { Error = "..." });
                }

                // 4. Crear la nueva entidad NoUsoRutasMediosTransporte.
                var nuevoRegistroNoUso = new NoUsoRutasMediosTransporte
                {
                    IdEstudianteMedioTransporte = estudianteMedioTransporte.Id, // Usar el Id del registro encontrado
                    Fecha = fechaAusencia.Date, // Asegurarse de guardar solo la fecha si es necesario
                    GUID = Guid.NewGuid().ToString(),
                    Motivo = motivo
                };

                // 5. Agregar la nueva entidad al DbSet y guardar los cambios de forma asíncrona.
                db.NoUsoRutasMediosTransporte.Add(nuevoRegistroNoUso);
                await db.SaveChangesAsync();

                // 6. Devolver una respuesta JSON de éxito.
                return Json(new
                {
                    Estatus = 201, // Created
                    Mensaje = "Inasistencia de estudiante registrada correctamente.",
                    GuidGenerado = nuevoRegistroNoUso.GUID // Opcional: devolver el GUID del nuevo registro
                });
            }
            catch (FormatException formatEx) // Capturar específicamente errores de formato de fecha
            {
                // Loguear formatEx
                return Json(new { Estatus = 400, Error = "Error en el formato de la fecha proporcionada: " + formatEx.Message });
            }
            catch (Exception ex)
            {
                // Es MUY importante registrar la excepción 'ex' completa en tu sistema de logging.
                // logger.LogError(ex, "Error en EstudianteNoAsistencia para idEstudiante {EstudianteId}", idEstudiante);

                // Devolver un error genérico. Evita exponer StackTrace en producción.
                return Json(new
                {
                    Estatus = 500, // Internal Server Error
                    Error = "Ocurrió un error al registrar la inasistencia.",
                    // Detail = ex.Message // Opcional, para desarrollo
                });
                // En ASP.NET Core, para errores de servidor:
                // return StatusCode(StatusCodes.Status500InternalServerError, new { error = "..." });
            }
        }
    }
}
