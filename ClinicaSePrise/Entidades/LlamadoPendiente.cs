using System.Globalization;
using ClinicaSePrise.Graficos;

namespace ClinicaSePrise.Entidades;

// Paciente al que Recepción tiene que llamar por teléfono
public class LlamadoPendiente
{
    public int IdTurno { get; set; }
    public string Motivo { get; set; } = "";   // "Confirmar turno" o "Faltó al turno"
    public DateTime FechaHora { get; set; }
    public string Paciente { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Profesional { get; set; } = "";
    public string Especialidad { get; set; } = "";
    public int Intentos { get; set; }
    public string UltimoResultado { get; set; } = "";

    public bool EsAusencia => Motivo == "Faltó al turno";
    public bool EsConfirmacion => !EsAusencia;

    public string FechaTxt => FechaHora.ToString("ddd dd/MM HH:mm", new CultureInfo("es-AR"));
    public string Detalle =>
        $"📞 {(Telefono == "" ? "sin teléfono" : Telefono)} · Turno {FechaTxt} · {Profesional} ({Especialidad})";
    public bool TieneIntentos => Intentos > 0;
    public string IntentosTxt =>
        $"Ya se llamó {Intentos} {(Intentos == 1 ? "vez" : "veces")} · último resultado: {UltimoResultado}";

    public Color ColorMotivo => ColoresEstado.Texto(EsAusencia ? "Ausente" : "Pendiente");
    public Color ColorFondoMotivo => ColoresEstado.Fondo(EsAusencia ? "Ausente" : "Pendiente");
}
