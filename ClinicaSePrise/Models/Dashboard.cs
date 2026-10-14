using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicaSePrise.Models;

using System.Globalization;

public class Turno
{
    public string Hora { get; set; } = "";
    public string Duracion { get; set; } = "";
    public string Paciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Profesional { get; set; } = "";
    public string Estado { get; set; } = "";

}

public class Insumo
{
    public string Nombre { get; set; } = "";
    public string Sector { get; set; } = "";
    public int Stock { get; set; }
}

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