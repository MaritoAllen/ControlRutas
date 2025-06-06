using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlRutas.Controllers
{
    public class MediosTransportesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();
        // GET: MediosTransportes
        public ActionResult Index()
        {
            return View(db.MediosTransporte.ToList());
        }

        [HttpGet] // Es bueno ser explícito sobre el verbo HTTP
        public async Task<ActionResult> Details(int? id)
        {
            // 1. Una forma más común y legible de verificar si el id es nulo.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // 2. Usar FindAsync para la operación asíncrona y una sintaxis más limpia.
                MediosTransporte medioTransporte = await db.MediosTransporte.FindAsync(id);

                // 3. Verificar si la entidad fue encontrada.
                if (medioTransporte == null)
                {
                    // El cast a (ActionResult) no es necesario.
                    return HttpNotFound();
                }

                // 4. Devolver la vista con el modelo sin casts innecesarios.
                return View(medioTransporte);
            }
            catch (Exception ex)
            {
                // Es buena práctica registrar la excepción en un sistema de logging.
                // logger.LogError(ex, "Error al obtener los detalles del Medio de Transporte con id {Id}", id);

                // Redirigir a una vista de error.
                return View("Error", new HandleErrorInfo(ex, "MediosTransportes", "Details"));
            }
        }

        [HttpGet] // Es bueno ser explícito sobre el verbo HTTP
        public ActionResult Create()
        {
            try
            {
                // 1. Poblar el ViewBag para las 3 listas desplegables (dropdowns) de la vista Create.

                // --- Para el dropdown de Establecimientos Educativos ---
                // Se obtiene la lista y se proyecta a un tipo anónimo para usar el Nombre en el dropdown.
                var listaEstablecimientos = db.EstablecimientosEducativos
                    .Select(e => new { e.Id, e.Nombre })
                    .ToList();
                ViewBag.IdCodigoEstablecimiento = new SelectList(listaEstablecimientos, "Id", "Nombre");

                // --- Para el dropdown de Tipos de Medios de Transporte ---
                var listaTiposTransporte = db.TiposMediosTransporte
                    .Select(t => new { t.Id, t.Nombre })
                    .ToList();
                ViewBag.IdTipoMedioTransporte = new SelectList(listaTiposTransporte, "Id", "Nombre");

                // --- Para el dropdown de Pilotos (filtrando solo usuarios que son pilotos) ---
                // Asumimos que los pilotos tienen IdTipoUsuario == 4. Ajusta este número si es diferente.
                const int idTipoUsuarioPiloto = 4;
                var listaPilotos = db.Usuarios
                    .Where(u => u.IdTipoUsuario == idTipoUsuarioPiloto)
                    .ToList(); // Se trae la lista a memoria para poder concatenar el nombre.

                // Se proyecta a un tipo anónimo con el nombre completo para el dropdown.
                var pilotosParaDropdown = listaPilotos
                    .Select(p => new {
                        Id = p.Id,
                        NombreCompleto = $"{p.PrimerNombre} {p.PrimerApellido}".Trim()
                    });
                ViewBag.IdCodigoPiloto = new SelectList(pilotosParaDropdown, "Id", "NombreCompleto");

                // 2. Devolver la vista Create.
                return View();
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex' en un sistema de logging.
                // logger.LogError(ex, "Error al preparar la vista Create de MediosTransportes.");

                // Devolver una vista de error.
                return View("Error", new HandleErrorInfo(ex, "MediosTransportes", "Create"));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ¡Bien hecho! Es crucial para la seguridad.
        public async Task<ActionResult> Create([Bind(Include = "Id,GUID,IdCodigoPiloto,IdCodigoDueño,IdCodigoEstablecimiento,IdTipoMedioTransporte,Identificador,Placa,Color,Estado")] MediosTransporte mediosTransporte)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Asignar un nuevo GUID antes de guardar.
                    mediosTransporte.GUID = Guid.NewGuid().ToString();

                    db.MediosTransporte.Add(mediosTransporte);
                    await db.SaveChangesAsync(); // Guardar cambios de forma asíncrona.
                    return RedirectToAction("Index"); // No se necesita cast.
                }
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción y añadir un error de modelo si algo falla al guardar.
                // logger.LogError(ex, "Error al crear el Medio de Transporte.");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos. Por favor, intente de nuevo.");
            }

            // --- Si llegamos aquí, el modelo no es válido o hubo un error al guardar ---

            // Llamamos a un método auxiliar para repoblar los dropdowns.
            await PopulateDropdowns(mediosTransporte);

            // Devolvemos la misma vista con el modelo con errores para que el usuario pueda corregirlos.
            return View(mediosTransporte);
        }

        // --- MÉTODO AUXILIAR PRIVADO ---
        // Este método se encarga de poblar los ViewBag, puede ser reutilizado en las acciones Create [GET] y Edit.
        private async Task PopulateDropdowns(MediosTransporte model = null)
        {
            // --- Para Establecimientos ---
            var establecimientos = await db.EstablecimientosEducativos
                .Select(e => new { e.Id, e.Nombre })
                .ToListAsync();
            // El último parámetro (model?.IdCodigoEstablecimiento) pre-selecciona el valor si el modelo no es nulo.
            ViewBag.IdCodigoEstablecimiento = new SelectList(establecimientos, "Id", "Nombre", model?.IdCodigoEstablecimiento);

            // --- Para Tipos de Transporte ---
            var tiposDeTransporte = await db.TiposMediosTransporte
                .Select(t => new { t.Id, t.Nombre })
                .ToListAsync();
            ViewBag.IdTipoMedioTransporte = new SelectList(tiposDeTransporte, "Id", "Nombre", model?.IdTipoMedioTransporte);

            // --- Para Pilotos (filtrando y mostrando nombre amigable) ---
            const int idTipoUsuarioPiloto = 4; // Asumiendo que 4 es el Id para el tipo "Piloto"
            var pilotos = await db.Usuarios
                .Where(u => u.IdTipoUsuario == idTipoUsuarioPiloto)
                .ToListAsync();

            var pilotosParaDropdown = pilotos
                 .Select(p => new {
                     Id = p.Id,
                     NombreCompleto = $"{p.PrimerNombre} {p.PrimerApellido}".Trim()
                 });
            ViewBag.IdCodigoPiloto = new SelectList(pilotosParaDropdown, "Id", "NombreCompleto", model?.IdCodigoPiloto);
        }

        // GET: MediosTransportes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                MediosTransporte medioTransporte = await db.MediosTransporte.FindAsync(id);

                if (medioTransporte == null)
                {
                    return HttpNotFound();
                }

                // 1. Llamar al método auxiliar para poblar todos los dropdowns.
                //    Le pasamos el modelo 'medioTransporte' para que se pre-seleccionen los valores correctos.
                await PopulateDropdowns(medioTransporte);

                // 2. Devolver la vista con el modelo a editar.
                return View(medioTransporte);
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'
                return View("Error", new HandleErrorInfo(ex, "MediosTransportes", "Edit"));
            }
        }

        // POST: MediosTransportes/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "Id,GUID,IdCodigoPiloto,IdCodigoDueño,IdCodigoEstablecimiento,IdTipoMedioTransporte,Identificador,Placa,Color,Estado")] MediosTransporte mediosTransporte)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // 1. Establecer el estado de la entidad a 'Modified'.
                    //    Esto es correcto para entidades que vienen de un formulario ("desconectadas").
                    db.Entry(mediosTransporte).State = EntityState.Modified;

                    // 2. Guardar los cambios de forma asíncrona.
                    await db.SaveChangesAsync();

                    // 3. Redirigir a la acción Index.
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Opcional: Manejar específicamente conflictos de concurrencia.
                // logger.LogWarning(ex, "Conflicto de concurrencia al editar Medio de Transporte con Id {Id}.", mediosTransporte.Id);
                ModelState.AddModelError("", "Este registro fue modificado por otro usuario. Por favor, recargue la página e intente de nuevo.");
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción y añadir un error de modelo si algo falla al guardar.
                // logger.LogError(ex, "Error al editar Medio de Transporte con Id {Id}.", mediosTransporte.Id);
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos. Por favor, intente de nuevo.");
            }

            // --- Si llegamos aquí, el modelo no es válido o hubo un error al guardar ---

            // 4. Llamamos al mismo método auxiliar para repoblar los dropdowns.
            //    Le pasamos el modelo 'mediosTransporte' para que se pre-seleccionen los valores que el usuario había enviado.
            await PopulateDropdowns(mediosTransporte);

            // 5. Devolvemos la misma vista con el modelo con errores para que el usuario pueda corregirlos.
            return View(mediosTransporte);
        }

        // GET: MediosTransportes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MediosTransportes/Delete/5
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
        public async Task<ActionResult> AsignarTransporte(int? id) // 1. Usar int? para manejar IDs nulos
        {
            // 2. Verificar si se proporcionó un ID
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // 3. Usar FindAsync para operaciones asíncronas y sintaxis limpia
                Usuarios usuario = await db.Usuarios.FindAsync(id);

                // 4. Verificar si el usuario fue encontrado en la base de datos
                if (usuario == null)
                {
                    return HttpNotFound(); // Devuelve un error 404 Not Found si no se encuentra
                }

                // --- Sugerencia: Poblar un ViewBag para un dropdown de transportes ---
                // Si tu vista 'AsignarTransporte' tiene una lista desplegable para seleccionar
                // un transporte, aquí es donde deberías cargar los datos para ella.
                var transportesDisponibles = await db.MediosTransporte
                    .Select(m => new { m.Id, Descripcion = m.Placa + " (" + m.Identificador + ")" })
                    .ToListAsync();

                ViewBag.IdMedioTransporte = new SelectList(transportesDisponibles, "Id", "Descripcion");
                // --- Fin de la sugerencia ---

                // 5. Devolver la vista con el modelo de usuario encontrado
                return View(usuario);
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'
                return View("Error", new HandleErrorInfo(ex, "NombreDelControlador", "AsignarTransporte"));
            }
        }
        [HttpPost]
        // 1. Añadir [ValidateAntiForgeryToken] para proteger contra ataques CSRF.
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AsignarTransporte(int idPiloto, int idTransporte) // 2. Nombres de parámetros más claros.
        {
            try
            {
                // 3. Usar FindAsync para operaciones asíncronas y sintaxis limpia.
                MediosTransporte medioTransporte = await db.MediosTransporte.FindAsync(idTransporte);

                // 4. ¡CRUCIAL! Verificar si el medio de transporte fue encontrado.
                if (medioTransporte == null)
                {
                    // Si no se encuentra, devuelve un error 404 Not Found.
                    return HttpNotFound("El medio de transporte especificado no fue encontrado.");
                }

                // 5. Opcional pero recomendado: verificar si el piloto también existe.
                var pilotoExiste = await db.Usuarios.AnyAsync(u => u.Id == idPiloto);
                if (!pilotoExiste)
                {
                    return HttpNotFound($"El piloto con ID {idPiloto} no fue encontrado.");
                }

                // 6. Asignar el piloto al transporte.
                medioTransporte.IdCodigoPiloto = idPiloto;

                // Como 'medioTransporte' fue cargado desde el contexto 'db',
                // Entity Framework rastreará el cambio automáticamente.
                // No es necesario establecer el estado a 'Modified' manualmente.

                // 7. Guardar cambios de forma asíncrona.
                await db.SaveChangesAsync();

                // 8. Redirección simplificada.
                return RedirectToAction("Details", "Usuarios", new { id = idPiloto });
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex' en un sistema de logging.
                // logger.LogError(ex, "Error al asignar transporte {IdTransporte} al piloto {IdPiloto}", idTransporte, idPiloto);

                // Devolver una vista de error.
                return View("Error", new HandleErrorInfo(ex, "NombreDelControlador", "AsignarTransporte"));
            }
        }
        [HttpGet]
        public async Task<ActionResult> CambiarOrdenRuta(int? id) // Usar int? para manejar IDs nulos
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Obtener la lista de estudiantes para el transporte, ordenados por su orden actual
                var model = await db.EstudiantesMediosTransporte
                    .Where(e => e.IdMedioTransporte == id)
                    .OrderBy(e => e.Orden) // Ordenar por el orden actual para mostrar en la UI
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'
                return View("Error", new HandleErrorInfo(ex, "NombreDelControlador", "CambiarOrdenRuta"));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Añadido para seguridad
        public async Task<JsonResult> CambiarOrdenRuta(List<EstudiantesMediosTransporte> ordenes)
        {
            // 1. Validación básica de la entrada
            if (ordenes == null || !ordenes.Any())
            {
                return Json(new { success = false, message = "No se proporcionaron datos para actualizar." });
            }

            try
            {
                // 2. Optimización N+1: Obtener todos los IDs de la lista recibida
                var idsParaActualizar = ordenes.Select(o => o.Id).ToList();

                // 3. Realizar UNA SOLA consulta a la base de datos para traer todas las entidades necesarias
                var entidadesDesdeDb = await db.EstudiantesMediosTransporte
                    .Where(e => idsParaActualizar.Contains(e.Id))
                    .ToDictionaryAsync(e => e.Id); // Un diccionario para búsqueda ultra rápida (O(1))

                // 4. Actualizar las entidades en memoria
                foreach (var ordenRecibido in ordenes)
                {
                    // Buscar la entidad en el diccionario (muy rápido)
                    if (entidadesDesdeDb.TryGetValue(ordenRecibido.Id, out var entidadParaActualizar))
                    {
                        entidadParaActualizar.Orden = ordenRecibido.Orden;
                        // No es necesario establecer el estado a 'Modified', el rastreador de cambios lo detecta.
                    }
                    // Opcional: puedes loguear si un 'ordenRecibido.Id' no se encontró en la base de datos.
                }

                // 5. Guardar todos los cambios en una sola transacción
                await db.SaveChangesAsync();

                // 6. Devolver una respuesta de éxito
                return Json(new { success = true, message = "Orden actualizado correctamente." });
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'
                return Json(new { success = false, message = "Ocurrió un error al actualizar el orden." });
            }
        }
    }
}
