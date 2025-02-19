using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlRutas.Data;
using ControlRutas.DTO;
using ControlRutas.Security;

namespace ControlRutas.Controllers
{
    [JwtAuthorize]
    public class ApiEstablecimientosEducativosController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();
        public JsonResult GetEstablecimientos()
        {
            var establecimientos = db.EstablecimientosEducativos.Select(e => new
            {
                e.Id,
                e.GUID,
                e.Nombre,
                e.Direccion,
                e.Latitud,
                e.Longitud,
                e.Estado
            }).ToList();

            return Json(new ResponseRutasDTO
            {
                data = establecimientos,
                message = "Establecimientos cargados correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CrearEstablecimiento(EstablecimientosEducativos establecimiento)
        {
            if (ModelState.IsValid)
            {
                establecimiento.GUID = Guid.NewGuid().ToString();
                establecimiento.Estado = true;
                db.EstablecimientosEducativos.Add(establecimiento);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new ResponseRutasDTO
                    {
                        data = e.InnerException.InnerException.Message,
                        message = "Error al crear el establecimiento",
                        status = HttpStatusCode.BadRequest,
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new ResponseRutasDTO
                {
                    data = establecimiento,
                    message = "Establecimiento creado correctamente",
                    status = HttpStatusCode.OK,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new ResponseRutasDTO
            {
                data = null,
                message = "Error al crear el establecimiento",
                status = HttpStatusCode.BadRequest,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DetallesEstablecimiento(string id)
        {
            if (id == null)
            {
                return Json(HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
            }
            EstablecimientosEducativos establecimiento = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
            if (establecimiento == null)
            {
                return Json(HttpStatusCode.NotFound, JsonRequestBehavior.AllowGet);
            }
            return Json(establecimiento, JsonRequestBehavior.AllowGet);
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
