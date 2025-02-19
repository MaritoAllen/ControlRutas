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
    public class APIEstudiantesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        [HttpGet]
        public JsonResult GetEstudiantes()
        {
            var estudiantes = db.Estudiantes.Select(e => new
            {
                Id = e.Id,
                GUID = e.GUID,
                PrimerNombre = e.PrimerNombre,
                SegundoNombre = e.SegundoNombre,
                PrimerApellido = e.PrimerApellido,
                SegundoApellido = e.SegundoApellido,
                IdEstablecimiento = e.IdEstablecimientoEducativo,
                Establecimiento = e.EstablecimientosEducativos.Nombre,
                Estado = e.Estado
            }).ToList();

            return Json(new ResponseRutasDTO
            {
                data = estudiantes,
                message = "Estudiantes cargados correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CrearEstudiante(Estudiantes estudiantes)
        {
            if (ModelState.IsValid)
            {
                estudiantes.GUID = Guid.NewGuid().ToString();
                estudiantes.Estado = true;
                db.Estudiantes.Add(estudiantes);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new ResponseRutasDTO
                    {
                        data = e.InnerException.InnerException.Message,
                        message = "Error al crear el estudiante",
                        status = HttpStatusCode.BadRequest,
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new ResponseRutasDTO
                {
                    data = estudiantes,
                    message = "Estudiante creado correctamente",
                    status = HttpStatusCode.OK,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new ResponseRutasDTO
            {
                data = null,
                message = "Error al crear el estudiante",
                status = HttpStatusCode.BadRequest,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetEstudiante(int id)
        {
            var estudiante = db.Estudiantes.Where(e => e.Id == id).Select(e => new
            {
                Id = e.Id,
                GUID = e.GUID,
                PrimerNombre = e.PrimerNombre,
                SegundoNombre = e.SegundoNombre,
                PrimerApellido = e.PrimerApellido,
                SegundoApellido = e.SegundoApellido,
                IdEstablecimiento = e.IdEstablecimientoEducativo,
                Establecimiento = e.EstablecimientosEducativos.Nombre,
                Estado = e.Estado
            }).FirstOrDefault();

            if (estudiante == null)
            {
                return Json(new ResponseRutasDTO
                {
                    data = null,
                    message = "Estudiante no encontrado",
                    status = HttpStatusCode.NotFound,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new ResponseRutasDTO
            {
                data = estudiante,
                message = "Estudiante cargado correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPatch]
        public JsonResult EditarEstudiante(int id, Estudiantes estudiantes)
        {
            // Comparar que parametros vienen vacios para no modificarlos
            var estudiante = db.Estudiantes.Find(estudiantes.Id);
            if (estudiante == null)
            {
                return Json(new ResponseRutasDTO
                {
                    data = null,
                    message = "Estudiante no encontrado",
                    status = HttpStatusCode.NotFound,
                }, JsonRequestBehavior.AllowGet);
            }

            estudiante.GUID = estudiantes.GUID ?? estudiante.GUID;
            estudiante.PrimerNombre = estudiantes.PrimerNombre ?? estudiante.PrimerNombre;
            estudiante.SegundoNombre = estudiantes.SegundoNombre ?? estudiante.SegundoNombre;
            estudiante.PrimerApellido = estudiantes.PrimerApellido ?? estudiante.PrimerApellido;
            estudiante.SegundoApellido = estudiantes.SegundoApellido ?? estudiante.SegundoApellido;
            if (estudiantes.IdEstablecimientoEducativo != 0)
                estudiante.IdEstablecimientoEducativo = estudiantes.IdEstablecimientoEducativo;
            estudiante.Estado = estudiantes.Estado;
            if (ModelState.IsValid)
            {
                db.Entry(estudiante).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new ResponseRutasDTO
                    {
                        data = e.InnerException.InnerException.Message,
                        message = "Error al editar el estudiante",
                        status = HttpStatusCode.BadRequest,
                    }, JsonRequestBehavior.AllowGet);
                }

                var estudianteRes = db.Estudiantes.Where(e => e.Id == id).Select(e => new
                {
                    e.Id,
                    e.GUID,
                    e.PrimerNombre,
                    e.SegundoNombre,
                    e.PrimerApellido,
                    e.SegundoApellido,
                    e.IdEstablecimientoEducativo,
                    e.EstablecimientosEducativos.Nombre,
                    e.Estado
                }).FirstOrDefault();

                return Json(new ResponseRutasDTO
                {
                    data = estudianteRes,
                    message = "Estudiante editado correctamente",
                    status = HttpStatusCode.OK,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new ResponseRutasDTO
            {
                data = null,
                message = "Error al editar el estudiante",
                status = HttpStatusCode.BadRequest,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEstudiantesByIdEstablecimiento(int id)
        {
            var estudiantes = db.Estudiantes.Where(e => e.IdEstablecimientoEducativo == id).Select(e => new
            {
                Id = e.Id,
                GUID = e.GUID,
                PrimerNombre = e.PrimerNombre,
                SegundoNombre = e.SegundoNombre,
                PrimerApellido = e.PrimerApellido,
                SegundoApellido = e.SegundoApellido,
                IdEstablecimiento = e.IdEstablecimientoEducativo,
                Establecimiento = e.EstablecimientosEducativos.Nombre,
                Estado = e.Estado
            }).ToList();

            return Json(new ResponseRutasDTO
            {
                data = estudiantes,
                message = "Estudiantes cargados correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetEstudiantesByUsuario(int id)
        {
            var estudiantes = db.UsuariosEstudiantes.Where(ue => ue.IdUsuario == id).Select(ue => new
            {
                Id = ue.Estudiantes.Id,
                GUID = ue.Estudiantes.GUID,
                PrimerNombre = ue.Estudiantes.PrimerNombre,
                SegundoNombre = ue.Estudiantes.SegundoNombre,
                PrimerApellido = ue.Estudiantes.PrimerApellido,
                SegundoApellido = ue.Estudiantes.SegundoApellido,
                IdEstablecimiento = ue.Estudiantes.IdEstablecimientoEducativo,
                Establecimiento = ue.Estudiantes.EstablecimientosEducativos.Nombre,
                Estado = ue.Estudiantes.Estado
            }).ToList();

            return Json(new ResponseRutasDTO
            {
                data = estudiantes,
                message = "Estudiantes cargados correctamente",
                status = HttpStatusCode.OK,
            }, JsonRequestBehavior.AllowGet);

        }

    }
}
