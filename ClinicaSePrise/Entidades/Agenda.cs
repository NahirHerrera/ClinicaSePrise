using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Agenda
    {
        public int idAgenda { get; set; }
        public int? idProfesional { get; set; }
        public string especialidad { get; set; }
        public string disponibilidad { get; set; }
        public int? duracionMinima { get; set; }
        public int? sobreturnosPermitidos { get; set; }
        public bool? recordatorio { get; set; }

        public Agenda(int idAgenda, int idProfesional, string especialidad, string disponibilidad,
                      int duracionMinima, int sobreturnosPermitidos, bool recordatorio)
        {
            this.idAgenda = idAgenda;
            this.idProfesional = idProfesional;
            this.especialidad = especialidad;
            this.disponibilidad = disponibilidad;
            this.duracionMinima = duracionMinima;
            this.sobreturnosPermitidos = sobreturnosPermitidos;
            this.recordatorio = recordatorio;
        }
    }
}
