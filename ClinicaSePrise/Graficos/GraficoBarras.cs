namespace ClinicaSePrise.Graficos;

// Gráfico de barras horizontales, dibujado a mano (sin librerías externas).
// Se usa con un GraphicsView: GraphicsView.Drawable = new GraficoBarras()
public class GraficoBarras : IDrawable
{
    public const float AltoFila = 34;

    public List<DatoGrafico> Datos { get; set; } = new();
    public Color ColorBarra { get; set; } = Color.FromArgb("#2563EB");
    public string MensajeVacio { get; set; } = "Sin datos en el período";

    static readonly Color ColorTexto = Color.FromArgb("#334155");
    static readonly Color ColorGris = Color.FromArgb("#94A3B8");
    static readonly Color ColorRiel = Color.FromArgb("#F1F5F9");

    public void Draw(ICanvas canvas, RectF r)
    {
        canvas.FontSize = 12;

        if (Datos.Count == 0)
        {
            canvas.FontColor = ColorGris;
            canvas.DrawString(MensajeVacio, r, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        float anchoEtiqueta = Math.Min(130, r.Width * 0.32f);
        float anchoValor = 90;
        float anchoBarras = Math.Max(10, r.Width - anchoEtiqueta - anchoValor);
        float altoBarra = 18;
        double maximo = Datos.Max(d => d.Valor);

        for (int i = 0; i < Datos.Count; i++)
        {
            var d = Datos[i];
            float y = r.Y + i * AltoFila;
            float yBarra = y + (AltoFila - altoBarra) / 2;
            float x = r.X + anchoEtiqueta;
            float ancho = maximo > 0 ? (float)(d.Valor / maximo) * anchoBarras : 0;

            canvas.FontColor = ColorTexto;
            canvas.DrawString(d.Etiqueta, r.X, y, anchoEtiqueta - 10, AltoFila,
                HorizontalAlignment.Right, VerticalAlignment.Center);

            canvas.FillColor = ColorRiel;
            canvas.FillRoundedRectangle(x, yBarra, anchoBarras, altoBarra, 4);

            if (ancho > 0)
            {
                canvas.FillColor = d.Color ?? ColorBarra;
                canvas.FillRoundedRectangle(x, yBarra, Math.Max(ancho, 6), altoBarra, 4);
            }

            canvas.FontColor = ColorTexto;
            canvas.DrawString(d.Texto ?? d.Valor.ToString("N0"), x + anchoBarras + 8, y, anchoValor - 8, AltoFila,
                HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }

    // Alto que necesita el GraphicsView para mostrar todas las filas
    public double AltoNecesario => Math.Max(60, Datos.Count * AltoFila);
}
