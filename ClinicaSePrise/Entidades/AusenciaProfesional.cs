namespace ClinicaSePrise.Entidades;

// Un día que faltó un médico
public class AusenciaProfesional
{
    public DateTime Fecha { get; set; }
    public string Profesional { get; set; } = "";
    public string Motivo { get; set; } = "";

    public string Texto => $"{Fecha:dd/MM} · {Profesional}{(Motivo == "" ? "" : " · " + Motivo)}";
}
