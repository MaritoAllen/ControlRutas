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
    public class ApiEstablecimientosEducativosController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: ApiEstablecimientosEducativos
        public JsonResult Index()
        {
            return Json(db.EstablecimientosEducativos.ToList(), JsonRequestBehavior.AllowGet);
        }

        // GET: ApiEstablecimientosEducativos/Details/5
        public JsonResult Details(string id)
        {
            if (id == null)
            {
                return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
            }
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
            if (establecimientosEducativos == null)
            {
                return Json(HttpStatusCode.NotFound, JsonRequestBehavior.AllowGet);
            }
            return Json(establecimientosEducativos, JsonRequestBehavior.AllowGet);
        }

        // POST: ApiEstablecimientosEducativos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind(Include = "Id,Nombre,Direccion,Latitud,Longitud")] EstablecimientosEducativos establecimientosEducativos)
        {
            if (ModelState.IsValid)
            {
                establecimientosEducativos.GUID = Guid.NewGuid().ToString();
                establecimientosEducativos.Estado = true;
                db.EstablecimientosEducativos.Add(establecimientosEducativos);
                db.SaveChanges();
                return Json("Index", JsonRequestBehavior.AllowGet);
            }

            return Json(establecimientosEducativos, JsonRequestBehavior.AllowGet);
        }

        // POST: ApiEstablecimientosEducativos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit([Bind(Include = "Id,Nombre,Direccion,Latitud,Longitud,Estado")] EstablecimientosEducativos establecimientosEducativos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(establecimientosEducativos).State = EntityState.Modified;
                db.SaveChanges();
                return Json(establecimientosEducativos, JsonRequestBehavior.AllowGet);
            }
            return Json(new { HttpStatusCode.Conflict, establecimientosEducativos }, JsonRequestBehavior.AllowGet); 
        }

        // GET: ApiEstablecimientosEducativos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.Find(id);
            if (establecimientosEducativos == null)
            {
                return HttpNotFound();
            }
            return View(establecimientosEducativos);
        }

        // POST: ApiEstablecimientosEducativos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.Find(id);
            db.EstablecimientosEducativos.Remove(establecimientosEducativos);
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
