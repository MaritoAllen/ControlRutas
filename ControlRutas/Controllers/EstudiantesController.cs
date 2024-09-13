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
    }
}
