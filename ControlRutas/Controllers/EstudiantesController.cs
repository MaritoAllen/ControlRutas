using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlRutas.Data;

namespace ControlRutas.Controllers
{
    public class EstudiantesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: Estudiantes
        public ActionResult Index()
        {
            var estudiantes = db.Estudiantes.Include(e => e.EstablecimientosEducativos);
            return View(estudiantes.ToList());
        }

        // GET: Estudiantes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Estudiantes estudiantes = db.Estudiantes.Find(id);
            if (estudiantes == null)
            {
                return HttpNotFound();
            }
            return View(estudiantes);
        }

        // GET: Estudiantes/Create
        public ActionResult Create()
        {
            ViewBag.IdEstablecimientoEducativo = new SelectList(db.EstablecimientosEducativos, "Id", "GUID");
            return View();
        }

        // POST: Estudiantes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,IdEstablecimientoEducativo")] Estudiantes estudiantes)
        {
            if (ModelState.IsValid)
            {
                estudiantes.GUID = Guid.NewGuid().ToString();
                estudiantes.Estado = true;
                db.Estudiantes.Add(estudiantes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                string errors = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

                ViewBag.Error = errors;
            }

            ViewBag.IdEstablecimientoEducativo = new SelectList(db.EstablecimientosEducativos, "Id", "GUID", estudiantes.IdEstablecimientoEducativo);
            return View(estudiantes);
        }

        // GET: Estudiantes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Estudiantes estudiantes = db.Estudiantes.Find(id);
            if (estudiantes == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdEstablecimientoEducativo = new SelectList(db.EstablecimientosEducativos, "Id", "GUID", estudiantes.IdEstablecimientoEducativo);
            return View(estudiantes);
        }

        // POST: Estudiantes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,GUID,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,IdEstablecimientoEducativo,Estado")] Estudiantes estudiantes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estudiantes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdEstablecimientoEducativo = new SelectList(db.EstablecimientosEducativos, "Id", "GUID", estudiantes.IdEstablecimientoEducativo);
            return View(estudiantes);
        }

        // GET: Estudiantes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Estudiantes estudiantes = db.Estudiantes.Find(id);
            if (estudiantes == null)
            {
                return HttpNotFound();
            }
            return View(estudiantes);
        }

        // POST: Estudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Estudiantes estudiantes = db.Estudiantes.Find(id);
            db.Estudiantes.Remove(estudiantes);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public ActionResult AsignarEstudianteTransporte(EstudiantesMediosTransporte estudiantesMediosTransporte)
        {
            if (ModelState.IsValid)
            {
                Estudiantes estudiantes = db.Estudiantes.Find(estudiantesMediosTransporte.IdEstudiante);
                EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.Find(estudiantes.IdEstablecimientoEducativo);
                estudiantesMediosTransporte.GUID = Guid.NewGuid().ToString();
                estudiantesMediosTransporte.DireccionOrigen = establecimientosEducativos.Direccion;
                estudiantesMediosTransporte.LatitudOrigen = establecimientosEducativos.Latitud;
                estudiantesMediosTransporte.LongitudOrigen = establecimientosEducativos.Longitud;
                estudiantesMediosTransporte.Activo = true;
                estudiantesMediosTransporte.Orden = db.EstudiantesMediosTransporte.Where(e => e.IdEstudiante == estudiantes.Id && e.IdMedioTransporte == estudiantesMediosTransporte.IdMedioTransporte).Count() + 1;
                db.EstudiantesMediosTransporte.Add(estudiantesMediosTransporte);

                //RutasMediosTransporte rutasMediosTransporte = new RutasMediosTransporte()
                //{
                //    GUID = Guid.NewGuid().ToString(),
                //    IdEstudianteMedioTransporte = estudiantesMediosTransporte.Id,
                //    Fecha = DateTime.Now,
                //    Grado = "",
                //    DireccionOrigen = establecimientosEducativos.Direccion,
                //    LatitudOrigen = establecimientosEducativos.Latitud,
                //    LongitudOrigen = establecimientosEducativos.Longitud,
                //    DireccionDestino = estudiantesMediosTransporte.DireccionDestino,
                //    LatitudDestino = estudiantesMediosTransporte.LatitudDestino,
                //    LongitudDestino = estudiantesMediosTransporte.LongitudDestino,
                //    Orden = db.RutasMediosTransporte.Where(r => r.IdEstudianteMedioTransporte == estudiantesMediosTransporte.Id).Count() + 1,
                //    HoraSalida = DateTime.Now,
                //    HoraLlegada = DateTime.Now,
                //};

                //db.RutasMediosTransporte.Add(rutasMediosTransporte);

                db.SaveChanges();
                return RedirectToAction("Index");
            }else
            {
                string errors = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

                ViewBag.Error = errors;
            }

            return View("Details", new { id = estudiantesMediosTransporte.IdEstudiante });
        }

        [HttpGet]
        public ActionResult Inasistencia(string id)
        {
            Estudiantes estudiantes = db.Estudiantes.Where(e => e.GUID == id).FirstOrDefault();
            if (estudiantes == null)
            {
                return HttpNotFound();
            }

            EstudiantesMediosTransporte estudiantesMediosTransporte = db.EstudiantesMediosTransporte.Where(e => e.IdEstudiante == estudiantes.Id).FirstOrDefault();

            ViewBag.IdEstudianteMedioTransporte = estudiantesMediosTransporte.Id;

            return View();
        }

        [HttpPost]
        public ActionResult Inasistencia(string id, [Bind(Include = "Id,motivo,fecha,IdEstudianteMedioTransporte")] NoUsoRutasMediosTransporte inasistencia)
        {
            Estudiantes estudiantes = db.Estudiantes.Where(e => e.GUID == id).FirstOrDefault();
            if (estudiantes == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(inasistencia.Motivo))
            {
                ModelState.AddModelError("motivo", "El motivo es requerido.");
                return View(estudiantes);
            }

            if (db.NoUsoRutasMediosTransporte.Where(n => n.EstudiantesMediosTransporte.IdEstudiante == estudiantes.Id && n.Fecha == inasistencia.Fecha).Any())
            {
                ModelState.AddModelError("motivo", "Ya se ha registrado una inasistencia para este estudiante.");
                return View(estudiantes);
            }

            EstudiantesMediosTransporte estudiantesMediosTransporte = db.EstudiantesMediosTransporte.Where(e => e.IdEstudiante == estudiantes.Id).FirstOrDefault();

            inasistencia.GUID = Guid.NewGuid().ToString();

            db.NoUsoRutasMediosTransporte.Add(inasistencia);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = estudiantes.Id });
        }
    }
}
