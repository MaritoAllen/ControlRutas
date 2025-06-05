using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class HijosDTO
    {
        public int Id { get; set; }

        public string GUID { get; set; }

        public string Nombre { get; set; }

        public string Colegio { get; set; }

        public string Estado { get; set; }

        public string RutaGuid { get; set; }

        public string LongitudOrigen { get; set; }

        public string LatitudOrigen { get; set; }

        public string DireccionOrigen { get; set; }

        public string LongitudDestino { get; set; }

        public string LatitudDestino { get; set; }

        public string DireccionDestino { get; set; }

        public int Orden { get; set; }
    }
}