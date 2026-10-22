namespace ClinicaSePrise.Entidades;

// Números de un médico en un mes (para los gráficos de Administración)
public class EstadisticaProfesional
{
    public int IdProfesional { get; set; }
    public string Nombre { get; set; } = "";
    public int Turnos { get; set; }       // turnos asignados (sin cancelados)
    public int Atendidos { get; set; }    // atenciones registradas
    public int Ausentes { get; set; }     // pacientes que no vinieron
    public decimal Facturado { get; set; }
    public int Faltas { get; set; }       // días que faltó el médico
}
