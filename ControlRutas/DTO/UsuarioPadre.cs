using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace ControlRutas.DTO
{
    public class UsuarioPadre : UsuarioOB
    {
        public List<HijosDTO> Hijos { get; set; }

        public UsuarioPadre(Usuarios usuario, ControlRutasEntities db) : base(usuario, db)
        {
            DateTime fechaHoy = DateTime.Now.Date;
            List<UsuariosEstudiantes> list1 = db.UsuariosEstudiantes
                                                 .Where(ue => ue.Usuarios.Id == usuario.Id)
                                                 .ToList();
            List<NoUsoRutasMediosTransporte> list2 = db.NoUsoRutasMediosTransporte
                                                         .Where(nurmt => nurmt.Fecha == fechaHoy)
                                                         .ToList();
            this.Hijos = new List<HijosDTO>();

            var todosIdEstudiantesEnList1 = list1.Select(ue => ue.IdEstudiante).Distinct().ToList();

            Dictionary<int, EstudiantesMediosTransporte> emtMap = new Dictionary<int, EstudiantesMediosTransporte>();

            if (todosIdEstudiantesEnList1.Any())
            {
                emtMap = db.EstudiantesMediosTransporte
                    .Where(emt => todosIdEstudiantesEnList1.Contains(emt.IdEstudiante))
                    .GroupBy(emt => emt.IdEstudiante)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());
            }
            List<RutaInfoDTO> todasRutasHoyParaEstudiantes = new List<RutaInfoDTO>();

            if (todosIdEstudiantesEnList1.Any())
            {
                todasRutasHoyParaEstudiantes = db.RutasMediosTransporte
                    .Where(rmt => todosIdEstudiantesEnList1.Contains(rmt.EstudiantesMediosTransporte.IdEstudiante) && rmt.Fecha == fechaHoy)
                    .Select(rmt => new RutaInfoDTO // Un DTO simple para la información de ruta necesaria
                    {
                        IdEstudiante = rmt.EstudiantesMediosTransporte.IdEstudiante,
                        Estado = rmt.Estado,
                        GUID = rmt.GUID
                    })
                    .ToList();
            }
            var rutasAgrupadasPorEstudiante = todasRutasHoyParaEstudiantes
                .GroupBy(r => r.IdEstudiante)
                .ToDictionary(g => g.Key, g => g.ToList());

            var estudiantesQueNoAsistieronIds = list2
                .Select(noUso => noUso.EstudiantesMediosTransporte.IdEstudiante)
                .ToHashSet();

            foreach (UsuariosEstudiantes usuarioEstudiante in list1) // Usamos 'usuarioEstudiante' directamente
            {
                var estudiante = usuarioEstudiante.Estudiantes; // Alias para brevedad

                // Obtener datos de EstudiantesMediosTransporte del mapa pre-cargado
                emtMap.TryGetValue(usuarioEstudiante.IdEstudiante, out var emtData);

                // Determinar flags y RutaGuid a partir de los datos de ruta pre-cargados
                string rutaGuidParaDto = string.Empty;
                bool tieneRutaActiva = false;
                bool tieneAlgunaRutaHoy = false;
                bool tieneRutaFinalizada = false;

                if (rutasAgrupadasPorEstudiante.TryGetValue(usuarioEstudiante.IdEstudiante, out var rutasDelEstudianteHoy))
                {
                    tieneAlgunaRutaHoy = rutasDelEstudianteHoy.Any();
                    var rutaActiva = rutasDelEstudianteHoy.FirstOrDefault(r => r.Estado == "Activa");
                    if (rutaActiva != null)
                    {
                        tieneRutaActiva = true;
                        rutaGuidParaDto = rutaActiva.GUID ?? string.Empty;
                    }
                    tieneRutaFinalizada = rutasDelEstudianteHoy.Any(r => r.Estado == "Finalizada");
                }

                // Lógica simplificada para el Estado
                string estadoDelHijo;
                if (tieneRutaActiva)
                {
                    estadoDelHijo = "En Ruta";
                }
                else if (tieneRutaFinalizada)
                {
                    estadoDelHijo = "Entregado";
                }
                else if (tieneAlgunaRutaHoy) // Hubo ruta hoy pero no activa ni finalizada
                {
                    estadoDelHijo = "Esperando";
                }
                else // No hubo rutas para este estudiante hoy
                {
                    if (estudiantesQueNoAsistieronIds.Contains(usuarioEstudiante.IdEstudiante))
                    {
                        estadoDelHijo = "No Asistio";
                    }
                    else
                    {
                        estadoDelHijo = "Esperando";
                    }
                }

                var hijosDto = new HijosDTO
                {
                    Id = estudiante.Id,
                    GUID = estudiante.GUID,
                    Nombre = $"{estudiante.PrimerNombre} {estudiante.SegundoNombre} {estudiante.PrimerApellido} {estudiante.SegundoApellido}".Trim(),
                    Colegio = estudiante.EstablecimientosEducativos?.Nombre ?? "Sin Colegio", // Usar ?. para seguridad
                    DireccionOrigen = emtData?.DireccionOrigen ?? "Sin Origen",
                    LatitudOrigen = emtData?.LatitudOrigen ?? string.Empty,
                    LongitudOrigen = emtData?.LongitudOrigen ?? string.Empty,
                    DireccionDestino = emtData?.DireccionDestino ?? "Sin Destino",
                    LatitudDestino = emtData?.LatitudDestino ?? string.Empty,
                    LongitudDestino = emtData?.LongitudDestino ?? string.Empty,
                    Estado = estadoDelHijo,
                    RutaGuid = rutaGuidParaDto
                };

                this.Hijos.Add(hijosDto);
            }


            foreach (UsuariosEstudiantes usuariosEstudiantes in list1)
            {
                UsuariosEstudiantes hijo = usuariosEstudiantes;
                HijosDTO hijosDto1 = new HijosDTO();
                hijosDto1.Id = hijo.Estudiantes.Id;
                hijosDto1.GUID = hijo.Estudiantes.GUID;
                hijosDto1.Nombre = $"{hijo.Estudiantes.PrimerNombre} {hijo.Estudiantes.SegundoNombre} {hijo.Estudiantes.PrimerApellido} {hijo.Estudiantes.SegundoApellido}";
                hijosDto1.Colegio = hijo.Estudiantes.EstablecimientosEducativos.Nombre;
                hijosDto1.DireccionOrigen = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.DireccionOrigen)).FirstOrDefault<string>();
                hijosDto1.LatitudOrigen = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.LatitudOrigen)).FirstOrDefault<string>();
                hijosDto1.LongitudOrigen = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.LongitudOrigen)).FirstOrDefault<string>();
                hijosDto1.DireccionDestino = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.DireccionDestino)).FirstOrDefault<string>();
                hijosDto1.LatitudDestino = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.LatitudDestino)).FirstOrDefault<string>();
                hijosDto1.LongitudDestino = ((IQueryable<EstudiantesMediosTransporte>)db.EstudiantesMediosTransporte).Where<EstudiantesMediosTransporte>((Expression<Func<EstudiantesMediosTransporte, bool>>)(emt => emt.IdEstudiante == hijo.IdEstudiante)).Select<EstudiantesMediosTransporte, string>((Expression<Func<EstudiantesMediosTransporte, string>>)(rmt => rmt.LongitudDestino)).FirstOrDefault<string>();
                bool flag1 = ((IQueryable<RutasMediosTransporte>)db.RutasMediosTransporte).Any<RutasMediosTransporte>((Expression<Func<RutasMediosTransporte, bool>>)(e => e.EstudiantesMediosTransporte.IdEstudiante == hijo.IdEstudiante && e.Fecha == fechaHoy && e.Estado == "Activa"));
                bool flag2 = ((IQueryable<RutasMediosTransporte>)db.RutasMediosTransporte).Any<RutasMediosTransporte>((Expression<Func<RutasMediosTransporte, bool>>)(e => e.EstudiantesMediosTransporte.IdEstudiante == hijo.IdEstudiante && e.Fecha == fechaHoy));
                bool flag3 = ((IQueryable<RutasMediosTransporte>)db.RutasMediosTransporte).Any<RutasMediosTransporte>((Expression<Func<RutasMediosTransporte, bool>>)(e => e.EstudiantesMediosTransporte.IdEstudiante == hijo.IdEstudiante && e.Fecha == fechaHoy && e.Estado == "Finalizada"));
                hijosDto1.Estado = !flag1 ? (!flag3 ? (!flag2 ? (list2.Any<NoUsoRutasMediosTransporte>((Func<NoUsoRutasMediosTransporte, bool>)(e => e.EstudiantesMediosTransporte.IdEstudiante == hijo.IdEstudiante)) ? "No Asistio" : "Esperando") : "Esperando") : "Entregado") : "En Ruta";
                HijosDTO hijosDto2 = hijosDto1;
                string str;
                if (!flag1)
                    str = string.Empty;
                else
                    str = ((IQueryable<RutasMediosTransporte>)db.RutasMediosTransporte).Where<RutasMediosTransporte>((Expression<Func<RutasMediosTransporte, bool>>)(e => e.EstudiantesMediosTransporte.Estudiantes.Id == hijo.IdEstudiante && e.Fecha == fechaHoy && e.Estado == "Activa")).Select<RutasMediosTransporte, string>((Expression<Func<RutasMediosTransporte, string>>)(e => e.GUID)).FirstOrDefault<string>() ?? string.Empty;
                hijosDto2.RutaGuid = str;
                this.Hijos.Add(hijosDto1);
            }
        }
    }
}