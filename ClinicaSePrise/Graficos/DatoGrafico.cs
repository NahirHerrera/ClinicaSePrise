namespace ClinicaSePrise.Graficos;

// Un valor de un gráfico. Texto es lo que se muestra al lado (si no hay, se muestra el número)
public record DatoGrafico(string Etiqueta, double Valor, string? Texto = null);
