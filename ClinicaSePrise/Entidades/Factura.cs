using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace ClinicaSePrise.Entidades
{
    public class Factura
    {
        public int idFactura { get; set; }
        public string nroComprobante { get; set; }
        public int idPaciente { get; set; }
        public int? idTurno { get; set; }
        public int? idTurnoEstudio { get; set; }
        public DateTime fechaEmision { get; set; }
        public string concepto { get; set; }
        public decimal monto { get; set; }
        public string medioPago { get; set; }
        public string estado { get; set; }

        public Factura(int idFactura, string nroComprobante, int idPaciente, int idTurno, int idTurnoEstudio,
                       DateTime fechaEmision, string concepto, decimal monto, string medioPago, string estado)
        {
            this.idFactura = idFactura;
            this.nroComprobante = nroComprobante;
            this.idPaciente = idPaciente;
            this.idTurno = idTurno;
            this.idTurnoEstudio = idTurnoEstudio;
            this.fechaEmision = fechaEmision;
            this.concepto = concepto;
            this.monto = monto;
            this.medioPago = medioPago;
            this.estado = estado;
        }
    }
}
