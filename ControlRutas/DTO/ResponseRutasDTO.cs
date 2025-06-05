using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ControlRutas.DTO
{
    public class ResponseRutasDTO
    {
        public HttpStatusCode status { get; set; }

        public string message { get; set; }

        public object data { get; set; }
    }
}