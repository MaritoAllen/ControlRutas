using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class EstudiantesDTO
    {
        public int Id { get; set; }

        public string DireccionDestino { get; set; }

        public int IdEstudiante { get; set; }

        public int IdMedioTransporte { get; set; }

        public string GUID { get; set; }

        public string Nombre { get; set; }
    }
}