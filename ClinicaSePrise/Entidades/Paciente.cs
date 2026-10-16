using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    internal class Paciente
    {
        public int idPaciente { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string dni { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string modalidadCobertura { get; set; }
        public string obraSocial { get; set; }
        public string nroAfiliado { get; set; }
        public string nroHistoriaClinica { get; set; }

        public Paciente(int idPaciente, string nombre, string apellido, string dni, string telefono,
                        string email, string modalidadCobertura, string obraSocial, string nroAfiliado, string nroHistoriaClinica)
        {
            this.idPaciente = idPaciente;
            this.nombre = nombre;
            this.apellido = apellido;
            this.dni = dni;
            this.telefono = telefono;
            this.email = email;
            this.modalidadCobertura = modalidadCobertura;
            this.obraSocial = obraSocial;
            this.nroAfiliado = nroAfiliado;
            this.nroHistoriaClinica = nroHistoriaClinica;
        }
    }
}
