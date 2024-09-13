using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlRutas.Data;
using ControlRutas.Security;

namespace ControlRutas.Controllers
{
    [JwtAuthorize]
    public class APIEstudiantesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: APIEstudiantes
        public JsonResult Index()
        {
            var estudiantes = db.Estudiantes.Include(e => e.EstablecimientosEducativos);
            return Json(estudiantes.ToList(), JsonRequestBehavior.AllowGet);
        }

        // GET: APIEstudiantes/Details/5
        public JsonResult Details(string id)
        {
            if (id == null)
            {
                return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
            }
            Estudiantes estudiantes = db.Estudiantes.FirstOrDefault(x=> x.GUID == id);
            if (estudiantes == null)
            {
                return Json(HttpStatusCode.NotFound, JsonRequestBehavior.AllowGet);
            }
            return Json(estudiantes, JsonRequestBehavior.AllowGet);
        }

        // POST: APIEstudiantes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind(Include = "Id,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,IdEstablecimientoEducativo")] Estudiantes estudiantes)
        {
            if (ModelState.IsValid)
            {
                estudiantes.GUID = Guid.NewGuid().ToString();
                estudiantes.Estado = true;
                db.Estudiantes.Add(estudiantes);
                db.SaveChanges();
                return Json(estudiantes, JsonRequestBehavior.AllowGet);
            }

            return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
        }

        // POST: APIEstudiantes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PrimerNombre,SegundoNombre,PrimerApellido,SegundoApellido,IdEstablecimientoEducativo,Estado")] Estudiantes estudiantes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estudiantes).State = EntityState.Modified;
                db.SaveChanges();
                return Json(estudiantes, JsonRequestBehavior.AllowGet);
            }
            ViewBag.IdEstablecimientoEducativo = new SelectList(db.EstablecimientosEducativos, "Id", "GUID", estudiantes.IdEstablecimientoEducativo);
            return Json( new { HttpStatusCode.Conflict, estudiantes }, JsonRequestBehavior.AllowGet);
        }

        // POST: APIEstudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Estudiantes estudiantes = db.Estudiantes.FirstOrDefault(x=> x.GUID == id);
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
