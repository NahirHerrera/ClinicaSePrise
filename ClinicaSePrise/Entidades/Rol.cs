using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Rol
    {
        public int idRol { get; set; }
        public string nombre { get; set; }

        public Rol(int idRol, string nombre)
        {
            this.idRol = idRol;
            this.nombre = nombre;
        }
    }
}