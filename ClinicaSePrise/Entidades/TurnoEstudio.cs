using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace ClinicaSePrise.Entidades
{
    public class TurnoEstudio
    {
        public int idTurno { get; set; }
        public int? idPaciente { get; set; }
        public int? idEstudio { get; set; }
        public DateTime? fechaHora { get; set; }
        public string modalidadSolicitud { get; set; }
        public bool? requiereTurno { get; set; }

        public TurnoEstudio(int idTurno, int idPaciente, int idEstudio, DateTime fechaHora, string modalidadSolicitud, bool requiereTurno)
        {
            this.idTurno = idTurno;
            this.idPaciente = idPaciente;
            this.idEstudio = idEstudio;
            this.fechaHora = fechaHora;
            this.modalidadSolicitud = modalidadSolicitud;
            this.requiereTurno = requiereTurno;
        }
    }
}
