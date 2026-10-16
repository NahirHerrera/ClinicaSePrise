using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Entidades;

using System.Globalization;

public class Dashboard
{
    public string Usuario { get; set; } = "";
    public string Fecha => DateTime.Now.ToString("dddd, d 'de' MMMM", new CultureInfo("es-AR"));
    public int TurnosHoy { get; set; }
    public int Atendidos { get; set; }
    public int EnEspera { get; set; }
    public int InsumosCriticos { get; set; }
    public List<Turno> Turnos { get; set; } = new();
    public List<Insumo> Stock { get; set; } = new();
    public string Rol { get; set; } = "";
    public List<string> Pantallas { get; set; } = new();
}