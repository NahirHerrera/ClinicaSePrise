namespace ClinicaSePrise.Entidades;

// Una consulta cobrada que entra en la liquidación de un médico
public class ConsultaLiquidada
{
    public DateTime Fecha { get; set; }
    public string Paciente { get; set; } = "";
    public decimal Monto { get; set; }
    public string EstadoTurno { get; set; } = "";   // "Atendido" lo marca el médico en su pantalla

    public string Texto =>
        $"{Fecha:dd/MM} · {Paciente} · $ {Monto.ToString("N0", new System.Globalization.CultureInfo("es-AR"))} · " +
        (EstadoTurno == "Atendido" ? "atendida" : "sin atender");
}
