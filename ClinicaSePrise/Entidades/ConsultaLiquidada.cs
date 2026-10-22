using System.Globalization;
using ClinicaSePrise.Graficos;

namespace ClinicaSePrise.Entidades;

// Una consulta cobrada que entra en la liquidación de un médico
public class ConsultaLiquidada
{
    static readonly CultureInfo Ar = new("es-AR");

    public DateTime Fecha { get; set; }
    public string Paciente { get; set; } = "";
    public decimal Monto { get; set; }
    public string MedioPago { get; set; } = "";
    public string EstadoTurno { get; set; } = "";   // "Atendido" lo marca el médico en su pantalla

    public bool Atendida => EstadoTurno == "Atendido";
    public string FechaTxt => Fecha.ToString("ddd dd/MM HH:mm", Ar);
    public string MontoTxt => $"$ {Monto.ToString("N0", Ar)}";
    public string EstadoTxt => Atendida ? "Atendida" : $"Sin atender ({EstadoTurno.ToLower()})";
    public Color ColorEstado => ColoresEstado.Texto(Atendida ? "Atendido" : "Pendiente");
    public Color ColorFondoEstado => ColoresEstado.Fondo(Atendida ? "Atendido" : "Pendiente");
}
