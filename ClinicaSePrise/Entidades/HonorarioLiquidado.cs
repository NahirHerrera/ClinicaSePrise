using System.Globalization;

namespace ClinicaSePrise.Entidades;

// Una liquidación de honorarios ya registrada
public class HonorarioLiquidado
{
    static readonly CultureInfo Ar = new("es-AR");

    public string Profesional { get; set; } = "";
    public int Pacientes { get; set; }
    public decimal? Facturado { get; set; }    // vacío en las liquidaciones cargadas antes del cálculo por %
    public decimal? Porcentaje { get; set; }
    public decimal Monto { get; set; }

    public string FacturadoTxt => Facturado is decimal f ? $"$ {f.ToString("N0", Ar)}" : "—";
    public string PorcentajeTxt => Porcentaje is decimal p ? $"{p.ToString("0.##", Ar)}%" : "—";
    public string MontoTxt => $"$ {Monto.ToString("N0", Ar)}";
}
