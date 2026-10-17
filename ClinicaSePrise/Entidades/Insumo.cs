using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades
{
    public class Insumo
    {
        public int idInsumo { get; set; }
        public string nombre { get; set; }
        public int? idEstudio { get; set; }
        public int? stock { get; set; }
        public int stockMinimo { get; set; }
        public string estadoReposicion { get; set; }

        public Insumo(int idInsumo, string nombre, int? idEstudio, int? stock, int stockMinimo, string estadoReposicion)
        {
            this.idInsumo = idInsumo;
            this.nombre = nombre;
            this.idEstudio = idEstudio;
            this.stock = stock;
            this.stockMinimo = stockMinimo;
            this.estadoReposicion = estadoReposicion;
        }
    }
}
