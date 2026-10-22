using ClinicaSePrise.Graficos;

namespace ClinicaSePrise.Entidades;

// Turno del día tal como lo ve Recepción
public class TurnoRecepcion
{
    public int IdTurno { get; set; }
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = "";
    public string Paciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Cobertura { get; set; } = "";
    public string ObraSocial { get; set; } = "";
    public string Profesional { get; set; } = "";
    public string Especialidad { get; set; } = "";
    public string Consultorio { get; set; } = "";
    public string EstadoCobertura { get; set; } = "";   // Particular / Al día / Con deuda / Sin convenio

    public string Hora => FechaHora.ToString("HH:mm");
    public string DniTxt => $"DNI {Dni}";
    public string Lugar => $"{Especialidad} · {Consultorio}";
    public string CoberturaTxt => ObraSocial == "" ? Cobertura : $"{Cobertura} – {ObraSocial}";

    // Se acredita cuando el paciente llega; se marca ausente si ya pasó la hora y no vino
    public bool PuedeAcreditar => Estado is "Pendiente" or "Confirmado";
    public bool PuedeMarcarAusente => PuedeAcreditar && FechaHora < DateTime.Now;

    public Color ColorEstado => ColoresEstado.Texto(Estado);
    public Color ColorFondoEstado => ColoresEstado.Fondo(Estado);

    // Obra social: solo se le factura si está al día con la clínica; si no, el paciente paga como particular
    public bool VienePorObraSocial => EstadoCobertura != "Particular";
    public bool CobraComoParticular => EstadoCobertura != "Al día";
    public string AvisoCobertura => EstadoCobertura switch
    {
        "Al día" => $"{ObraSocial} está al día: se factura a la obra social y el paciente paga solo el copago.",
        "Con deuda" => $"{ObraSocial} tiene deuda con la clínica: el paciente paga como particular (consulta completa).",
        "Sin convenio" => (ObraSocial == "" ? "No tiene la obra social cargada" : $"{ObraSocial} no tiene convenio con la clínica") +
                          ": el paciente paga como particular (consulta completa).",
        _ => "Particular: el paciente paga la consulta completa."
    };
    public Color ColorAvisoCobertura => ColoresEstado.Texto(EstadoCobertura == "Al día" ? "Atendido" : VienePorObraSocial ? "Ausente" : "");
    public Color ColorFondoAvisoCobertura => ColoresEstado.Fondo(EstadoCobertura == "Al día" ? "Atendido" : VienePorObraSocial ? "Ausente" : "");
}
