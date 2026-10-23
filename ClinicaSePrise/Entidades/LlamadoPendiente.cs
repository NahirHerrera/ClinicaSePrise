namespace ClinicaSePrise.Entidades;

// Turno pendiente que Recepción tiene que confirmar por teléfono
public class LlamadoPendiente
{
    public int IdTurno { get; set; }
    public DateTime FechaHora { get; set; }
    public string Paciente { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Profesional { get; set; } = "";

    public string FechaTxt => FechaHora.ToString("dd/MM HH:mm");
    public string Detalle => $"Tel. {Telefono} · Turno {FechaTxt} con {Profesional}";
}
