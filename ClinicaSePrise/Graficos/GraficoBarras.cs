namespace ClinicaSePrise.Graficos;

// Gráfico de barras horizontales, dibujado a mano (sin librerías externas).
// Se usa con un GraphicsView: GraphicsView.Drawable = new GraficoBarras()
public class GraficoBarras : IDrawable
{
    const float AltoFila = 30;

    public List<DatoGrafico> Datos { get; set; } = new();
    public Color ColorBarra { get; set; } = Colors.SteelBlue;

    public void Draw(ICanvas canvas, RectF r)
    {
        canvas.FontSize = 12;
        canvas.FontColor = Colors.DimGray;

        if (Datos.Count == 0)
        {
            canvas.DrawString("Sin datos este mes", r, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        float anchoEtiqueta = 90, anchoValor = 70;
        float anchoBarras = Math.Max(10, r.Width - anchoEtiqueta - anchoValor);
        double maximo = Datos.Max(d => d.Valor);

        for (int i = 0; i < Datos.Count; i++)
        {
            var d = Datos[i];
            float y = r.Y + i * AltoFila;
            float ancho = (float)(d.Valor / maximo) * anchoBarras;

            canvas.DrawString(d.Etiqueta, r.X, y, anchoEtiqueta - 8, AltoFila, HorizontalAlignment.Right, VerticalAlignment.Center);
            canvas.FillColor = ColorBarra;
            canvas.FillRectangle(r.X + anchoEtiqueta, y + 6, Math.Max(ancho, 4), AltoFila - 12);
            canvas.DrawString(d.Texto ?? d.Valor.ToString("N0"), r.X + anchoEtiqueta + ancho + 6, y, anchoValor, AltoFila,
                HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }

    // Alto que necesita el GraphicsView para mostrar todas las filas
    public double AltoNecesario => Math.Max(60, Datos.Count * AltoFila);
}
