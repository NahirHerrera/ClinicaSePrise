namespace ClinicaSePrise.Graficos;

// Gráfico de dona con su leyenda, dibujado a mano (sin librerías externas)
public class GraficoDona : IDrawable
{
    public List<DatoGrafico> Datos { get; set; } = new();
    public string MensajeVacio { get; set; } = "Sin datos en el período";

    static readonly Color ColorTexto = Color.FromArgb("#334155");
    static readonly Color ColorGris = Color.FromArgb("#94A3B8");

    public void Draw(ICanvas canvas, RectF r)
    {
        double total = Datos.Sum(d => d.Valor);
        canvas.FontSize = 12;

        if (total <= 0)
        {
            canvas.FontColor = ColorGris;
            canvas.DrawString(MensajeVacio, r, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        float grosor = 26;
        float diametro = Math.Min(r.Height, r.Width * 0.45f) - grosor;
        float x = r.X + grosor / 2;
        float y = r.Y + (r.Height - diametro) / 2;

        canvas.StrokeSize = grosor;
        canvas.StrokeLineCap = LineCap.Butt;

        // Arranca arriba (90°) y va en el sentido de las agujas del reloj
        float inicio = 90;
        foreach (var d in Datos.Where(d => d.Valor > 0))
        {
            float barrido = (float)(d.Valor / total * 360);
            canvas.StrokeColor = d.Color ?? ColorGris;
            if (barrido >= 359.9f)
                canvas.DrawEllipse(x, y, diametro, diametro);
            else
                canvas.DrawArc(x, y, diametro, diametro, inicio, inicio - barrido, true, false);
            inicio -= barrido;
        }

        // Total en el centro
        canvas.FontColor = ColorTexto;
        canvas.FontSize = 20;
        canvas.DrawString(total.ToString("N0"), x, y + diametro / 2 - 18, diametro, 24,
            HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.FontSize = 11;
        canvas.FontColor = ColorGris;
        canvas.DrawString("turnos", x, y + diametro / 2 + 4, diametro, 16,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        // Leyenda a la derecha
        float xLeyenda = x + diametro + grosor / 2 + 24;
        float altoFila = 22;
        float yLeyenda = r.Y + (r.Height - Datos.Count * altoFila) / 2;
        canvas.FontSize = 12;
        foreach (var d in Datos)
        {
            canvas.FillColor = d.Color ?? ColorGris;
            canvas.FillRoundedRectangle(xLeyenda, yLeyenda + 5, 12, 12, 3);
            canvas.FontColor = ColorTexto;
            canvas.DrawString($"{d.Etiqueta}: {d.Valor:N0} ({d.Valor / total:P0})",
                xLeyenda + 20, yLeyenda, r.Right - xLeyenda - 20, altoFila,
                HorizontalAlignment.Left, VerticalAlignment.Center);
            yLeyenda += altoFila;
        }
    }
}
