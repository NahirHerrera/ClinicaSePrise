using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace ClinicaSePrise.Entidades
{
    public class Comprobante
    {
        public int idComprobante { get; set; }
        public int? idPaciente { get; set; }
        public int? idEstudio { get; set; }
        public DateTime? fechaAtencion { get; set; }
        public bool? comprobanteRetiro { get; set; }

        public Comprobante(int idComprobante, int idPaciente, int idEstudio, DateTime? fechaAtencion, bool comprobanteRetiro)
        {
            this.idComprobante = idComprobante;
            this.idPaciente = idPaciente;
            this.idEstudio = idEstudio;
            this.fechaAtencion = fechaAtencion;
            this.comprobanteRetiro = comprobanteRetiro;
        }
    }
}
