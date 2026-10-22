using ClinicaSePrise.Graficos;

namespace ClinicaSePrise.Entidades;

// Obra social con convenio: si tiene deuda con la clínica, sus pacientes pagan como particulares
public class ObraSocialConvenio
{
    public int IdObraSocial { get; set; }
    public string Nombre { get; set; } = "";
    public bool AlDia { get; set; }
    public int Pacientes { get; set; }

    public string PacientesTxt => Pacientes == 1 ? "1 paciente" : $"{Pacientes} pacientes";
    public string EstadoTxt => AlDia ? "Al día" : "Con deuda";
    public string BotonTxt => AlDia ? "Marcar con deuda" : "Marcar al día";
    public Color ColorEstado => ColoresEstado.Texto(AlDia ? "Atendido" : "Ausente");
    public Color ColorFondoEstado => ColoresEstado.Fondo(AlDia ? "Atendido" : "Ausente");
}
