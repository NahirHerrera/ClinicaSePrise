using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace ClinicaSePrise.Entidades
{
    public class Turno
    {
        public int idTurno { get; set; }
        public int? idPaciente { get; set; }
        public int? idProfesional { get; set; }
        public string especialidad { get; set; }
        public DateTime? fechaHora { get; set; }
        public string tipoTurno { get; set; }
        public int? duracion { get; set; }
        public string modalidadSolicitud { get; set; }
        public string estado { get; set; }

        public Turno(int idTurno, int? idPaciente, int? idProfesional, string especialidad, DateTime? fechaHora,
                    string tipoTurno, int? duracion, string modalidadSolicitud, string estado)
        {
            this.idTurno = idTurno;
            this.idPaciente = idPaciente;
            this.idProfesional = idProfesional;
            this.especialidad = especialidad;
            this.fechaHora = fechaHora;
            this.tipoTurno = tipoTurno;
            this.duracion = duracion;
            this.modalidadSolicitud = modalidadSolicitud;
            this.estado = estado;
        }
    }
}

