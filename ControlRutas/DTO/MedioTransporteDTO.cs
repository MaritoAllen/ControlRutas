using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class MedioTransporteDTO
    {
        public int Id { get; set; }

        public string Placa { get; set; }

        public string Identificador { get; set; }

        public string Dueño { get; set; }

        public string GUID { get; set; }

        public int IdTipoMedioTransporte { get; set; }

        public string TipoMedioTransporte { get; set; }

        public int IdDueño { get; set; }

        public int IdCodigoPiloto { get; set; }

        public string Piloto { get; set; }

        public string Estado { get; set; }

        public List<HijosDTO> EstudiantesBus { get; set; }
    }
}