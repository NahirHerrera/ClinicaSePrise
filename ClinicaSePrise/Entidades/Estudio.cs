using System;

namespace ClinicaSePrise.Entidades
{
    public class Estudio
    {
        public int idEstudio { get; set; }
        public string nombre { get; set; }
        public string especialidad { get; set; }
        public string requisitosPrevios { get; set; }
        public string horarioEspecifico { get; set; }
        public string responsable { get; set; }

        // Constructor vacío necesario para mapeos rápidos
        public Estudio() { }

        public Estudio(int idEstudio, string nombre, string especialidad, string requisitosPrevios, string horarioEspecifico, string responsable)
        {
            this.idEstudio = idEstudio;
            this.nombre = nombre;
            this.especialidad = especialidad;
            this.requisitosPrevios = requisitosPrevios;
            this.horarioEspecifico = horarioEspecifico;
            this.responsable = responsable;
        }

        // CLAVE: Esto evita que el desplegable se vea en blanco en Windows
        public override string ToString() => nombre;
    }
}