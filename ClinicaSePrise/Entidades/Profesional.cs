using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Profesional
    {
        public int idProfesional { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string nroMatricula { get; set; }
        public string especialidad { get; set; }
        public string consultorioAsignado { get; set; }
        public string insumosRequeridos { get; set; }

        public Profesional(int idProfesional, string nombre, string apellido, string nroMatricula,string especialidad, string consultorioAsignado, string insumosRequeridos)
        {
            this.idProfesional = idProfesional;
            this.nombre = nombre;
            this.apellido = apellido;
            this.nroMatricula = nroMatricula;
            this.especialidad = especialidad;
            this.consultorioAsignado = consultorioAsignado;
            this.insumosRequeridos = insumosRequeridos;
        }
    }
}
