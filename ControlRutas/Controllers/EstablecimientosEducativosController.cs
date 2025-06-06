﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlRutas.Data;

namespace ControlRutas.Controllers
{
    public class EstablecimientosEducativosController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        // GET: EstablecimientosEducativos
        public ActionResult Index()
        {
            return View(db.EstablecimientosEducativos.ToList());
        }

        // GET: EstablecimientosEducativos/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
            if (establecimientosEducativos == null)
            {
                return HttpNotFound();
            }
            return View(establecimientosEducativos);
        }

        // GET: EstablecimientosEducativos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstablecimientosEducativos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombre,Direccion,Latitud,Longitud,Estado")] EstablecimientosEducativos establecimientosEducativos, HttpPostedFileBase Foto)
        {
            if (ModelState.IsValid)
            {
                if (Foto != null && Foto.ContentLength > 0)
                {
                    // Leer el contenido del archivo
                    using (var binaryReader = new BinaryReader(Foto.InputStream))
                    {
                        byte[] fileBytes = binaryReader.ReadBytes(Foto.ContentLength);

                        // Convertir a Base64
                        string base64String = Convert.ToBase64String(fileBytes);

                        // Guardar el Base64 en el modelo
                        establecimientosEducativos.Foto = base64String;
                    }
                }
                establecimientosEducativos.GUID = Guid.NewGuid().ToString();
                establecimientosEducativos.Estado = true;
                db.EstablecimientosEducativos.Add(establecimientosEducativos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(establecimientosEducativos);
        }

        // GET: EstablecimientosEducativos/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
            if (establecimientosEducativos == null)
            {
                return HttpNotFound();
            }
            return View(establecimientosEducativos);
        }

        // POST: EstablecimientosEducativos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,GUID,Nombre,Direccion,Latitud,Longitud,Estado")] EstablecimientosEducativos establecimientosEducativos, HttpPostedFileBase Foto)
        {
            if (ModelState.IsValid)
            {
                if (Foto != null && Foto.ContentLength > 0)
                {
                    // Leer el contenido del archivo
                    using (var binaryReader = new BinaryReader(Foto.InputStream))
                    {
                        byte[] fileBytes = binaryReader.ReadBytes(Foto.ContentLength);

                        // Convertir a Base64
                        string base64String = Convert.ToBase64String(fileBytes);

                        // Guardar el Base64 en el modelo
                        establecimientosEducativos.Foto = base64String;
                    }
                    db.Entry(establecimientosEducativos).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "La Foto es requerida.");
                }
            }
            else
            {
                ModelState.AddModelError("", "No se ha podido guardar los cambios.");
            }
            return View(establecimientosEducativos);
        }

        // GET: EstablecimientosEducativos/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
            if (establecimientosEducativos == null)
            {
                return HttpNotFound();
            }
            return View(establecimientosEducativos);
        }

        // POST: EstablecimientosEducativos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            EstablecimientosEducativos establecimientosEducativos = db.EstablecimientosEducativos.FirstOrDefault(x => x.GUID == id);
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
