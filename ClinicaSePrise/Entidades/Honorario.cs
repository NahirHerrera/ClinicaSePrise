using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Honorario
    {
        public int idHonorario { get; set; }
        public int? idProfesional { get; set; }
        public string periodoLiquidacion { get; set; }
        public int? cantidadPacientes { get; set; }
        public decimal? monto { get; set; }

        public Honorario(int idHonorario, int idProfesional, string periodoLiquidacion, int cantidadPacientes, decimal monto)
        {
            this.idHonorario = idHonorario;
            this.idProfesional = idProfesional;
            this.periodoLiquidacion = periodoLiquidacion;
            this.cantidadPacientes = cantidadPacientes;
            this.monto = monto;
        }
    }
}
