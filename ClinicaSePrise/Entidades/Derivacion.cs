using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    internal class Derivacion
    {
        public int IdDerivacion { get; set; }
        public int? IdPaciente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Origen { get; set; }
        public int? IdEstudio { get; set; }
        public string EstudioNombre { get; set; }
        public string Estado { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";

        public Derivacion() { }
        public Derivacion(int idDerivacion, int? idPaciente, string origen, int? idEstudio, string estado, DateTime fecha)
        {
            IdDerivacion = idDerivacion;
            IdPaciente = idPaciente;
            Origen = origen;
            IdEstudio = idEstudio;
            Estado = estado;
        }
    }
}
