using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace ControlRutas.DTO
{
    public class UsuarioPiloto : UsuarioOB
    {
        private DateTime fechaHoy = DateTime.Now.Date;

        public MedioTransporteDTO Bus { get; set; }

        public List<HijosDTO> Hijos { get; set; }

        public bool EnRuta { get; set; }

        public new string RutaGuid { get; set; }

            public UsuarioPiloto(Usuarios usuario, ControlRutasEntities db) : base(usuario, db)
            {
                this.Bus = this.ObtenerBusDelPiloto(usuario, db);
                this.Hijos = new List<HijosDTO>();
                if (this.Bus == null)
                    return;
                List<EstudiantesMediosTransporte> mediosTransporteList = this.ObtenerEstudiantesAsignadosAlBus(this.Bus.Id, db);
                List<NoUsoRutasMediosTransporte> noUsoRutas = this.ObtenerNoUsoRutasHoy(db);
                foreach (EstudiantesMediosTransporte estudiante in mediosTransporteList)
                {
                    if (this.EstaAsignadoHoy(estudiante))
                        this.Hijos.Add(this.CrearHijoDTO(estudiante, db, noUsoRutas));
                }
                this.RutaGuid = this.GuidRutaActiva(this.Bus.Id, db);
                this.EnRuta = this.RutaGuid != "Sin Ruta";
            }
        private string GuidRutaActiva(int idBus, ControlRutasEntities db)
        {
            string guidRuta = db.RutasMediosTransporte
                .Where(rmt => rmt.EstudiantesMediosTransporte.IdMedioTransporte == idBus &&
                              rmt.Fecha == this.fechaHoy && // 'this.fechaHoy' debe ser accesible aquí
                              rmt.Estado == "Activa")
                .Select(rmt => rmt.GUID)
                .FirstOrDefault();

            return guidRuta ?? "Sin Ruta";
        }
        private MedioTransporteDTO ObtenerBusDelPiloto(Usuarios usuario, ControlRutasEntities db)
        {
            MedioTransporteDTO busDelPiloto = db.MediosTransporte // Accede directamente a la colección
                .Where(mt => mt.IdCodigoPiloto == usuario.Id) // Condición de filtro simplificada
                .Select(mt => new MedioTransporteDTO // Proyección a MedioTransporteDTO
                {
                    Id = mt.Id,
                    Placa = mt.Placa,
                    Identificador = mt.Identificador,
                    GUID = mt.GUID,
                    IdTipoMedioTransporte = mt.IdTipoMedioTransporte,
                    IdDueño = mt.IdCodigoDueño
                })
                .FirstOrDefault(); // Obtiene el primer bus que coincida o null si no hay ninguno.

            return busDelPiloto;
        }
        private List<EstudiantesMediosTransporte> ObtenerEstudiantesAsignadosAlBus(int busId, ControlRutasEntities db)
        {
            try
            {
                return db.EstudiantesMediosTransporte // Acceso directo a la colección
                    .Where(emt => emt.IdMedioTransporte == busId) // Condición de filtro simplificada
                    .ToList(); // El tipo se infiere automáticamente
            }
            catch (Exception ex)
            {
                // Es una buena práctica registrar la excepción 'ex' aquí para poder diagnosticar problemas.
                // Por ejemplo: log.Error("Error al obtener estudiantes asignados al bus.", ex);
                return new List<EstudiantesMediosTransporte>(); // Devuelve lista vacía en caso de error
            }
        }
        private List<NoUsoRutasMediosTransporte> ObtenerNoUsoRutasHoy(ControlRutasEntities db)
        {
            return db.NoUsoRutasMediosTransporte
                .Where(nurmt => nurmt.Fecha == this.fechaHoy)
                .ToList();
        }
        private bool EstaAsignadoHoy(EstudiantesMediosTransporte estudiante)
        {
            string str = DateTime.Now.DayOfWeek.ToString();
            if (str != null)
            {
                switch (str.Length)
                {
                    case 6:
                        switch (str[0])
                        {
                            case 'F':
                                if (str == "Friday")
                                    return estudiante.Viernes;
                                break;
                            case 'M':
                                if (str == "Monday")
                                    return estudiante.Lunes;
                                break;
                            case 'S':
                                if (str == "Sunday")
                                    return estudiante.Domingo;
                                break;
                        }
                        break;
                    case 7:
                        if (str == "Tuesday")
                            return estudiante.Martes;
                        break;
                    case 8:
                        switch (str[0])
                        {
                            case 'S':
                                if (str == "Saturday")
                                    return estudiante.Sabado;
                                break;
                            case 'T':
                                if (str == "Thursday")
                                    return estudiante.Jueves;
                                break;
                        }
                        break;
                    case 9:
                        if (str == "Wednesday")
                            return estudiante.Miercoles;
                        break;
                }
            }
            return false;
        }
        private HijosDTO CrearHijoDTO(EstudiantesMediosTransporte estudiante, ControlRutasEntities db, List<NoUsoRutasMediosTransporte> noUsoRutas)
        {
            return new HijosDTO()
            {
                Id = estudiante.Id,
                GUID = estudiante.Estudiantes.GUID,
                Nombre = $"{estudiante.Estudiantes.PrimerNombre} {estudiante.Estudiantes.SegundoNombre} {estudiante.Estudiantes.PrimerApellido} {estudiante.Estudiantes.SegundoApellido}",
                Estado = this.ObtenerEstadoEstudiante(estudiante, db, noUsoRutas),
                RutaGuid = this.ObtenerRutaGuid(estudiante, db),
                Colegio = this.ObtenerColegioEstudiante(estudiante, db),
                DireccionOrigen = estudiante.DireccionOrigen,
                LatitudOrigen = estudiante.LatitudOrigen,
                LongitudOrigen = estudiante.LongitudOrigen,
                DireccionDestino = estudiante.DireccionDestino,
                LatitudDestino = estudiante.LatitudDestino,
                LongitudDestino = estudiante.LongitudDestino,
                Orden = estudiante.Orden
            };
        }
        private string ObtenerEstadoEstudiante(EstudiantesMediosTransporte estudiante, ControlRutasEntities db, List<NoUsoRutasMediosTransporte> noUsoRutas)
        {
            string estadoRutaActual = db.RutasMediosTransporte
                .Where(rmt => rmt.IdEstudianteMedioTransporte == estudiante.Id &&
                              rmt.Fecha == this.fechaHoy) // 'this.fechaHoy' debe ser accesible
                .Select(rmt => rmt.Estado) // Solo necesitamos el estado
                .FirstOrDefault();

            if (estadoRutaActual == "Activa")
            {
                return "En Ruta";
            }

            if (estadoRutaActual == "Finalizada")
            {
                return "Entregado";
            }

            if (noUsoRutas.Any(noUso => noUso.EstudiantesMediosTransporte.IdEstudiante == estudiante.IdEstudiante))
            {
                return "Ausente";
            }
            else
            {
                return "Esperando";
            }
        }
        private string ObtenerRutaGuid(EstudiantesMediosTransporte estudiante, ControlRutasEntities db)
        {
            string rutaGuid = db.RutasMediosTransporte
                .Where(e => e.EstudiantesMediosTransporte.Estudiantes.Id == estudiante.Id && // Condición de filtro
                              e.Fecha == this.fechaHoy) // 'this.fechaHoy' debe ser accesible
                .Select(e => e.GUID) // Selecciona solo el GUID
                .FirstOrDefault(); // Obtiene el primer GUID o null

            return rutaGuid ?? "Sin Ruta"; // Devuelve "Sin Ruta" si es null
        }
        private string ObtenerColegioEstudiante(EstudiantesMediosTransporte estudiante, ControlRutasEntities db)
        {
            return db.UsuariosEstudiantes
                .Where(ue => ue.IdEstudiante == estudiante.IdEstudiante)
                .Select(ue => ue.Estudiantes.EstablecimientosEducativos.Nombre) // Asume que las navegaciones no son nulas
                .FirstOrDefault(); // Devuelve el nombre del colegio o null si no se encuentra
        }
    }
}