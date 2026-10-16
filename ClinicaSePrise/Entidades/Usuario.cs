using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Usuario
    {
        public int idUsuario { get; set; }
        public string usuario { get; set; }
        public string contrasenia { get; set; }
        public int idRol { get; set; }
        public int? idPaciente { get; set; }
        public int? idProfesional { get; set; }

        public Usuario(int idUsuario, string usuario, string contrasenia, int idRol, int? idPaciente, int? idProfesional)
        {
            this.idUsuario = idUsuario;
            this.usuario = usuario;
            this.contrasenia = contrasenia;
            this.idRol = idRol;
            this.idPaciente = idPaciente;
            this.idProfesional = idProfesional;
        }
    }
}
