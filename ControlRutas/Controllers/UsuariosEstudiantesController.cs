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
    public class UsuariosEstudiantesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: UsuariosEstudiantes
        public ActionResult Index()
        {
            var usuariosEstudiantes = db.UsuariosEstudiantes.Include(u => u.Estudiantes).Include(u => u.Usuarios);
            return View(usuariosEstudiantes.ToList());
        }

        // GET: UsuariosEstudiantes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UsuariosEstudiantes usuariosEstudiantes = db.UsuariosEstudiantes.Find(id);
            if (usuariosEstudiantes == null)
            {
                return HttpNotFound();
            }
            return View(usuariosEstudiantes);
        }

        // GET: UsuariosEstudiantes/Create
        public ActionResult Create(int id)
        {
            return View(db.Usuarios.FirstOrDefault(x=> x.Id == id));
        }

        // POST: UsuariosEstudiantes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, int idEstudiante)
        {
            UsuariosEstudiantes usuariosEstudiantes = new UsuariosEstudiantes()
            {
                GUID = Guid.NewGuid().ToString(),
                IdEstudiante = idEstudiante,
                IdUsuario = id
            };

            db.UsuariosEstudiantes.Add(usuariosEstudiantes);

            db.SaveChanges();

            return RedirectToAction("Details", "Usuarios", new { id });
        }

        // GET: UsuariosEstudiantes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UsuariosEstudiantes usuariosEstudiantes = db.UsuariosEstudiantes.Find(id);
            if (usuariosEstudiantes == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdEstudiante = new SelectList(db.Estudiantes, "Id", "PrimerNombre", usuariosEstudiantes.IdEstudiante);
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "Id", "PrimerNombre", usuariosEstudiantes.IdUsuario);
            return View(usuariosEstudiantes);
        }

        // POST: UsuariosEstudiantes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,GUID,IdUsuario,IdEstudiante")] UsuariosEstudiantes usuariosEstudiantes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(usuariosEstudiantes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdEstudiante = new SelectList(db.Estudiantes, "Id", "PrimerNombre", usuariosEstudiantes.IdEstudiante);
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "Id", "PrimerNombre", usuariosEstudiantes.IdUsuario);
            return View(usuariosEstudiantes);
        }

        // GET: UsuariosEstudiantes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UsuariosEstudiantes usuariosEstudiantes = db.UsuariosEstudiantes.Find(id);
            if (usuariosEstudiantes == null)
            {
                return HttpNotFound();
            }
            return View(usuariosEstudiantes);
        }

        // POST: UsuariosEstudiantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UsuariosEstudiantes usuariosEstudiantes = db.UsuariosEstudiantes.Find(id);
            db.UsuariosEstudiantes.Remove(usuariosEstudiantes);
            db.SaveChanges();
            return RedirectToAction("Details", "Usuarios", new { id = usuariosEstudiantes.IdUsuario });
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
