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
    public class EstudiantesMediosTransportesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();
        // GET: EstudiantesMediosTransportes
        public ActionResult Index()
        {
            var listaEstudiantesTransporte = db.EstudiantesMediosTransporte
            .Include(e => e.Estudiantes)            // Carga previa de la entidad Estudiantes
            .Include(e => e.MediosTransporte)       // Carga previa de la entidad MediosTransporte
            .ToList();


            return View(listaEstudiantesTransporte);
        }

        // GET: EstudiantesMediosTransportes/Details/5
        [HttpGet] // Es bueno ser explícito sobre el verbo HTTP
        public ActionResult Details(int? id)
        {
            // 1. Una forma más común de verificar si el id es nulo.
            if (id == null)
            {
                // El cast a (ActionResult) no es necesario.
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 2. Usar FindAsync para la operación asíncrona y una sintaxis más limpia.
            EstudiantesMediosTransporte estudianteMedioTransporte = db.EstudiantesMediosTransporte.Find(id);

            // 3. Verificar si la entidad fue encontrada.
            if (estudianteMedioTransporte == null)
            {
                // El cast a (ActionResult) no es necesario aquí tampoco.
                return HttpNotFound();
            }

            // 4. Devolver la vista con el modelo sin casts innecesarios.
            return View(estudianteMedioTransporte);
        }

        // GET: EstudiantesMediosTransportes/Create
        [HttpGet] // Es bueno ser explícito sobre el verbo HTTP para las acciones Create que muestran el formulario
        public ActionResult Create()
        {
            // 1. Poblar el ViewBag para las listas desplegables (dropdowns) en la vista.

            // Para el dropdown de Estudiantes:
            // Obtener la lista de estudiantes de forma asíncrona.
            var listaEstudiantes = db.Estudiantes.ToList();
            // Crear el SelectList y asignarlo a una propiedad de ViewBag.
            ViewBag.IdEstudiante = new SelectList(listaEstudiantes, "Id", "GUID");

            // Para el dropdown de Medios de Transporte:
            var listaTransportes = db.MediosTransporte.ToList();
            ViewBag.IdMedioTransporte = new SelectList(listaTransportes, "Id", "GUID");

            // 2. Devolver la vista. El cast a (ActionResult) es innecesario.
            return View();
        }

        // POST: EstudiantesMediosTransportes/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // ¡Bien hecho! Es crucial para la seguridad contra ataques CSRF.
        public ActionResult Create([Bind(Include = "Id,GUID,IdEstudiante,IdMedioTransporte,DireccionOrigen,LatitudOrigen,LongitudOrigen,DireccionDestino,LatitudDestino,LongitudDestino,Orden")] EstudiantesMediosTransporte estudiantesMediosTransporte)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.EstudiantesMediosTransporte.Add(estudiantesMediosTransporte);
                    db.SaveChanges(); // Guardar cambios de forma asíncrona
                    return RedirectToAction("Index"); // No se necesita cast
                }
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción y añadir un error de modelo si algo falla al guardar.
                // logger.LogError(ex, "Error al crear la relación Estudiante-MedioTransporte.");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos. Por favor, intente de nuevo.");
            }

            // --- Si llegamos aquí, el modelo no es válido o hubo un error al guardar ---

            // Repoblamos los ViewBag para que los dropdowns en la vista no aparezcan vacíos al recargar la página.
            // Sugerencia: Usar un nombre amigable en lugar de GUID para el texto del dropdown.
            var estudiantesParaDropdown = db.Estudiantes
                .Select(s => new {
                    s.Id,
                    NombreCompleto = s.PrimerNombre + " " + s.PrimerApellido
                })
                .ToList();

            var transportesParaDropdown = db.MediosTransporte
                .Select(m => new {
                    m.Id,
                    Descripcion = m.Placa + " (" + m.Identificador + ")"
                })
                .ToList();

            // Crear los SelectList, pasando el valor que el usuario ya había seleccionado para que no se pierda.
            ViewBag.IdEstudiante = new SelectList(estudiantesParaDropdown, "Id", "NombreCompleto", estudiantesMediosTransporte.IdEstudiante);
            ViewBag.IdMedioTransporte = new SelectList(transportesParaDropdown, "Id", "Descripcion", estudiantesMediosTransporte.IdMedioTransporte);

            // Devolvemos la misma vista, pasándole el modelo con los datos que el usuario ingresó
            // para que pueda corregirlos. Los errores de validación se mostrarán automáticamente.
            return View(estudiantesMediosTransporte);
        }

        [HttpGet] // Es bueno ser explícito sobre el verbo HTTP
        public ActionResult Edit(int? id)
        {
            // 1. Una forma más común de verificar si el id es nulo.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Usaremos un bloque try-catch para manejar posibles errores de base de datos.
            try
            {
                // 2. Usar FindAsync para la operación asíncrona y una sintaxis más limpia.
                EstudiantesMediosTransporte estudianteMedioTransporte = db.EstudiantesMediosTransporte.Find(id);

                // 3. Verificar si la entidad fue encontrada.
                if (estudianteMedioTransporte == null)
                {
                    return HttpNotFound();
                }

                // 4. Poblar el ViewBag para las listas desplegables, pre-seleccionando los valores actuales.
                //    SUGERENCIA: Es mucho mejor mostrar un nombre amigable que un GUID.

                // --- Para el dropdown de Estudiantes (mostrar nombre completo) ---
                var estudiantesParaDropdown = db.Estudiantes
                    .Select(s => new {
                        s.Id,
                        NombreCompleto = s.PrimerNombre + " " + s.PrimerApellido
                    })
                    .ToList();

                ViewBag.IdEstudiante = new SelectList(
                    estudiantesParaDropdown,
                    "Id",
                    "NombreCompleto",
                    estudianteMedioTransporte.IdEstudiante); // Valor seleccionado actualmente

                // --- Para el dropdown de Medios de Transporte (mostrar placa e identificador) ---
                var transportesParaDropdown = db.MediosTransporte
                    .Select(m => new {
                        m.Id,
                        Descripcion = m.Placa + " (" + m.Identificador + ")"
                    })
                    .ToList();

                ViewBag.IdMedioTransporte = new SelectList(
                    transportesParaDropdown,
                    "Id",
                    "Descripcion",
                    estudianteMedioTransporte.IdMedioTransporte); // Valor seleccionado actualmente

                // 5. Devolver la vista con el modelo a editar.
                return View(estudianteMedioTransporte);
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'
                // Devolver una vista de error.
                return View("Error", new HandleErrorInfo(ex, "NombreDelControlador", "Edit"));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // ¡Bien hecho! Es crucial para la seguridad.
        public ActionResult Edit([Bind(Include = "Id,GUID,IdEstudiante,IdMedioTransporte,DireccionOrigen,LatitudOrigen,LongitudOrigen,DireccionDestino,LatitudDestino,LongitudDestino,Orden,Lunes,Martes,Miercoles,Jueves,Viernes,Sabado,Domingo,Activo")] EstudiantesMediosTransporte estudiantesMediosTransporte)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // 1. Establecer el estado de la entidad a 'Modified'.
                    //    Esto es correcto porque 'estudiantesMediosTransporte' viene del formulario
                    //    y no está siendo rastreado por el DbContext.
                    db.Entry(estudiantesMediosTransporte).State = EntityState.Modified; // Usar el enum es más legible que el número 16.

                    // 2. Guardar los cambios de forma asíncrona.
                    db.SaveChanges();

                    // 3. Redirigir a otra acción de forma simplificada.
                    return RedirectToAction("Details", "Estudiantes", new { id = estudiantesMediosTransporte.IdEstudiante });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Opcional: Manejar específicamente conflictos de concurrencia.
                // logger.LogWarning(ex, "Conflicto de concurrencia al editar EstudiantesMediosTransporte.");
                ModelState.AddModelError("", "El registro fue modificado por otro usuario mientras usted editaba. Por favor, recargue la página.");
            }
            catch (Exception ex)
            {
                // Es buena práctica loguear la excepción y añadir un error de modelo si algo falla al guardar.
                // logger.LogError(ex, "Error al editar la relación Estudiante-MedioTransporte.");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos. Por favor, intente de nuevo.");
            }

            // --- Si llegamos aquí, el modelo no es válido o hubo un error al guardar ---

            // Repoblamos los ViewBag para que los dropdowns en la vista no aparezcan vacíos al recargar la página.
            // Usar un nombre amigable en lugar de GUID es altamente recomendado.
            var estudiantesParaDropdown = db.Estudiantes
                .Select(s => new {
                    s.Id,
                    NombreCompleto = s.PrimerNombre + " " + s.PrimerApellido
                })
                .ToList();

            var transportesParaDropdown = db.MediosTransporte
                .Select(m => new {
                    m.Id,
                    Descripcion = m.Placa + " (" + m.Identificador + ")"
                })
                .ToList();

            // Crear los SelectList, pasando el valor que el usuario ya había seleccionado para que no se pierda.
            ViewBag.IdEstudiante = new SelectList(estudiantesParaDropdown, "Id", "NombreCompleto", estudiantesMediosTransporte.IdEstudiante);
            ViewBag.IdMedioTransporte = new SelectList(transportesParaDropdown, "Id", "Descripcion", estudiantesMediosTransporte.IdMedioTransporte);

            // Devolvemos la misma vista con el modelo con errores para que el usuario pueda corregirlos.
            return View(estudiantesMediosTransporte);
        }

        // GET: EstudiantesMediosTransportes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EstudiantesMediosTransportes/Delete/5
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inactivar(int? id)
        {
            // 3. Verificación de ID simplificada.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // 4. Usar FindAsync para operaciones asíncronas y sintaxis limpia.
                EstudiantesMediosTransporte estudianteMedioTransporte = db.EstudiantesMediosTransporte.Find(id);

                if (estudianteMedioTransporte == null)
                {
                    return HttpNotFound();
                }

                // 5. Modificar la propiedad de la entidad.
                estudianteMedioTransporte.Activo = false;

                // 6. ¡IMPORTANTE! No es necesario establecer el estado de la entidad a 'Modified'.
                //    Entity Framework ya está rastreando la entidad 'estudianteMedioTransporte'
                //    porque fue cargada desde el mismo contexto 'db' con 'FindAsync'.
                //    Detectará automáticamente que la propiedad 'Activo' ha cambiado.
                //
                //    db.Entry(estudianteMedioTransporte).State = EntityState.Modified; // <- Esta línea es redundante aquí.

                // 7. Guardar cambios de forma asíncrona.
                db.SaveChanges();

                // 8. Redirección simplificada.
                return RedirectToAction("Details", "Estudiantes", new { id = estudianteMedioTransporte.IdEstudiante });
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex'.
                // Devolver una vista de error o redirigir con un mensaje de error.
                return View("Error", new HandleErrorInfo(ex, "ControllerName", "Inactivar"));
            }
        }
        // 1. Usar [HttpPost] para acciones que modifican datos.
        [HttpPost]
        // 2. Usar [ValidateAntiForgeryToken] para proteger contra ataques CSRF.
        [ValidateAntiForgeryToken]
        public ActionResult Activar(int? id)
        {
            // 3. Verificación de ID simplificada.
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // 4. Usar FindAsync para operaciones asíncronas y sintaxis limpia.
                EstudiantesMediosTransporte estudianteMedioTransporte = db.EstudiantesMediosTransporte.Find(id);

                if (estudianteMedioTransporte == null)
                {
                    return HttpNotFound();
                }

                // 5. Modificar la propiedad de la entidad.
                estudianteMedioTransporte.Activo = true;

                // 6. ¡IMPORTANTE! No es necesario establecer el estado de la entidad a 'Modified'.
                //    Entity Framework ya está rastreando la entidad 'estudianteMedioTransporte'
                //    porque fue cargada desde este mismo contexto 'db'. El cambio en 'Activo'
                //    se detectará automáticamente.

                //    db.Entry(estudianteMedioTransporte).State = EntityState.Modified; // <- Esta línea es redundante.

                // 7. Guardar cambios de forma asíncrona.
                db.SaveChanges();

                // 8. Redirección simplificada.
                return RedirectToAction("Details", "Estudiantes", new { id = estudianteMedioTransporte.IdEstudiante });
            }
            catch (Exception ex)
            {
                // Registrar la excepción 'ex' en un sistema de logging.
                // logger.LogError(ex, "Error al activar la relación para el id {Id}", id);

                // Devolver una vista de error.
                return View("Error", new HandleErrorInfo(ex, "ControllerName", "Activar"));
            }
        }
    }
}
