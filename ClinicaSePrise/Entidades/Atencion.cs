using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace ClinicaSePrise.Entidades
{
    public class Atencion
    {
        public int idAtencion { get; set; }
        public int? idPaciente { get; set; }
        public int? idProfesional { get; set; } 
        public DateTime? fechaHoraFin { get; set; }
        public bool? indicacionNuevoTurno { get; set; }

        public Atencion(int idAtencion, int idPaciente, int idProfesional, DateTime fechaHoraFin, bool indicacionNuevoTurno)
        {
            this.idAtencion = idAtencion;
            this.idPaciente = idPaciente;
            this.idProfesional = idProfesional;
            this.fechaHoraFin = fechaHoraFin;
            this.indicacionNuevoTurno = indicacionNuevoTurno;
        }
    }
}
