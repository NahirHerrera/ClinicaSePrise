using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class AgendaEstudio
    {
        public int idAgenda { get; set; }
        public string sector { get; set; }
        public string disponibilidad { get; set; }
        public int? duracionEstimada { get; set; }
        public bool? recordatorio { get; set; }

        public AgendaEstudio(int idAgenda, string sector, string disponibilidad, int duracionEstimada, bool recordatorio)
        {
            this.idAgenda = idAgenda;
            this.sector = sector;
            this.disponibilidad = disponibilidad;
            this.duracionEstimada = duracionEstimada;
            this.recordatorio = recordatorio;
        }
    }
}
