namespace ClinicaSePrise.Entidades;

// Turno del día tal como lo ve Recepción
public class TurnoRecepcion
{
    public int IdTurno { get; set; }
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = "";
    public string Paciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Cobertura { get; set; } = "";
    public string ObraSocial { get; set; } = "";
    public string Profesional { get; set; } = "";
    public string Especialidad { get; set; } = "";
    public string Consultorio { get; set; } = "";

    public string Hora => FechaHora.ToString("HH:mm");
    public string Lugar => $"{Especialidad} · {Consultorio}";
    public bool VienePorObraSocial => Cobertura == "Obra Social";

    // Se acredita cuando el paciente llega; se marca ausente si ya pasó la hora y no vino
    public bool PuedeAcreditar => Estado is "Pendiente" or "Confirmado";
    public bool PuedeMarcarAusente => PuedeAcreditar && FechaHora < DateTime.Now;
}
