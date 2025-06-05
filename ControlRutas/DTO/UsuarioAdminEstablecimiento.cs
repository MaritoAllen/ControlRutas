using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class UsuarioAdminEstablecimiento : UsuarioOB
    {
        public List<HijosDTO> Estudiantes { get; set; }

        public List<MedioTransporteDTO> MedioTransportes { get; set; }

        public UsuarioAdminEstablecimiento(Usuarios usuario, ControlRutasEntities db) : base(usuario, db)
        {
            EstablecimientosEducativos establecimiento = usuario.UsuariosEstablecimientos
                                                             .FirstOrDefault(x => x.Activo)
                                                             ?.EstablecimientosEducativos; DateTime fechaHoy = DateTime.Now.Date;
            if (establecimiento != null)
            {
                var list1 = db.Estudiantes
                .Where(e => e.IdEstablecimientoEducativo == establecimiento.Id)
                .Select(e => new 
                {
                    Id = e.Id,
                    GUID = e.GUID,
                    PrimerNombre = e.PrimerNombre,
                    SegundoNombre = e.SegundoNombre,
                    PrimerApellido = e.PrimerApellido,
                    SegundoApellido = e.SegundoApellido,
                    EstablecimientoNombre = e.EstablecimientosEducativos.Nombre, // Accede a la navegación
                    EstablecimientoDireccion = e.EstablecimientosEducativos.Direccion, // Accede a la navegación
                    EstudiantesMedios = e.EstudiantesMediosTransporte.Select(emt => new // Proyección al tipo anónimo anidado
                    {
                        Id = emt.Id,
                        DireccionDestino = emt.DireccionDestino,
                        IdMedioTransporte = emt.IdMedioTransporte,
                        RutaHoy = emt.RutasMediosTransporte
                                    .FirstOrDefault(rmt => rmt.Fecha == fechaHoy) // Simplificado
                    })
                // No se necesita .ToList() aquí si EstudiantesMedios debe ser IEnumerable como en el tipo original
                })
                .ToList();

                // La parte derecha de la asignación simplificada:
                var list2 = db.MediosTransporte
                .Where(mt => mt.IdCodigoEstablecimiento == establecimiento.Id)
                .Select(mt => new // Proyección al tipo anónimo principal
                {
                    Id = mt.Id,
                    Placa = mt.Placa,
                    Identificador = mt.Identificador,
                    IdCodigoDueño = mt.IdCodigoDueño,
                    DueñoNombre = mt.Usuarios == null ?
                                        "Sin Dueño" :
                                        $"{mt.Usuarios.PrimerNombre} {mt.Usuarios.PrimerApellido}".Trim(),
                    GUID = mt.GUID,
                    IdTipoMedioTransporte = mt.IdTipoMedioTransporte,
                    TipoMedioTransporteNombre = mt.TiposMediosTransporte.Nombre ?? "Sin Tipo",
                    EstudiantesMedios = mt.EstudiantesMediosTransporte.Select(emt => new
                    {
                        Id = emt.Id,
                        EstudianteNombre = emt.Estudiantes == null ?
                                                "Sin Estudiante" :
                                                $"{emt.Estudiantes.PrimerNombre} {emt.Estudiantes.SegundoNombre} {emt.Estudiantes.PrimerApellido} {emt.Estudiantes.SegundoApellido}".Trim(),
                        ColegioNombre = emt.Estudiantes.EstablecimientosEducativos.Nombre ?? "Sin Colegio",
                        RutaHoy = emt.RutasMediosTransporte
                                        .FirstOrDefault(rmt => rmt.Fecha == fechaHoy)
                    })
                    // No se necesita .ToList() aquí si EstudiantesMedios debe ser IEnumerable como en el tipo original
                })
                .ToList(); // Materializa la lista de objetos anónimos principales
                this.Estudiantes = list1.Select(e =>
                {
                    // Obtenemos el primer elemento de EstudiantesMedios una sola vez
                    // para evitar múltiples llamadas y asegurar consistencia.
                    var primerMedioEstudiante = e.EstudiantesMedios.FirstOrDefault();

                    return new HijosDTO
                    {
                        Colegio = e.EstablecimientoNombre,
                        DireccionOrigen = e.EstablecimientoDireccion,
                        DireccionDestino = primerMedioEstudiante?.DireccionDestino ?? "Sin Destino",
                        Nombre = $"{e.PrimerNombre} {e.SegundoNombre} {e.PrimerApellido} {e.SegundoApellido}".Trim(), // Añadido .Trim()
                        RutaGuid = primerMedioEstudiante?.RutaHoy?.GUID ?? "Sin Ruta",
                        GUID = e.GUID,
                        Id = e.Id
                    };
                }).ToList();
                this.MedioTransportes = list2.Select(mt => new MedioTransporteDTO
                {
                    Placa = mt.Placa,
                    Identificador = mt.Identificador,
                    IdDueño = mt.IdCodigoDueño,
                    Dueño = mt.DueñoNombre,
                    GUID = mt.GUID, // Se asume que mt.GUID existe en el tipo anónimo de list2
                    IdTipoMedioTransporte = mt.IdTipoMedioTransporte,
                    TipoMedioTransporte = mt.TipoMedioTransporteNombre,
                    Id = mt.Id,
                    EstudiantesBus = mt.EstudiantesMedios.Select(emt => new HijosDTO
                    {
                        Id = emt.Id,
                        GUID = emt.Id.ToString(), // El GUID se genera a partir de Id.ToString()
                        Nombre = emt.EstudianteNombre,
                        Colegio = emt.ColegioNombre,
                        Estado = emt.RutaHoy != null ? "En Ruta" : "Esperando",
                        RutaGuid = emt.RutaHoy?.GUID ?? "Sin Ruta"
                    }).ToList(), // El tipo HijosDTO se infiere automáticamente
                    Estado = mt.EstudiantesMedios.Any(emt => emt.RutaHoy != null) ? "En Ruta" : "Esperando"
                }).ToList(); // El tipo MedioTransporteDTO se infiere automáticamente
            }
            else
            {
              this.Estudiantes = new List<HijosDTO>();
              this.MedioTransportes = new List<MedioTransporteDTO>();
            }
        }
    }
}