using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace ControlRutas.DTO
{
    public class UsuarioAdmin : UsuarioOB
    {
        public List<UsuarioOB> Usuarios { get; set; }

        public List<HijosDTO> Estudiantes { get; set; }

        public List<MedioTransporteDTO> MedioTransportes { get; set; }

        public UsuarioAdmin(Usuarios usuario, ControlRutasEntities db) : base(usuario, db)
        {
            DateTime fechaHoy = DateTime.Now.Date;
            List<EstablecimientosEducativos> listaEstablecimientos = db.EstablecimientosEducativos.ToList();
            this.Usuarios = db.Usuarios.Where(u => u.Id != usuario.Id).Select(u => new UsuarioOB(u, db)).ToList();
            List<EstudiantesDTO> estudiantesMedios = db.EstudiantesMediosTransporte.Include(emt => emt.Estudiantes).Select(emt => new EstudiantesDTO
            {
                Id = emt.Id, // Id del EstudiantesMediosTransporte
                IdEstudiante = emt.IdEstudiante,
                DireccionDestino = emt.DireccionDestino,
                IdMedioTransporte = emt.IdMedioTransporte,
                GUID = emt.Estudiantes.GUID ?? "Sin GUID", // Accede a Estudiantes (navegación)
                Nombre = emt.Estudiantes == null ?
                                "Estudiante no disponible" :
                                $"{emt.Estudiantes.PrimerNombre} {emt.Estudiantes.SegundoNombre} {emt.Estudiantes.PrimerApellido} {emt.Estudiantes.SegundoApellido}".Trim()
            })
            .ToList();
            List<ControlRutas.Data.Estudiantes> list = db.Estudiantes.Include(e => e.EstablecimientosEducativos).ToList();
            List<RutasMediosTransporte> rutasHoy = db.RutasMediosTransporte.Where(rmt => rmt.Fecha == fechaHoy).ToList();
            this.Estudiantes = list.Select(e =>
            {
                // Busca el EstudiantesDTO correspondiente una sola vez por cada estudiante 'e'
                var estudianteMedioDto = estudiantesMedios.FirstOrDefault(dto => dto.IdEstudiante == e.Id);

                return new HijosDTO
                {
                    Colegio = e.EstablecimientosEducativos?.Nombre ?? "Sin Colegio",
                    DireccionOrigen = e.EstablecimientosEducativos?.Direccion ?? "Sin Dirección",
                    DireccionDestino = estudianteMedioDto?.DireccionDestino ?? "Sin Destino",
                    Nombre = $"{e.PrimerNombre} {e.SegundoNombre} {e.PrimerApellido} {e.SegundoApellido}".Trim(),
                    RutaGuid = rutasHoy.FirstOrDefault(rmt =>
                                   estudianteMedioDto != null && // Solo busca si se encontró el DTO
                                   rmt.IdEstudianteMedioTransporte == estudianteMedioDto.Id
                               )?.GUID ?? "Sin Ruta",
                    GUID = e.GUID,
                    Id = e.Id
                };
            }).ToList();

            // --- PASO 1: Obtener la lista principal de MediosTransporte ---
            var mediosTransporteList = db.MediosTransporte
                .Include(mt => mt.TiposMediosTransporte) // Para TiposMediosTransporte.Nombre
                .ToList();

            // Si no hay medios de transporte, no hay nada más que hacer
            if (!mediosTransporteList.Any())
            {
                this.MedioTransportes = new List<MedioTransporteDTO>();
                // Considera retornar o asignar la lista vacía aquí si es apropiado
            }
            else
            {
                // --- PASO 2: Pre-cargar Dueños (Usuarios) ---
                var idsDeDueños = mediosTransporteList
                    .Select(mt => mt.IdCodigoDueño)
                    .Distinct()
                    .ToList();

                Dictionary<int?, ControlRutas.Data.Usuarios> dueñosMap = new Dictionary<int?, ControlRutas.Data.Usuarios>();
                if (idsDeDueños.Any())
                {
                    dueñosMap = db.Usuarios
                        .Where(u => idsDeDueños.Contains(u.Id)) // Asumiendo que u.Id es compatible con IdCodigoDueño
                        .ToDictionary(u => (int?)u.Id); // La Key del diccionario debe coincidir con el tipo de IdCodigoDueño
                }

                // --- PASO 3: Pre-cargar Información de Colegios (via UsuariosEstudiantes) ---
                // Asumimos que 'estudiantesMedios' es una List<EstudiantesDTO> ya en memoria.
                // Y EstudiantesDTO tiene: Id (para rutasHoy), IdEstudiante (para UsuariosEstudiantes), IdMedioTransporte.
                var idsDeEstudiantesParaColegios = estudiantesMedios
                    .Select(estDto => estDto.IdEstudiante) // IdEstudiante es la FK a UsuariosEstudiantes.Estudiantes
                    .Distinct()
                    .ToList();

                Dictionary<int, UsuariosEstudiantes> usuariosEstudiantesMap = new Dictionary<int, UsuariosEstudiantes>();
                if (idsDeEstudiantesParaColegios.Any())
                {
                    usuariosEstudiantesMap = db.UsuariosEstudiantes
                        .Where(ue => idsDeEstudiantesParaColegios.Contains(ue.IdEstudiante))
                        .Include(ue => ue.Estudiantes.EstablecimientosEducativos)
                        .ToDictionary(ue => ue.IdEstudiante);
                }

                this.MedioTransportes = mediosTransporteList.Select(mt =>
                {
                    dueñosMap.TryGetValue(mt.IdCodigoDueño, out var dueñoInfo);

                    var estudiantesEnEsteMedioDto = estudiantesMedios
                        .Where(estDto => estDto.IdMedioTransporte == mt.Id)
                        .ToList();

                    return new MedioTransporteDTO
                    {
                        Placa = mt.Placa,
                        Identificador = mt.Identificador,
                        IdDueño = mt.IdCodigoDueño,
                        Dueño = dueñoInfo != null ? $"{dueñoInfo.PrimerNombre} {dueñoInfo.PrimerApellido}".Trim() : "Dueño no especificado",
                        GUID = mt.GUID,
                        IdTipoMedioTransporte = mt.IdTipoMedioTransporte,
                        TipoMedioTransporte = mt.TiposMediosTransporte?.Nombre ?? "Sin Tipo",
                        Id = mt.Id,
                        EstudiantesBus = estudiantesEnEsteMedioDto.Select(estDto =>
                        {
                            // Buscar info del colegio en el mapa pre-cargado
                            usuariosEstudiantesMap.TryGetValue(estDto.IdEstudiante, out var ueInfo);

                            // Buscar ruta en la lista en memoria 'rutasHoy'
                            var rutaEstudiante = rutasHoy
                                .FirstOrDefault(r => r.IdEstudianteMedioTransporte == estDto.Id); // Asegúrate que estDto.Id es la clave correcta para rutasHoy

                            return new HijosDTO
                            {
                                Id = estDto.Id, // ID del EstudiantesDTO o el que necesites para HijosDTO
                                GUID = estDto.GUID, // Asumiendo que EstudiantesDTO tiene GUID
                                Nombre = estDto.Nombre, // Asumiendo que EstudiantesDTO tiene Nombre
                                Colegio = ueInfo?.Estudiantes?.EstablecimientosEducativos?.Nombre ?? "Sin Colegio",
                                Estado = rutaEstudiante != null ? "En Ruta" : "Esperando",
                                RutaGuid = rutaEstudiante?.GUID ?? "Sin Ruta"
                            };
                        }).ToList(),
                        // El estado del MedioTransporte se calcula en memoria
                        Estado = rutasHoy.Any(rmt =>
                            estudiantesEnEsteMedioDto.Any(x => rmt.IdEstudianteMedioTransporte == x.Id) // Asegúrate que x.Id es la clave correcta
                        ) ? "En Ruta" : "Esperando"
                    };
                }).ToList();
            }
        }
    }
}