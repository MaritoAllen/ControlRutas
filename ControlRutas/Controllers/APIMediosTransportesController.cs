using ControlRutas.Data;
using ControlRutas.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlRutas.Controllers
{
    public class APIMediosTransportesController : Controller
    {
        private ControlRutasEntities db = new ControlRutasEntities();

        [HttpGet]
        public async Task<JsonResult> GetMediosTransporte()
        {
            try
            {
                // Usar 'var' para simplificar la declaración de la lista con tipos anónimos.
                // Incluir explícitamente las entidades relacionadas para evitar N+1 y NullReferenceExceptions.
                var list = await db.MediosTransporte
                    .Include(e => e.TiposMediosTransporte)     // Carga previa para TiposMediosTransporte.Nombre
                    .Include(e => e.EstablecimientosEducativos) // Carga previa para EstablecimientosEducativos.Nombre
                    .Select(e => new // Proyección a un tipo anónimo
                    {
                        Id = e.Id,
                        GUID = e.GUID,
                        Placa = e.Placa,
                        Identificador = e.Identificador,
                        Color = e.Color,
                        IdCodigoDueño = e.IdCodigoDueño,
                        IdCodigoPiloto = e.IdCodigoPiloto,
                        IdTipoMedioTransporte = e.IdTipoMedioTransporte,
                        Tipo = e.TiposMediosTransporte != null ? e.TiposMediosTransporte.Nombre : "Sin Tipo", // Acceso seguro
                        Establecimiento = e.EstablecimientosEducativos != null ? e.EstablecimientosEducativos.Nombre : "Sin Establecimiento", // Acceso seguro
                        Estado = e.Estado
                    })
                    .ToListAsync(); // Usar ToListAsync() para operaciones asíncronas

                var response = new ResponseRutasDTO
                {
                    data = list, // 'data' es de tipo object, 'list' es List<anonymous_type>
                    message = "Medios de Transporte cargados correctamente",
                    status = HttpStatusCode.OK
                };
                // Para .NET Framework MVC, JsonRequestBehavior.AllowGet es necesario para peticiones GET.
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Es MUY importante registrar la excepción 'ex' en tu sistema de logging.
                // logger.LogError(ex, "Error en GetMediosTransporte");

                var errorResponse = new ResponseRutasDTO
                {
                    data = null,
                    message = "Error al cargar los medios de transporte. " + ex.Message, // Considera un mensaje más genérico en producción
                    status = HttpStatusCode.InternalServerError
                };
                // En .NET Framework, si quieres asegurar el código de estado HTTP en la respuesta:
                // HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(errorResponse, JsonRequestBehavior.AllowGet); // Es común devolver AllowGet también para errores en GET
            }
        }
        // GET: APIMediosTransportes
        public ActionResult Index()
        {
            return View();
        }

        // GET: APIMediosTransportes/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: APIMediosTransportes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: APIMediosTransportes/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: APIMediosTransportes/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: APIMediosTransportes/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: APIMediosTransportes/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: APIMediosTransportes/Delete/5
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
    }
}
