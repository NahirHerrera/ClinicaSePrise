namespace ClinicaSePrise.Entidades;

// Una liquidación de honorarios ya registrada
public class HonorarioLiquidado
{
    public string Profesional { get; set; } = "";
    public int Pacientes { get; set; }
    public decimal Facturado { get; set; }
    public decimal Monto { get; set; }

    public string Texto => string.Format(new System.Globalization.CultureInfo("es-AR"),
        "{0} · {1} pacientes · facturado $ {2:N0} · honorario $ {3:N0}", Profesional, Pacientes, Facturado, Monto);
}
