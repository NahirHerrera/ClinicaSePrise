using System.Globalization;

namespace ClinicaSePrise.Entidades;

// Un día que faltó un médico
public class AusenciaProfesional
{
    public DateTime Fecha { get; set; }
    public string Profesional { get; set; } = "";
    public string Motivo { get; set; } = "";

    public string FechaTxt => Fecha.ToString("ddd dd/MM", new CultureInfo("es-AR"));
    public string MotivoTxt => Motivo == "" ? "Sin motivo" : Motivo;
}
