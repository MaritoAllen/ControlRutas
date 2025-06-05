using ControlRutas.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class UsuarioDueñoBuses : UsuarioOB
    {
        public List<MedioTransporteDTO> MedioTransportes { get; set; }
        public UsuarioDueñoBuses(Usuarios usuario, ControlRutasEntities db) : base(usuario, db)
        {
            this.MedioTransportes = db.MediosTransporte
                .Where(mt => mt.IdCodigoDueño == usuario.Id)
                .Select(mt => new MedioTransporteDTO
                {
                    Id = mt.Id,
                    Placa = mt.Placa,
                    Identificador = mt.Identificador,
                    Dueño = $"{mt.Usuarios.PrimerNombre} {mt.Usuarios.PrimerApellido}".Trim(),
                    GUID = mt.GUID,
                    IdTipoMedioTransporte = mt.IdTipoMedioTransporte,
                    TipoMedioTransporte = mt.TiposMediosTransporte.Nombre ?? "Sin Tipo"
                })
                .ToList();
        }
    }
}