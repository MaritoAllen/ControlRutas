using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlRutas.DTO
{
    public class RutaTransporteDTO
    {
        public string GUID { get; set; }

        public int Orden { get; set; }

        public string DireccionOrigen { get; set; }

        public string LatitudOrigen { get; set; }

        public string LongitudOrigen { get; set; }

        public string DireccionDestino { get; set; }

        public string LatitudDestino { get; set; }

        public string LongitudDestino { get; set; }

        public int IdEstudianteMedioTransporte { get; set; }

        public DateTime? HoraLlegada { get; set; }

        public DateTime? HoraSalida { get; set; }

        public DateTime Fecha { get; set; }

        public string Grado { get; set; }

        public int Id { get; set; }

        public int IdEstudiante { get; set; }

        public string EstudianteGUID { get; set; }

        public string NombreEstudiante { get; set; }
    }
}