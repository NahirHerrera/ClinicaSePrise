namespace ClinicaSePrise.Entidades;

public class TurnoImagen
{
    public int idTurno { get; set; }
    public string paciente { get; set; } = "";
    public string dni { get; set; } = "";
    public string estudio { get; set; } = "";
    public string sector { get; set; } = "";
    public DateTime fechaHora { get; set; }
    public string indicaciones { get; set; } = "";
    public string estado { get; set; } = "";
    public bool PuedeLlamar => estado != "Llamado";
    public bool PuedeAtender => estado == "Llamado";
    public string Fecha => fechaHora.ToString("dd/MM/yyyy HH:mm");
}